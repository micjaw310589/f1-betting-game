import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface ToastMessage {
  id: string;
  type: 'success' | 'info' | 'warning';
  title: string;
  message: string;
  points?: number;       // optional: points earned
  duration?: number;     // default 4000ms
}

const MAX_TOASTS = 3;

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toasts: ToastMessage[] = [];
  private readonly TOAST_ID_PREFIX = 'toast_';
  private toastIdCounter = 0;

  // Subject to emit toast changes to subscribers
  private toastSubject = new Subject<ToastMessage[]>();

  // Observable for components to subscribe to
  get toastChanges() {
    return this.toastSubject.asObservable();
  }

  // Show a toast notification
  show(toast: Omit<ToastMessage, 'id'>): string {
    const id = `${this.TOAST_ID_PREFIX}${++this.toastIdCounter}`;
    const newToast: ToastMessage = {
      ...toast,
      id,
      duration: toast.duration ?? 4000
    };

    // Enforce max toasts: remove oldest if at capacity
    if (this.toasts.length >= MAX_TOASTS) {
      this.toasts.shift();
    }

    this.toasts.push(newToast);

    // Notify subscribers
    this.toastSubject.next([...this.toasts]);

    // Auto-dismiss after duration
    setTimeout(() => {
      this.dismiss(id);
    }, newToast.duration);

    return id;
  }

  // Dismiss a specific toast
  dismiss(id: string): void {
    const index = this.toasts.findIndex(t => t.id === id);
    if (index !== -1) {
      this.toasts.splice(index, 1);
      this.toastSubject.next([...this.toasts]);
    }
  }

  // Get current toasts (for template binding)
  getToasts(): ToastMessage[] {
    return [...this.toasts];
  }

  // Clear all toasts
  clear(): void {
    this.toasts = [];
    this.toastSubject.next([]);
  }

  // Convenience method: show points earned toast
  showPointsEarned(questName: string, points: number): void {
    this.show({
      type: 'success',
      title: 'Quest Completed!',
      message: `${questName}`,
      points
    });
  }

  // Convenience method: show daily login toast
  showDailyLogin(streakDays: number, points: number): void {
    this.show({
      type: 'success',
      title: 'Daily Login',
      message: `Streak: ${streakDays} day${streakDays !== 1 ? 's' : ''}`,
      points
    });
  }
}
