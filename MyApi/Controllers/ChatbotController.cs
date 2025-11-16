using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.DTOs;
using MyApi.Services;
using System.Security.Claims;

namespace MyApi.Controllers;

/// <summary>
/// AI-powered chatbot for querying receipts, warranties, and spending patterns
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;
    private readonly ILogger<ChatbotController> _logger;

    public ChatbotController(
        IChatbotService chatbotService,
        ILogger<ChatbotController> logger)
    {
        _chatbotService = chatbotService;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    /// <summary>
    /// Sends a message to the chatbot and receives an AI-generated response
    /// </summary>
    /// <param name="dto">Message content</param>
    /// <returns>AI assistant response</returns>
    [HttpPost("message")]
    [ProducesResponseType(typeof(ChatMessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ChatMessageResponseDto>> SendMessage([FromBody] SendChatMessageDto dto)
    {
        var userId = GetUserId();

        try
        {
            var response = await _chatbotService.ProcessMessageAsync(userId, dto.Message);

            if (response.Contains("daily chat limit"))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, 
                    new { message = response });
            }

            return Ok(new ChatMessageResponseDto
            {
                MessageId = Guid.NewGuid(),
                Role = "assistant",
                Content = response,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chatbot message for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while processing your message" });
        }
    }

    /// <summary>
    /// Retrieves the conversation history for the current user
    /// </summary>
    /// <param name="limit">Maximum number of messages to retrieve (default: 50, max: 100)</param>
    /// <returns>Chat conversation history</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ChatHistoryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChatHistoryDto>> GetHistory([FromQuery] int limit = 50)
    {
        var userId = GetUserId();

        if (limit < 1 || limit > 100)
        {
            limit = 50;
        }

        var history = await _chatbotService.GetConversationHistoryAsync(userId, limit);

        var messages = history
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageResponseDto
            {
                MessageId = Guid.NewGuid(),
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToList();

        return Ok(new ChatHistoryDto
        {
            Messages = messages,
            TotalMessages = messages.Count
        });
    }

    /// <summary>
    /// Clears the conversation history for the current user
    /// </summary>
    /// <returns>Success message</returns>
    [HttpDelete("history")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearHistory()
    {
        var userId = GetUserId();
        await _chatbotService.ClearConversationHistoryAsync(userId);

        _logger.LogInformation("Cleared chat history for user {UserId}", userId);

        return NoContent();
    }

    /// <summary>
    /// Gets suggested questions to help users get started with the chatbot
    /// </summary>
    /// <returns>List of suggested questions</returns>
    [HttpGet("suggested-questions")]
    [ResponseCache(Duration = 3600)] // Cache for 1 hour (static content)
    [ProducesResponseType(typeof(SuggestedQuestionsDto), StatusCodes.Status200OK)]
    public ActionResult<SuggestedQuestionsDto> GetSuggestedQuestions()
    {
        var questions = _chatbotService.GetSuggestedQuestions();

        return Ok(new SuggestedQuestionsDto
        {
            Questions = questions
        });
    }
}
