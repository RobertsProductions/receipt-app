namespace MyApi.Services;

/// <summary>
/// Service for AI-powered chatbot to query receipts and warranties
/// </summary>
public interface IChatbotService
{
    /// <summary>
    /// Processes a user message and generates an AI response
    /// </summary>
    Task<string> ProcessMessageAsync(string userId, string message);

    /// <summary>
    /// Gets the conversation history for a user
    /// </summary>
    Task<List<(string Role, string Content, DateTime CreatedAt)>> GetConversationHistoryAsync(string userId, int limit = 50);

    /// <summary>
    /// Clears the conversation history for a user
    /// </summary>
    Task ClearConversationHistoryAsync(string userId);

    /// <summary>
    /// Gets suggested questions to help users get started
    /// </summary>
    List<string> GetSuggestedQuestions();
}
