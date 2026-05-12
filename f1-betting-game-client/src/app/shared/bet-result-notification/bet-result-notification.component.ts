import { Component, OnInit, OnDestroy, Input, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Subscription } from 'rxjs';

export interface BetResultNotification {
  id: number;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
  type: 'win' | 'loss' | 'info';
}

@Component({
  selector: 'app-bet-result-notification',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="notification-container" *ngIf="showNotification">
      <div class="notification-banner" [class.win]="notification?.type === 'win'" [class.loss]="notification?.type === 'loss'" [class.info]="notification?.type === 'info'">
        <div class="notification-content">
          <div class="notification-icon">
            <span *ngIf="notification?.type === 'win'">🏆</span>
            <span *ngIf="notification?.type === 'loss'">😔</span>
            <span *ngIf="notification?.type === 'info'">🏁</span>
          </div>
          <div class="notification-text">
            <h4 class="notification-title">{{ notification?.title }}</h4>
            <p class="notification-message">{{ notification?.message }}</p>
          </div>
          <button class="notification-close" (click)="dismissNotification()" type="button">
            <svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor">
              <path d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z"/>
            </svg>
          </button>
        </div>
        <div class="notification-progress-bar" *ngIf="autoDismiss"></div>
      </div>
    </div>
  `,
  styles: [`
    .notification-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      animation: slideIn 0.3s ease-out;
    }

    .notification-banner {
      position: relative;
      min-width: 350px;
      max-width: 450px;
      border-radius: 12px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
      overflow: hidden;
      animation: fadeIn 0.3s ease-out;
    }

    .notification-banner.win {
      background: linear-gradient(135deg, #1a472a 0%, #2d6a4f 100%);
      border: 1px solid #40916c;
    }

    .notification-banner.loss {
      background: linear-gradient(135deg, #5c1a1a 0%, #7a2d2d 100%);
      border: 1px solid #9a3d3d;
    }

    .notification-banner.info {
      background: linear-gradient(135deg, #1a2a5c 0%, #2d4a7a 100%);
      border: 1px solid #3d5a9a;
    }

    .notification-content {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 16px 48px 16px 16px;
    }

    .notification-icon {
      font-size: 32px;
      line-height: 1;
    }

    .notification-text {
      flex: 1;
    }

    .notification-title {
      margin: 0 0 4px 0;
      font-size: 16px;
      font-weight: 600;
      color: #ffffff;
    }

    .notification-message {
      margin: 0;
      font-size: 14px;
      color: rgba(255, 255, 255, 0.9);
      line-height: 1.4;
    }

    .notification-close {
      position: absolute;
      top: 12px;
      right: 12px;
      background: transparent;
      border: none;
      color: rgba(255, 255, 255, 0.7);
      cursor: pointer;
      padding: 4px;
      border-radius: 4px;
      transition: all 0.2s ease;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .notification-close:hover {
      background: rgba(255, 255, 255, 0.15);
      color: #ffffff;
    }

    .notification-progress-bar {
      position: absolute;
      bottom: 0;
      left: 0;
      height: 3px;
      background: rgba(255, 255, 255, 0.5);
      animation: progressFade 5s linear forwards;
    }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    @keyframes fadeIn {
      from {
        opacity: 0;
        transform: scale(0.95);
      }
      to {
        opacity: 1;
        transform: scale(1);
      }
    }

    @keyframes progressFade {
      from {
        width: 100%;
      }
      to {
        width: 0%;
      }
    }

    @media (max-width: 576px) {
      .notification-container {
        top: 10px;
        right: 10px;
        left: 10px;
      }

      .notification-banner {
        min-width: auto;
        max-width: none;
      }
    }
  `]
})
export class BetResultNotificationComponent implements OnInit, OnDestroy {
  @Input() autoDismiss: boolean = true;
  @Input() dismissDelay: number = 5000;

  showNotification: boolean = false;
  notification: BetResultNotification | null = null;
  private subscription: Subscription | null = null;
  private progressTimeout: number | null = null;

  constructor(
    private http: HttpClient,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.listenForBetResults();
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
    if (this.progressTimeout) {
      clearTimeout(this.progressTimeout);
    }
  }

  /**
   * Listen for bet result notifications via WebSocket or polling
   * For now, we use a simple event-based approach
   */
  private listenForBetResults(): void {
    // Listen for custom browser events triggered when bet results are processed
    const handler = (event: Event): void => {
      const customEvent = event as CustomEvent;
      if (customEvent.detail?.type === 'bet-result') {
        this.ngZone.run(() => {
          this.showNotificationInternal(customEvent.detail.payload);
        });
      }
    };

    window.addEventListener('betResultNotification', handler as EventListener);
    
    // Also listen for race result events
    const raceResultHandler = (event: Event): void => {
      const customEvent = event as CustomEvent;
      if (customEvent.detail?.type === 'race-result') {
        this.ngZone.run(() => {
          this.showNotificationInternal(customEvent.detail.payload);
        });
      }
    };

    window.addEventListener('raceResultNotification', raceResultHandler as EventListener);

this.subscription = new Subscription(() => {
        window.removeEventListener('betResultNotification', handler);
        window.removeEventListener('raceResultNotification', raceResultHandler);
      });
  }

  /**
   * Fetch latest notifications from the server and display any unread ones
   */
  private fetchAndDisplayNotifications(): void {
    const token = localStorage.getItem('token');
    if (!token) return;

    this.http.get<any[]>('/api/notifications/unread', {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: (notifications) => {
        if (notifications?.length > 0) {
          // Get the latest notification
          const latest = notifications[notifications.length - 1];
          if (latest) {
            const notificationType: 'win' | 'loss' | 'info' = 
              latest.title?.includes('Won') || latest.title?.includes('win') ? 'win' : 
              latest.title?.includes('did not win') ? 'loss' : 'info';
            
            this.showNotificationInternal({
              id: latest.id,
              title: latest.title || 'Race Results',
              message: latest.message || 'A race has been completed.',
              isRead: latest.isRead || false,
              createdAt: latest.createdAt,
              type: notificationType
            });
          }
        }
      },
      error: (error) => {
        console.error('Failed to fetch notifications:', error);
      }
    });
  }

  private showNotificationInternal(notification: BetResultNotification): void {
    if (this.progressTimeout) {
      clearTimeout(this.progressTimeout);
    }

    this.notification = notification;
    this.showNotification = true;

    if (this.autoDismiss) {
      this.progressTimeout = window.setTimeout(() => {
        this.dismissNotification();
      }, this.dismissDelay);
    }
  }

  dismissNotification(): void {
    this.showNotification = false;
    this.notification = null;
    if (this.progressTimeout) {
      clearTimeout(this.progressTimeout);
      this.progressTimeout = null;
    }
  }

  /**
   * Public method to trigger a notification programmatically
   * Used by services that detect race completions
   */
  showBetResult(type: 'win' | 'loss' | 'info', title: string, message: string): void {
    this.showNotificationInternal({
      id: Date.now(),
      title,
      message,
      isRead: false,
      createdAt: new Date().toISOString(),
      type
    });
  }
}