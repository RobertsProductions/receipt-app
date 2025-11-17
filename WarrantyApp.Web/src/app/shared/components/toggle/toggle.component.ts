import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-toggle',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toggle.component.html',
  styleUrl: './toggle.component.scss',
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => ToggleComponent),
    multi: true
  }]
})
export class ToggleComponent implements ControlValueAccessor {
  @Input() label?: string;
  @Input() disabled: boolean = false;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Output() valueChange = new EventEmitter<boolean>();

  value: boolean = false;
  onChange: any = () => {};
  onTouched: any = () => {};

  toggle(): void {
    if (this.disabled) return;
    this.value = !this.value;
    this.onChange(this.value);
    this.onTouched();
    this.valueChange.emit(this.value);
  }

  writeValue(value: boolean): void {
    this.value = value || false;
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
