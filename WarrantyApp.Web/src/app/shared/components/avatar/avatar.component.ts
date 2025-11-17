import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-avatar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './avatar.component.html',
  styleUrl: './avatar.component.scss'
})
export class AvatarComponent {
  @Input() src?: string;
  @Input() alt?: string;
  @Input() initials?: string;
  @Input() size: 'xs' | 'sm' | 'md' | 'lg' | 'xl' = 'md';
  @Input() status?: 'online' | 'offline' | 'away';

  imageError: boolean = false;

  onImageError(): void {
    this.imageError = true;
  }

  get displayInitials(): string {
    if (this.initials) {
      return this.initials.substring(0, 2).toUpperCase();
    }
    return '?';
  }
}
