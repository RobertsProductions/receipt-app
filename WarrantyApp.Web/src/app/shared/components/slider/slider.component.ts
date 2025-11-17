import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-slider',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './slider.component.html',
  styleUrl: './slider.component.scss',
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => SliderComponent),
    multi: true
  }]
})
export class SliderComponent implements ControlValueAccessor {
  @Input() label?: string;
  @Input() min: number = 0;
  @Input() max: number = 100;
  @Input() step: number = 1;
  @Input() disabled: boolean = false;
  @Input() showValue: boolean = true;
  @Input() unit?: string;
  @Output() valueChange = new EventEmitter<number>();

  value: number = 0;
  onChange: any = () => {};
  onTouched: any = () => {};

  onSliderChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.value = Number(target.value);
    this.onChange(this.value);
    this.valueChange.emit(this.value);
  }

  writeValue(value: number): void {
    this.value = value || this.min;
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

  get percentage(): number {
    return ((this.value - this.min) / (this.max - this.min)) * 100;
  }
}
