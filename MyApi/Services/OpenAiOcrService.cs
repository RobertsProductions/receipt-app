using System.Text;
using System.Text.Json;

namespace MyApi.Services;

public class OpenAiOcrService : IOcrService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<OpenAiOcrService> _logger;
    private const string OpenAiApiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAiOcrService(IConfiguration configuration, ILogger<OpenAiOcrService> logger)
    {
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("OpenAI API key not configured. OCR features will not work. Set it in user secrets with: dotnet user-secrets set \"OpenAI:ApiKey\" \"your-key\"");
        }
        else
        {
            _logger.LogInformation("OpenAI OCR service initialized successfully");
        }
        
        _httpClient = new HttpClient();
        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }
    }

    public async Task<OcrResult> ExtractReceiptDataAsync(Stream imageStream, string fileName)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogError("Cannot perform OCR: OpenAI API key is not configured");
            return new OcrResult
            {
                Success = false,
                ErrorMessage = "OpenAI API key is not configured"
            };
        }

        try
        {
            _logger.LogInformation("Starting OCR processing for file: {FileName}", fileName);
            // Convert image to base64
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();
            var base64Image = Convert.ToBase64String(imageBytes);
            
            // Determine image type
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var mimeType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "image/jpeg"
            };

            // Create the request payload for GPT-4 Vision
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "text",
                                text = @"Extract the following information from this receipt image and return ONLY a valid JSON object with these exact fields (use null for missing values):
{
  ""merchant"": ""store or merchant name"",
  ""amount"": 0.00,
  ""purchaseDate"": ""YYYY-MM-DD"",
  ""productName"": ""main product or category"",
  ""extractedText"": ""brief summary of what you see""
}

Important: Return ONLY the JSON object, no additional text or explanation."
                            },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = $"data:{mimeType};base64,{base64Image}"
                                }
                            }
                        }
                    }
                },
                max_tokens = 500,
                temperature = 0.1
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending request to OpenAI API");
            var response = await _httpClient.PostAsync(OpenAiApiUrl, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return new OcrResult
                {
                    Success = false,
                    ErrorMessage = $"OpenAI API error: {response.StatusCode}"
                };
            }

            _logger.LogDebug("Received response from OpenAI API, parsing results");

            // Parse OpenAI response
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            
            if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                _logger.LogWarning("No choices in OpenAI response");
                return new OcrResult { Success = false, ErrorMessage = "No response from OpenAI" };
            }

            var messageContent = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                return new OcrResult { Success = false, ErrorMessage = "Empty response from OpenAI" };
            }

            // Parse the extracted data from GPT response
            var extractedData = ParseExtractedData(messageContent);
            extractedData.Success = true;
            
            _logger.LogInformation("Successfully extracted receipt data: Merchant={Merchant}, Amount={Amount}", 
                extractedData.Merchant, extractedData.Amount);

            return extractedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OCR processing for file {FileName}", fileName);
            return new OcrResult
            {
                Success = false,
                ErrorMessage = $"OCR processing failed: {ex.Message}"
            };
        }
    }

    private OcrResult ParseExtractedData(string jsonResponse)
    {
        try
        {
            // Clean up the response - remove markdown code blocks if present
            var cleanJson = jsonResponse.Trim();
            if (cleanJson.StartsWith("```json"))
            {
                cleanJson = cleanJson.Substring(7);
            }
            if (cleanJson.StartsWith("```"))
            {
                cleanJson = cleanJson.Substring(3);
            }
            if (cleanJson.EndsWith("```"))
            {
                cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
            }
            cleanJson = cleanJson.Trim();

            using var document = JsonDocument.Parse(cleanJson);
            var root = document.RootElement;

            var result = new OcrResult();

            if (root.TryGetProperty("merchant", out var merchant) && merchant.ValueKind != JsonValueKind.Null)
            {
                result.Merchant = merchant.GetString();
            }

            if (root.TryGetProperty("amount", out var amount) && amount.ValueKind == JsonValueKind.Number)
            {
                result.Amount = amount.GetDecimal();
            }

            if (root.TryGetProperty("purchaseDate", out var purchaseDate) && 
                purchaseDate.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(purchaseDate.GetString(), out var date))
            {
                result.PurchaseDate = date;
            }

            if (root.TryGetProperty("productName", out var productName) && productName.ValueKind != JsonValueKind.Null)
            {
                result.ProductName = productName.GetString();
            }

            if (root.TryGetProperty("extractedText", out var extractedText) && extractedText.ValueKind != JsonValueKind.Null)
            {
                result.ExtractedText = extractedText.GetString();
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse OCR JSON response: {Response}", jsonResponse);
            return new OcrResult
            {
                ExtractedText = jsonResponse,
                ErrorMessage = "Failed to parse structured data, but raw text is available"
            };
        }
    }
}
