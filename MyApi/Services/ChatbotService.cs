using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;
using System.Text;
using System.Text.Json;

namespace MyApi.Services;

public class ChatbotService : IChatbotService
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ChatbotService> _logger;
    private const string OpenAiApiUrl = "https://api.openai.com/v1/chat/completions";
    private const int MaxHistoryMessages = 10;
    private const int MaxTokensPerDay = 10000;

    public ChatbotService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<ChatbotService> logger)
    {
        _context = context;
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;

        _httpClient = new HttpClient();
        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }
    }

    public async Task<string> ProcessMessageAsync(string userId, string message)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return "Chat service is not configured. Please contact support.";
        }

        try
        {
            // Check rate limiting
            var dailyTokenUsage = await GetDailyTokenUsageAsync(userId);
            if (dailyTokenUsage > MaxTokensPerDay)
            {
                return "You've reached your daily chat limit. Please try again tomorrow.";
            }

            // Save user message
            var userMessage = new ChatMessage
            {
                UserId = userId,
                Role = "user",
                Content = message
            };
            _context.ChatMessages.Add(userMessage);
            await _context.SaveChangesAsync();

            // Get user's receipts for context
            var receiptsContext = await GetReceiptsContextAsync(userId);

            // Get recent conversation history
            var history = await GetConversationHistoryAsync(userId, MaxHistoryMessages);

            // Build messages for OpenAI API
            var messages = new List<object>
            {
                new { role = "system", content = BuildSystemPrompt(receiptsContext) }
            };

            // Add conversation history (oldest to newest, excluding current message)
            foreach (var (role, content, _) in history.OrderBy(h => h.CreatedAt).TakeLast(MaxHistoryMessages - 1))
            {
                messages.Add(new { role, content });
            }

            // Create the request payload
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages,
                max_tokens = 500,
                temperature = 0.7
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(OpenAiApiUrl, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return "I'm having trouble processing your request right now. Please try again later.";
            }

            var jsonResponse = JsonDocument.Parse(responseContent);
            var assistantResponse = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "I couldn't generate a response.";

            var totalTokens = jsonResponse.RootElement
                .GetProperty("usage")
                .GetProperty("total_tokens")
                .GetInt32();

            // Save assistant response
            var assistantMessage = new ChatMessage
            {
                UserId = userId,
                Role = "assistant",
                Content = assistantResponse,
                TokenCount = totalTokens
            };
            _context.ChatMessages.Add(assistantMessage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Chatbot processed message for user {UserId}, tokens: {Tokens}",
                userId, totalTokens);

            return assistantResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chatbot message for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<(string Role, string Content, DateTime CreatedAt)>> GetConversationHistoryAsync(
        string userId, int limit = 50)
    {
        return await _context.ChatMessages
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .Select(m => new ValueTuple<string, string, DateTime>(m.Role, m.Content, m.CreatedAt))
            .ToListAsync();
    }

    public async Task ClearConversationHistoryAsync(string userId)
    {
        var messages = await _context.ChatMessages
            .Where(m => m.UserId == userId)
            .ToListAsync();

        _context.ChatMessages.RemoveRange(messages);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleared conversation history for user {UserId}", userId);
    }

    public List<string> GetSuggestedQuestions()
    {
        return new List<string>
        {
            "What receipts do I have?",
            "Show me receipts from last month",
            "How much have I spent on electronics?",
            "Which warranties are expiring soon?",
            "What's my total spending this year?",
            "Show me receipts from Amazon",
            "Do I have any warranties expiring this month?",
            "What's the most expensive item I bought?",
            "List all receipts with warranties",
            "How many receipts do I have?"
        };
    }

    private async Task<string> GetReceiptsContextAsync(string userId)
    {
        var receipts = await _context.Receipts
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.PurchaseDate ?? r.UploadedAt)
            .Take(100)
            .Select(r => new
            {
                r.FileName,
                r.Merchant,
                r.Amount,
                r.PurchaseDate,
                r.ProductName,
                r.WarrantyMonths,
                r.WarrantyExpirationDate,
                r.Description,
                r.Notes
            })
            .ToListAsync();

        var sharedReceipts = await _context.ReceiptShares
            .Include(rs => rs.Receipt)
            .Where(rs => rs.SharedWithUserId == userId)
            .Select(rs => new
            {
                rs.Receipt.FileName,
                rs.Receipt.Merchant,
                rs.Receipt.Amount,
                rs.Receipt.PurchaseDate,
                rs.Receipt.ProductName,
                rs.Receipt.WarrantyMonths,
                rs.Receipt.WarrantyExpirationDate,
                IsShared = true
            })
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("User's Receipts:");
        
        if (!receipts.Any() && !sharedReceipts.Any())
        {
            sb.AppendLine("No receipts found.");
        }
        else
        {
            foreach (var receipt in receipts)
            {
                sb.AppendLine($"- {receipt.FileName}");
                if (!string.IsNullOrEmpty(receipt.Merchant))
                    sb.AppendLine($"  Merchant: {receipt.Merchant}");
                if (receipt.Amount.HasValue)
                    sb.AppendLine($"  Amount: ${receipt.Amount:F2}");
                if (receipt.PurchaseDate.HasValue)
                    sb.AppendLine($"  Purchase Date: {receipt.PurchaseDate:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(receipt.ProductName))
                    sb.AppendLine($"  Product: {receipt.ProductName}");
                if (receipt.WarrantyMonths.HasValue)
                    sb.AppendLine($"  Warranty: {receipt.WarrantyMonths} months");
                if (receipt.WarrantyExpirationDate.HasValue)
                    sb.AppendLine($"  Warranty Expires: {receipt.WarrantyExpirationDate:yyyy-MM-dd}");
            }

            if (sharedReceipts.Any())
            {
                sb.AppendLine("\nShared with me:");
                foreach (var receipt in sharedReceipts)
                {
                    sb.AppendLine($"- {receipt.FileName} (shared)");
                    if (!string.IsNullOrEmpty(receipt.Merchant))
                        sb.AppendLine($"  Merchant: {receipt.Merchant}");
                    if (receipt.Amount.HasValue)
                        sb.AppendLine($"  Amount: ${receipt.Amount:F2}");
                }
            }
        }

        return sb.ToString();
    }

    private string BuildSystemPrompt(string receiptsContext)
    {
        return $@"You are a helpful AI assistant for a warranty and receipt management application. 
Your role is to help users query their receipts, track warranties, and analyze spending patterns.

Available receipt data:
{receiptsContext}

Guidelines:
- Answer questions about receipts, warranties, merchants, spending, and dates
- Parse natural language dates (""last month"", ""this year"", ""last week"")
- Provide summaries and statistics when asked
- Be concise and friendly
- If no matching receipts are found, politely inform the user
- Suggest related queries that might be helpful
- For warranty questions, prioritize items expiring soon
- Include specific details like amounts, dates, and merchant names when relevant

Current date: {DateTime.UtcNow:yyyy-MM-dd}";
    }

    private async Task<int> GetDailyTokenUsageAsync(string userId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.ChatMessages
            .Where(m => m.UserId == userId && m.CreatedAt >= today && m.TokenCount.HasValue)
            .SumAsync(m => m.TokenCount ?? 0);
    }
}
