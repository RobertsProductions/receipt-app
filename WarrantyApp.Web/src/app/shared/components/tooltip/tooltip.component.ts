import { Component, Input, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';

type TooltipPosition = 'top' | 'bottom' | 'left' | 'right';

@Component({
  selector: 'app-tooltip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tooltip.component.html',
  styleUrl: './tooltip.component.scss'
})
export class TooltipComponent {
  @Input() text: string = '';
  @Input() position: TooltipPosition = 'top';
  @Input() delay: number = 200;
  @Input() disabled: boolean = false;

  isVisible = false;
  private timeoutId?: number;

  @HostListener('mouseenter')
  onMouseEnter() {
    if (this.disabled || !this.text) return;
    
    this.timeoutId = window.setTimeout(() => {
      this.isVisible = true;
    }, this.delay);
  }

  @HostListener('mouseleave')
  onMouseLeave() {
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }
    this.isVisible = false;
  }

  @HostListener('click')
  onClick() {
    // Hide tooltip on click for touch devices
    this.isVisible = false;
  }
}
