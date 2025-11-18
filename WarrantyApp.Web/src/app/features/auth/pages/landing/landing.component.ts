import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { CardComponent } from '../../../../shared/components/card/card.component';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonComponent, CardComponent],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss'
})
export class LandingComponent {
  features = [
    {
      icon: '📷',
      title: 'AI-Powered OCR',
      description: 'Upload receipts and AI extracts merchant, amount, date, and products.'
    },
    {
      icon: '⏰',
      title: 'Warranty Tracking',
      description: 'Never miss a warranty expiration. Track all your warranties in one place.'
    },
    {
      icon: '🔔',
      title: 'Smart Notifications',
      description: 'Get timely email and SMS alerts before your warranties expire.'
    },
    {
      icon: '🤖',
      title: 'AI Chatbot',
      description: 'Ask questions about your receipts in natural language and get instant answers.'
    },
    {
      icon: '🔗',
      title: 'Easy Sharing',
      description: 'Share receipts with family members or service providers securely.'
    },
    {
      icon: '📱',
      title: 'Mobile Ready',
      description: 'Access your receipts anywhere, anytime from any device.'
    }
  ];

  steps = [
    {
      number: '1',
      title: 'Upload',
      description: 'Take a photo or upload your receipt. Our AI extracts all the details automatically.'
    },
    {
      number: '2',
      title: 'Track',
      description: 'View all your receipts and warranties in one organized dashboard.'
    },
    {
      number: '3',
      title: 'Get Notified',
      description: 'Receive alerts before warranties expire so you never miss a claim.'
    }
  ];
}
