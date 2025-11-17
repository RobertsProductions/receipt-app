# AI Chatbot for Receipt Queries

**Created**: 2025-11-16  
**Status**: Active  
**Feature**: AI-powered natural language interface for querying receipts, warranties, and spending patterns

## Overview

The AI Chatbot provides an intelligent, conversational interface for users to query their receipt data, track warranties, and analyze spending patterns using natural language. Built with OpenAI's GPT-4o-mini model, it understands context, maintains conversation history, and provides accurate, helpful responses based on the user's actual receipt data.

## Features

### Core Capabilities

1. **Natural Language Queries**
   - Ask questions in plain English
   - No need to learn complex query syntax
   - Contextual understanding of user intent

2. **Receipt Search & Discovery**
   - Find receipts by merchant, product, date range
   - Search by amount, category, or description
   - Access both owned and shared receipts

3. **Warranty Management**
   - Check which warranties are expiring soon
   - View warranty status for specific products
   - Get reminders about warranty expirations

4. **Spending Analysis**
   - Total spending by time period (month, year)
   - Spending breakdown by merchant or category
   - Most expensive purchases
   - Budget tracking and insights

5. **Conversation History**
   - Maintains context across messages
   - Refer back to previous answers
   - Progressive refinement of queries

6. **Smart Suggestions**
   - Pre-built questions to get started
   - Related query recommendations
   - Context-aware follow-up suggestions

## Architecture

### Components

```
┌─────────────────────────────────────────┐
│         ChatbotController               │
│  - POST /api/chatbot/message            │
│  - GET /api/chatbot/history             │
│  - DELETE /api/chatbot/history          │
│  - GET /api/chatbot/suggested-questions │
└─────────────────┬───────────────────────┘
                  │
                  ↓
┌─────────────────────────────────────────┐
│         ChatbotService                  │
│  - Process user messages                │
│  - Query receipt database               │
│  - Maintain conversation context        │
│  - Call OpenAI API                      │
│  - Rate limiting (10K tokens/day)       │
└─────────────────┬───────────────────────┘
                  │
                  ├──→ ApplicationDbContext
                  │    (Receipts, ReceiptShares, ChatMessages)
                  │
                  └──→ OpenAI GPT-4o-mini API
                       (Natural language processing)
```

### Database Schema

**ChatMessages Table**
```sql
CREATE TABLE ChatMessages (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    Role NVARCHAR(20) NOT NULL,        -- 'user' or 'assistant'
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    TokenCount INT NULL,               -- For rate limiting
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_ChatMessages_UserId ON ChatMessages(UserId);
CREATE INDEX IX_ChatMessages_CreatedAt ON ChatMessages(CreatedAt);
```

## API Endpoints

### 1. Send Message

**Endpoint**: `POST /api/chatbot/message`

**Description**: Sends a message to the AI chatbot and receives a response.

**Authorization**: Required (Bearer token)

**Request Body**:
```json
{
  "message": "What receipts do I have from Amazon?"
}
```

**Response** (200 OK):
```json
{
  "messageId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "role": "assistant",
  "content": "You have 3 receipts from Amazon:\n1. Laptop ($1,299.99) - June 15, 2024\n2. USB Cable ($12.99) - July 3, 2024\n3. Keyboard ($89.99) - August 20, 2024\n\nTotal Amazon spending: $1,402.97",
  "createdAt": "2024-11-16T19:30:00Z"
}
```

**Rate Limiting**: 10,000 tokens per day per user

**Response** (429 Too Many Requests):
```json
{
  "message": "You've reached your daily chat limit. Please try again tomorrow."
}
```

### 2. Get Conversation History

**Endpoint**: `GET /api/chatbot/history?limit=50`

**Description**: Retrieves the conversation history for the current user.

**Authorization**: Required (Bearer token)

**Query Parameters**:
- `limit` (optional): Maximum number of messages (default: 50, max: 100)

**Response** (200 OK):
```json
{
  "messages": [
    {
      "messageId": "guid",
      "role": "user",
      "content": "What receipts do I have?",
      "createdAt": "2024-11-16T19:25:00Z"
    },
    {
      "messageId": "guid",
      "role": "assistant",
      "content": "You have 15 receipts...",
      "createdAt": "2024-11-16T19:25:02Z"
    }
  ],
  "totalMessages": 2
}
```

### 3. Clear Conversation History

**Endpoint**: `DELETE /api/chatbot/history`

**Description**: Clears all conversation history for the current user.

**Authorization**: Required (Bearer token)

**Response** (204 No Content)

### 4. Get Suggested Questions

**Endpoint**: `GET /api/chatbot/suggested-questions`

**Description**: Returns a list of suggested questions to help users get started.

**Authorization**: Required (Bearer token)

**Response** (200 OK):
```json
{
  "questions": [
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
  ]
}
```

## Example Queries

### Receipt Discovery

**User**: "Show me all receipts from last month"  
**Assistant**: Lists receipts from the previous calendar month with details

**User**: "Find receipts over $100"  
**Assistant**: Shows receipts with amounts exceeding $100

**User**: "What did I buy at Best Buy?"  
**Assistant**: Lists all purchases from Best Buy with dates and amounts

### Warranty Tracking

**User**: "Which warranties are expiring soon?"  
**Assistant**: Lists warranties expiring in the next 30-60 days

**User**: "Do I still have warranty on my laptop?"  
**Assistant**: Checks warranty status for laptop purchases

**User**: "Show me all items with active warranties"  
**Assistant**: Lists all receipts with unexpired warranties

### Spending Analysis

**User**: "How much did I spend this month?"  
**Assistant**: Calculates total spending for current month

**User**: "What's my biggest purchase this year?"  
**Assistant**: Identifies most expensive item purchased

