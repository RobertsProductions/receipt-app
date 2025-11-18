export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: string;
  receipts?: string[];
}

export interface ChatMessageResponseDto {
  messageId: string;
  role: string;
  content: string;
  createdAt: string;
}

export interface ChatbotResponse {
  message: string;
  receipts?: Array<{
    id: string;
    merchant?: string;
    amount?: number;
    purchaseDate?: string;
    productName?: string;
  }>;
  statistics?: {
    totalSpent?: number;
    receiptCount?: number;
    averageAmount?: number;
    [key: string]: unknown;
  };
  suggestedQuestions?: string[];
}

export interface SendMessageRequest {
  message: string;
}

export interface SuggestedQuestion {
  category: string;
  questions: string[];
}
