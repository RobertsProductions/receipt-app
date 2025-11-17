import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type ProgressVariant = 'primary' | 'success' | 'warning' | 'error';

@Component({
  selector: 'app-progress-bar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './progress-bar.component.html',
  styleUrl: './progress-bar.component.scss'
})
export class ProgressBarComponent {
  @Input() value: number = 0;
  @Input() max: number = 100;
  @Input() variant: ProgressVariant = 'primary';
  @Input() showLabel: boolean = false;
  @Input() label?: string;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() striped: boolean = false;
  @Input() animated: boolean = false;
  @Input() testId?: string;

  get percentage(): number {
    return Math.min(Math.max((this.value / this.max) * 100, 0), 100);
  }

  get displayLabel(): string {
    if (this.label) {
      return this.label;
    }
    return `${Math.round(this.percentage)}%`;
  }
}