**User**: "How much have I spent on electronics?"  
**Assistant**: Sums spending on electronics category

### Natural Language Date Parsing

The chatbot understands various date formats:
- "last month", "this month", "next month"
- "last week", "this week"
- "this year", "last year"
- "in June", "last summer"
- "30 days ago", "6 months ago"

## Configuration

### OpenAI API Key

The chatbot requires an OpenAI API key. Configure it using user secrets:

```bash
dotnet user-secrets set "OpenAI:ApiKey" "your-openai-api-key"
```

Or in `appsettings.json` (not recommended for production):
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key"
  }
}
```

### Rate Limiting

Default limits (configurable in `ChatbotService.cs`):
- **Max tokens per day**: 10,000 tokens per user
- **Max history context**: 10 recent messages
- **Max response tokens**: 500 tokens per response
- **Temperature**: 0.7 (balanced creativity)

## System Prompt

The chatbot uses a dynamic system prompt that includes:
1. **Role definition**: AI assistant for warranty management
2. **Available data**: User's receipts and shared receipts
3. **Guidelines**: Response format, tone, and behavior
4. **Current date**: For date-relative queries
5. **Receipt context**: Recent receipts with full details

## Security & Privacy

### User Isolation
- Each user can only query their own receipts
- Conversation history is per-user
- Shared receipts are included with proper permissions

### Data Minimization
- Only necessary receipt data is sent to OpenAI
- Filenames, amounts, merchants, dates included
- No user personal information (names, emails) sent
- No file contents or images sent

### Rate Limiting
- Prevents abuse and controls costs
- 10,000 tokens per day per user
- Token usage tracked in database

### Audit Trail
- All messages stored in database
- Full conversation history maintained
- Token usage logged for cost tracking

## Cost Estimation

**OpenAI GPT-4o-mini Pricing** (as of Nov 2024):
- Input: $0.150 per 1M tokens
- Output: $0.600 per 1M tokens

**Example Usage**:
- Average query: ~500 tokens (prompt + response)
- Daily limit: 10,000 tokens per user
- Cost per user per day: ~$0.005 (half a cent)
- 1,000 active users: ~$5/day or $150/month

## Error Handling

### API Key Not Configured
```json
{
  "message": "Chat service is not configured. Please contact support."
}
```

### Rate Limit Exceeded
```json
{
  "message": "You've reached your daily chat limit. Please try again tomorrow."
}
```

### OpenAI API Error
```json
{
  "message": "I'm having trouble processing your request right now. Please try again later."
}
```

### Generic Error
```json
{
  "message": "An error occurred while processing your message"
}
```

## Testing

### Manual Testing with Swagger

1. Navigate to `/swagger`
2. Authenticate with Bearer token
3. Try suggested questions endpoint
4. Send messages to the chatbot
5. View conversation history

### Example Test Flow

```bash
# 1. Get suggested questions
GET /api/chatbot/suggested-questions

# 2. Send a message
POST /api/chatbot/message
{
  "message": "What receipts do I have?"
}

# 3. Follow-up question
POST /api/chatbot/message
{
  "message": "Show me the ones from Amazon"
}

# 4. View history
GET /api/chatbot/history?limit=10

# 5. Clear history
DELETE /api/chatbot/history
```

## Future Enhancements

### Planned Features
- [ ] Multi-language support (Spanish, French, German)
- [ ] Voice input/output integration
- [ ] Receipt image queries ("Show me the receipt with the red logo")
- [ ] Spending predictions and budgeting advice
- [ ] Export conversation history to PDF/email
- [ ] Integration with calendar for warranty reminders
- [ ] Advanced analytics (trends, comparisons, forecasts)
- [ ] Batch operations via chat ("Delete all receipts from 2020")

### Performance Optimization
- [ ] Response caching for common queries
- [ ] Streaming responses for faster perceived performance
- [ ] Embeddings for semantic search (vector database)
- [ ] Local LLM option for cost reduction

### Advanced Features
- [ ] Multi-turn form filling ("I want to add a receipt")
- [ ] Receipt dispute assistance
- [ ] Return policy tracking and reminders
- [ ] Warranty claim guidance
- [ ] Product recall notifications

## Monitoring & Analytics

### Key Metrics to Track
1. **Usage**:
   - Messages per day/week/month
   - Active users with chatbot access
   - Average messages per session
   - Conversation length distribution

2. **Performance**:
   - Average response time
   - OpenAI API latency
   - Error rate
   - Token usage per query

3. **Cost**:
   - Daily/monthly OpenAI API costs
   - Cost per user
   - Cost per query
   - Budget alerts

4. **Quality**:
   - User satisfaction (thumbs up/down)
   - Follow-up question rate
   - Clear history frequency (indicator of poor quality)
   - Repeated similar queries (not getting good answers)

## Troubleshooting

### Chatbot not responding
- Verify OpenAI API key is configured
- Check API key validity at platform.openai.com
- Review logs for API errors
- Confirm user is authenticated

### Inaccurate responses
- Check receipt data quality in database
- Review system prompt for clarity
- Adjust temperature (lower = more focused)
- Add more context to system prompt

### Rate limit issues
- Review token usage patterns
- Adjust daily limits if needed
- Implement token usage warnings
- Consider tiered plans (free vs premium)

### Performance issues
- Monitor OpenAI API latency
- Check database query performance
- Review conversation history size
- Consider response caching

## Conclusion

The AI Chatbot feature provides a powerful, intuitive interface for users to interact with their receipt data. By leveraging OpenAI's natural language understanding, users can ask questions in plain English and receive accurate, contextual answers. The feature includes proper rate limiting, security measures, and conversation history management, making it both user-friendly and cost-effective.

---

**Next Steps**: Implement frontend chat UI with message history, suggested questions, and typing indicators.
