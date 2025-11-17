import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './badge.component.html',
  styleUrl: './badge.component.scss'
})
export class BadgeComponent {
  @Input() variant: 'success' | 'warning' | 'error' | 'info' | 'neutral' = 'neutral';
  @Input() size: 'sm' | 'md' = 'md';
  @Input() rounded: boolean = true;
}
