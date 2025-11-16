using System.ComponentModel.DataAnnotations;

namespace MyApi.DTOs;

/// <summary>
/// Request to send a message to the chatbot
/// </summary>
public class SendChatMessageDto
{
    /// <summary>
    /// The user's message/question
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Response from the chatbot
/// </summary>
public class ChatMessageResponseDto
{
    public Guid MessageId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Chat conversation history
/// </summary>
public class ChatHistoryDto
{
    public List<ChatMessageResponseDto> Messages { get; set; } = new();
    public int TotalMessages { get; set; }
}

/// <summary>
/// Suggested questions to help users get started
/// </summary>
public class SuggestedQuestionsDto
{
    public List<string> Questions { get; set; } = new();
}
