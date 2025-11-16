using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyApi.Data;
using MyApi.Models;
using MyApi.Services;
using Xunit;

namespace MyApi.Tests.Services;

public class ChatbotServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<ChatbotService>> _mockLogger;
    private readonly ApplicationDbContext _context;
    private readonly string _testUserId = "test-user-id";

    public ChatbotServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<ChatbotService>>();

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ProcessMessageAsync_WithoutApiKey_ReturnsConfigurationMessage()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns((string)null);
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.ProcessMessageAsync(_testUserId, "Hello");

        // Assert
        result.Should().Contain("not configured");
        result.Should().Contain("contact support");
    }

    [Fact]
    public async Task ProcessMessageAsync_WithEmptyApiKey_ReturnsConfigurationMessage()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns(string.Empty);
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.ProcessMessageAsync(_testUserId, "Hello");

        // Assert
        result.Should().Contain("not configured");
    }

    [Fact]
    public async Task GetConversationHistoryAsync_WithNoMessages_ReturnsEmptyList()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.GetConversationHistoryAsync(_testUserId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetConversationHistoryAsync_WithMessages_ReturnsOrderedByDate()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        var message1 = new ChatMessage
        {
            UserId = _testUserId,
            Role = "user",
            Content = "First message",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        var message2 = new ChatMessage
        {
            UserId = _testUserId,
            Role = "assistant",
            Content = "First response",
            CreatedAt = DateTime.UtcNow.AddMinutes(-9)
        };
        var message3 = new ChatMessage
        {
            UserId = _testUserId,
            Role = "user",
            Content = "Second message",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _context.ChatMessages.AddRange(message1, message2, message3);
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetConversationHistoryAsync(_testUserId);

        // Assert
        result.Should().HaveCount(3);
        result[0].Content.Should().Be("Second message"); // Most recent first
        result[1].Content.Should().Be("First response");
        result[2].Content.Should().Be("First message");
    }

    [Fact]
    public async Task GetConversationHistoryAsync_WithLimit_ReturnsLimitedResults()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        for (int i = 0; i < 20; i++)
        {
            _context.ChatMessages.Add(new ChatMessage
            {
                UserId = _testUserId,
                Role = i % 2 == 0 ? "user" : "assistant",
                Content = $"Message {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetConversationHistoryAsync(_testUserId, limit: 5);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetConversationHistoryAsync_OnlyReturnsCurrentUserMessages()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        var userMessage = new ChatMessage
        {
            UserId = _testUserId,
            Role = "user",
            Content = "User message",
            CreatedAt = DateTime.UtcNow
        };
        var otherUserMessage = new ChatMessage
        {
            UserId = "other-user-id",
            Role = "user",
            Content = "Other user message",
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatMessages.AddRange(userMessage, otherUserMessage);
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetConversationHistoryAsync(_testUserId);

        // Assert
        result.Should().HaveCount(1);
        result[0].Content.Should().Be("User message");
    }

    [Fact]
    public async Task ClearConversationHistoryAsync_RemovesAllUserMessages()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        _context.ChatMessages.AddRange(
            new ChatMessage { UserId = _testUserId, Role = "user", Content = "Message 1" },
            new ChatMessage { UserId = _testUserId, Role = "assistant", Content = "Response 1" },
            new ChatMessage { UserId = "other-user", Role = "user", Content = "Other message" }
        );
        await _context.SaveChangesAsync();

        // Act
        await service.ClearConversationHistoryAsync(_testUserId);

        // Assert
        var remaining = await _context.ChatMessages.Where(m => m.UserId == _testUserId).ToListAsync();
        remaining.Should().BeEmpty();

        var otherUserMessages = await _context.ChatMessages.Where(m => m.UserId == "other-user").ToListAsync();
        otherUserMessages.Should().HaveCount(1); // Other user's messages should remain
    }

    [Fact]
    public void GetSuggestedQuestions_ReturnsListOfQuestions()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = service.GetSuggestedQuestions();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(q => q.Contains("receipts"));
        result.Should().Contain(q => q.Contains("warranties") || q.Contains("warranty"));
        result.Should().Contain(q => q.Contains("spent") || q.Contains("spending"));
    }

    [Fact]
    public void GetSuggestedQuestions_ReturnsConsistentResults()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result1 = service.GetSuggestedQuestions();
        var result2 = service.GetSuggestedQuestions();

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public async Task ProcessMessageAsync_SavesUserMessageToDatabase()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("sk-test-key-12345");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);
        var message = "What receipts do I have?";

        // Note: This will fail to call OpenAI (no real API), but should still save user message
        try
        {
            // Act
            await service.ProcessMessageAsync(_testUserId, message);
        }
        catch
        {
            // Expected to fail on API call
        }

        // Assert
        var savedMessage = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.UserId == _testUserId && m.Role == "user");
        
        savedMessage.Should().NotBeNull();
        savedMessage!.Content.Should().Be(message);
    }

    [Fact]
    public async Task GetConversationHistoryAsync_ReturnsCorrectRoles()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        _context.ChatMessages.AddRange(
            new ChatMessage { UserId = _testUserId, Role = "user", Content = "Question", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new ChatMessage { UserId = _testUserId, Role = "assistant", Content = "Answer", CreatedAt = DateTime.UtcNow.AddMinutes(-1) }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetConversationHistoryAsync(_testUserId);

        // Assert
        result.Should().HaveCount(2);
        result[0].Role.Should().Be("assistant");
        result[1].Role.Should().Be("user");
    }

    [Fact]
    public async Task GetConversationHistoryAsync_ReturnsMessagesWithTimestamps()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        var timestamp = DateTime.UtcNow.AddMinutes(-5);
        _context.ChatMessages.Add(new ChatMessage
        {
            UserId = _testUserId,
            Role = "user",
            Content = "Test message",
            CreatedAt = timestamp
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetConversationHistoryAsync(_testUserId);

        // Assert
        result.Should().HaveCount(1);
        result[0].CreatedAt.Should().BeCloseTo(timestamp, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ClearConversationHistoryAsync_WithNoMessages_DoesNotThrow()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var act = async () => await service.ClearConversationHistoryAsync(_testUserId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetConversationHistoryAsync_WithZeroLimit_ReturnsEmptyList()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        _context.ChatMessages.Add(new ChatMessage
        {
            UserId = _testUserId,
            Role = "user",
            Content = "Test",
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await service.GetConversationHistoryAsync(_testUserId, limit: 0);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetSuggestedQuestions_ReturnsAtLeastFiveQuestions()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = service.GetSuggestedQuestions();

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void GetSuggestedQuestions_AllQuestionsAreNonEmpty()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("test-key");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = service.GetSuggestedQuestions();

        // Assert
        result.Should().AllSatisfy(question => question.Should().NotBeNullOrWhiteSpace());
    }

    [Fact]
    public async Task ProcessMessageAsync_WithWhitespaceApiKey_ReturnsConfigurationMessage()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["OpenAI:ApiKey"]).Returns("   ");
        var service = new ChatbotService(_context, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.ProcessMessageAsync(_testUserId, "Test");

        // Assert
        result.Should().Contain("not configured");
    }
}
