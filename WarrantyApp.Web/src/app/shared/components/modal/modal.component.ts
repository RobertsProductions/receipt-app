import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, HostListener, ContentChild, ElementRef, AfterContentInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modal.component.html',
  styleUrl: './modal.component.scss',
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('200ms ease-out', style({ opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ opacity: 0 }))
      ])
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateY(-20px)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateY(0)', opacity: 1 }))
      ])
    ])
  ]
})
export class ModalComponent implements OnInit, OnDestroy, AfterContentInit {
  @Input() isOpen: boolean = false;
  @Input() title?: string;
  @Input() size: 'sm' | 'md' | 'lg' | 'xl' | 'full' = 'md';
  @Input() closeOnBackdrop: boolean = true;
  @Input() closeOnEscape: boolean = true;
  @Input() showCloseButton: boolean = true;
  @Output() onClose = new EventEmitter<void>();

  @ContentChild('[footer]') footerContent?: ElementRef;
  hasFooter = false;

  ngOnInit(): void {
    if (this.isOpen) {
      this.lockBodyScroll();
    }
  }

  ngOnDestroy(): void {
    this.unlockBodyScroll();
  }

  ngOnChanges(): void {
    if (this.isOpen) {
      this.lockBodyScroll();
    } else {
      this.unlockBodyScroll();
    }
  }

  ngAfterContentInit(): void {
    this.hasFooter = !!this.footerContent;
  }

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.isOpen && this.closeOnEscape) {
      this.close();
    }
  }

  close(): void {
    this.onClose.emit();
  }

  onBackdropClick(): void {
    if (this.closeOnBackdrop) {
      this.close();
    }
  }

  onModalContentClick(event: Event): void {
    event.stopPropagation();
  }

  private lockBodyScroll(): void {
    document.body.style.overflow = 'hidden';
  }

  private unlockBodyScroll(): void {
    document.body.style.overflow = '';
  }
}
