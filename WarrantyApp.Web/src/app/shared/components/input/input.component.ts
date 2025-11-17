import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-input',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './input.component.html',
  styleUrl: './input.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true
    }
  ]
})
export class InputComponent implements ControlValueAccessor {
  @Input() type: 'text' | 'email' | 'password' | 'number' | 'tel' | 'url' | 'search' = 'text';
  @Input() label?: string;
  @Input() placeholder?: string;
  @Input() disabled: boolean = false;
  @Input() required: boolean = false;
  @Input() error?: string;
  @Input() hint?: string;
  @Input() icon?: string;
  @Input() autocomplete?: string;
  @Output() onBlurEvent = new EventEmitter<void>();
  @Output() onFocusEvent = new EventEmitter<void>();

  value: string = '';
  showPassword: boolean = false;
  isFocused: boolean = false;

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  get inputType(): string {
    if (this.type === 'password' && this.showPassword) {
      return 'text';
    }
    return this.type;
  }

  writeValue(value: string): void {
    this.value = value || '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.value = target.value;
    this.onChange(this.value);
  }

  onBlur(): void {
    this.isFocused = false;
    this.onTouched();
    this.onBlurEvent.emit();
  }

  onFocus(): void {
    this.isFocused = true;
    this.onFocusEvent.emit();
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
