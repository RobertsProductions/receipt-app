import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Toast {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastsSubject = new BehaviorSubject<Toast[]>([]);
  public toasts$: Observable<Toast[]> = this.toastsSubject.asObservable();

  success(message: string, duration: number = 5000): void {
    this.addToast('success', message, duration);
  }

  error(message: string, duration: number = 5000): void {
    this.addToast('error', message, duration);
  }

  warning(message: string, duration: number = 5000): void {
    this.addToast('warning', message, duration);
  }

  info(message: string, duration: number = 5000): void {
    this.addToast('info', message, duration);
  }

  remove(id: string): void {
    const currentToasts = this.toastsSubject.value;
    this.toastsSubject.next(currentToasts.filter(toast => toast.id !== id));
  }

  private addToast(type: Toast['type'], message: string, duration: number): void {
    const toast: Toast = {
      id: this.generateId(),
      type,
      message,
      duration
    };

    const currentToasts = this.toastsSubject.value;
    this.toastsSubject.next([...currentToasts, toast]);

    if (duration > 0) {
      setTimeout(() => this.remove(toast.id), duration);
    }
  }

  private generateId(): string {
    return `toast_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }
}
