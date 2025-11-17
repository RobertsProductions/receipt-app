import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../button/button.component';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, ButtonComponent],
  templateUrl: './empty-state.component.html',
  styleUrl: './empty-state.component.scss'
})
export class EmptyStateComponent {
  @Input() icon?: string;
  @Input() title: string = '';
  @Input() description?: string;
  @Input() actionText?: string;
  @Output() onAction = new EventEmitter<void>();

  handleAction(): void {
    this.onAction.emit();
  }
}
