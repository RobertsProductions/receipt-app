import { Component, Input, Output, EventEmitter, forwardRef, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

export interface DropdownOption {
  value: any;
  label: string;
  disabled?: boolean;
}

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dropdown.component.html',
  styleUrl: './dropdown.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DropdownComponent),
      multi: true
    }
  ]
})
export class DropdownComponent implements ControlValueAccessor {
  @Input() options: DropdownOption[] = [];
  @Input() placeholder: string = 'Select an option';
  @Input() label?: string;
  @Input() disabled: boolean = false;
  @Input() searchable: boolean = false;
  @Input() error?: string;
  @Input() testId?: string;
  @Output() selectionChange = new EventEmitter<any>();

  isOpen = false;
  searchQuery = '';
  selectedOption?: DropdownOption;
  highlightedIndex = -1;

  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private elementRef: ElementRef) {}

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent) {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.closeDropdown();
    }
  }

  get filteredOptions(): DropdownOption[] {
    if (!this.searchable || !this.searchQuery) {
      return this.options;
    }
    const query = this.searchQuery.toLowerCase();
    return this.options.filter(opt => 
      opt.label.toLowerCase().includes(query)
    );
  }

  get displayValue(): string {
    return this.selectedOption?.label || this.placeholder;
  }

  toggleDropdown() {
    if (!this.disabled) {
      this.isOpen = !this.isOpen;
      if (this.isOpen) {
        this.highlightedIndex = this.options.findIndex(opt => opt.value === this.selectedOption?.value);
      } else {
        this.searchQuery = '';
      }
    }
  }

  closeDropdown() {
    this.isOpen = false;
    this.searchQuery = '';
    this.highlightedIndex = -1;
  }

  selectOption(option: DropdownOption) {
    if (option.disabled) return;
    
    this.selectedOption = option;
    this.onChange(option.value);
    this.onTouched();
    this.selectionChange.emit(option.value);
    this.closeDropdown();
  }

  onKeyDown(event: KeyboardEvent) {
    if (this.disabled) return;

    switch (event.key) {
      case 'Enter':
      case ' ':
        event.preventDefault();
        if (!this.isOpen) {
          this.isOpen = true;
        } else if (this.highlightedIndex >= 0) {
          this.selectOption(this.filteredOptions[this.highlightedIndex]);
        }
        break;
      case 'Escape':
        event.preventDefault();
        this.closeDropdown();
        break;
      case 'ArrowDown':
        event.preventDefault();
        if (!this.isOpen) {
          this.isOpen = true;
        } else {
          this.highlightedIndex = Math.min(this.highlightedIndex + 1, this.filteredOptions.length - 1);
        }
        break;
      case 'ArrowUp':
        event.preventDefault();
        if (this.isOpen) {
          this.highlightedIndex = Math.max(this.highlightedIndex - 1, 0);
        }
        break;
      case 'Home':
        event.preventDefault();
        if (this.isOpen) {
          this.highlightedIndex = 0;
        }
        break;
      case 'End':
        event.preventDefault();
        if (this.isOpen) {
          this.highlightedIndex = this.filteredOptions.length - 1;
        }
        break;
    }
  }

  writeValue(value: any): void {
    this.selectedOption = this.options.find(opt => opt.value === value);
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}
