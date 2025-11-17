import { Component, Input, Output, EventEmitter, ContentChild, ElementRef, AfterContentInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './card.component.html',
  styleUrl: './card.component.scss'
})
export class CardComponent implements AfterContentInit {
  @Input() elevated: boolean = true;
  @Input() hoverable: boolean = false;
  @Input() padding: 'sm' | 'md' | 'lg' = 'md';
  @Input() clickable: boolean = false;
  @Output() onClick = new EventEmitter<void>();

  @ContentChild('[header]') headerContent?: ElementRef;
  @ContentChild('[footer]') footerContent?: ElementRef;

  hasHeader = false;
  hasFooter = false;

  ngAfterContentInit(): void {
    this.hasHeader = !!this.headerContent;
    this.hasFooter = !!this.footerContent;
  }

  handleClick(): void {
    if (this.clickable) {
      this.onClick.emit();
    }
  }
}
