import { Component, OnInit, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ChatbotService } from '../../../../services/chatbot.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { ChatMessage } from '../../../../models';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { CardComponent } from '../../../../shared/components/card/card.component';

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonComponent,
    InputComponent,
    SpinnerComponent,
    EmptyStateComponent,
    CardComponent
  ],
  templateUrl: './chatbot.component.html',
  styleUrl: './chatbot.component.scss'
})
export class ChatbotComponent implements OnInit, AfterViewChecked {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;

  messages: ChatMessage[] = [];
  userMessage = '';
  isLoading = false;
  isSending = false;
  shouldScroll = false;

  suggestedQuestions = [
    'Show me receipts from last month',
    'What did I spend at Walmart?',
    'Which warranties expire soon?',
    'What\'s my total spending this year?',
    'Find receipts over $100',
    'Show me all electronics warranties'
  ];

  constructor(
    private chatbotService: ChatbotService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadChatHistory();
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  loadChatHistory(): void {
    this.isLoading = true;
    this.chatbotService.getHistory().subscribe({
      next: (response) => {
        this.messages = response.messages;
        this.shouldScroll = true;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load chat history:', error);
        this.toastService.error('Failed to load chat history');
        this.isLoading = false;
      }
    });
  }

  sendMessage(message?: string): void {
    const messageToSend = message || this.userMessage.trim();
    
    if (!messageToSend || this.isSending) {
      return;
    }

    // Add user message to UI immediately
    const userMsg: ChatMessage = {
      id: `temp-${Date.now()}`,
      role: 'user',
      content: messageToSend,
      timestamp: new Date().toISOString()
    };
    this.messages.push(userMsg);
    this.shouldScroll = true;

    // Clear input
    this.userMessage = '';
    this.isSending = true;

    // Send to API
    this.chatbotService.sendMessage(messageToSend).subscribe({
      next: (response) => {
        // Backend returns ChatMessageResponseDto with Content field
        const aiMsg: ChatMessage = {
          id: response.messageId || `ai-${Date.now()}`,
          role: 'assistant',
          content: response.content || '',
          timestamp: response.createdAt || new Date().toISOString()
        };
        this.messages.push(aiMsg);
        this.shouldScroll = true;
        this.isSending = false;
      },
      error: (error) => {
        console.error('Failed to send message:', error);
        this.toastService.error('Failed to send message. Please try again.');
        // Remove the failed message
        this.messages.pop();
        // Restore the user's input
        this.userMessage = messageToSend;
        this.isSending = false;
      }
    });
  }

  useSuggestedQuestion(question: string): void {
    this.sendMessage(question);
  }

  clearConversation(): void {
    if (!confirm('Are you sure you want to clear the entire conversation? This cannot be undone.')) {
      return;
    }

    this.chatbotService.clearHistory().subscribe({
      next: () => {
        this.messages = [];
        this.toastService.success('Conversation cleared');
      },
      error: (error) => {
        console.error('Failed to clear conversation:', error);
        this.toastService.error('Failed to clear conversation');
      }
    });
  }

  navigateToReceipts(): void {
    this.router.navigate(['/receipts']);
  }

  private scrollToBottom(): void {
    try {
      if (this.messagesContainer) {
        const container = this.messagesContainer.nativeElement;
        container.scrollTop = container.scrollHeight;
      }
    } catch (err) {
      console.error('Scroll to bottom failed:', err);
    }
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }
}
