import { TestBed } from '@angular/core/testing';
import { ToastService, ToastMessage } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastService);
  });

  afterEach(() => {
    // Clear all toasts after each test
    service.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should show a toast and add it to the list', () => {
    // Act
    const id = service.show({
      type: 'info',
      title: 'Test Toast',
      message: 'This is a test'
    });

    // Assert
    expect(id).toBeTruthy();
    expect(id).toContain('toast_');
    const toasts = service.getToasts();
    expect(toasts.length).toBe(1);
    expect(toasts[0].title).toBe('Test Toast');
    expect(toasts[0].message).toBe('This is a test');
    expect(toasts[0].type).toBe('info');
  });

  it('should dismiss a toast by id', () => {
    // Arrange
    const id = service.show({
      type: 'info',
      title: 'Test Toast',
      message: 'This is a test'
    });

    // Act
    service.dismiss(id);

    // Assert
    expect(service.getToasts().length).toBe(0);
  });

  it('should show points earned toast with points', () => {
    // Act
    service.showPointsEarned('First Bet', 200);

    // Assert
    const toasts = service.getToasts();
    expect(toasts.length).toBe(1);
    expect(toasts[0].title).toBe('Quest Completed!');
    expect(toasts[0].message).toBe('First Bet');
    expect(toasts[0].points).toBe(200);
    expect(toasts[0].type).toBe('success');
  });

  it('should show daily login toast with streak info', () => {
    // Act
    service.showDailyLogin(5, 20);

    // Assert
    const toasts = service.getToasts();
    expect(toasts.length).toBe(1);
    expect(toasts[0].title).toBe('Daily Login');
    expect(toasts[0].message).toBe('Streak: 5 days');
    expect(toasts[0].points).toBe(20);
  });

  it('should show daily login toast with singular day', () => {
    // Act
    service.showDailyLogin(1, 10);

    // Assert
    const toasts = service.getToasts();
    expect(toasts[0].message).toBe('Streak: 1 day');
  });

  it('should enforce max 3 toasts', () => {
    // Act - show 5 toasts
    service.show({ type: 'info', title: 'Toast 1', message: '1' });
    service.show({ type: 'info', title: 'Toast 2', message: '2' });
    service.show({ type: 'info', title: 'Toast 3', message: '3' });
    service.show({ type: 'info', title: 'Toast 4', message: '4' });
    service.show({ type: 'info', title: 'Toast 5', message: '5' });

    // Assert - only last 3 should remain
    const toasts = service.getToasts();
    expect(toasts.length).toBe(3);
    expect(toasts[0].title).toBe('Toast 3');
    expect(toasts[1].title).toBe('Toast 4');
    expect(toasts[2].title).toBe('Toast 5');
  });

  it('should clear all toasts', () => {
    // Arrange
    service.show({ type: 'info', title: 'Toast 1', message: '1' });
    service.show({ type: 'info', title: 'Toast 2', message: '2' });

    // Act
    service.clear();

    // Assert
    expect(service.getToasts().length).toBe(0);
  });

  it('should emit toast changes via toastChanges observable', (done: DoneFn) => {
    // Arrange
    service.toastChanges.subscribe(toasts => {
      expect(toasts.length).toBe(1);
      expect(toasts[0].title).toBe('Test');
      done();
    });

    // Act
    service.show({ type: 'info', title: 'Test', message: 'Test' });
  });

  it('should use default duration of 4000ms', () => {
    // Act
    const id = service.show({
      type: 'info',
      title: 'Test Toast',
      message: 'Test'
    });

    // Assert
    const toasts = service.getToasts();
    expect(toasts[0].duration).toBe(4000);
  });

  it('should use custom duration when provided', () => {
    // Act
    service.show({
      type: 'info',
      title: 'Test Toast',
      message: 'Test',
      duration: 10000
    });

    // Assert
    const toasts = service.getToasts();
    expect(toasts[0].duration).toBe(10000);
  });
});
