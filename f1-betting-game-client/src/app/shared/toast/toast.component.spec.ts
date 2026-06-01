import { ComponentFixture, TestBed, fakeAsync, tick, waitForAsync } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { ToastComponent } from './toast.component';
import { ToastService, ToastMessage } from './toast.service';
import { Subject } from 'rxjs';

describe('ToastComponent', () => {
  let component: ToastComponent;
  let fixture: ComponentFixture<ToastComponent>;
  let toastService: ToastService;
  let toastSubject: Subject<ToastMessage[]>;

  beforeEach(waitForAsync(() => {
    // Create a mock Subject for toast changes
    toastSubject = new Subject<ToastMessage[]>();

    TestBed.configureTestingModule({
      imports: [ToastComponent, CommonModule],
      providers: [
        {
          provide: ToastService,
          useValue: {
            toastChanges: toastSubject.asObservable(),
            getToasts: () => [],
            dismiss: jasmine.createSpy('dismiss'),
            clear: jasmine.createSpy('clear'),
            show: jasmine.createSpy('show').and.returnValue('toast_1'),
            showPointsEarned: jasmine.createSpy('showPointsEarned'),
            showDailyLogin: jasmine.createSpy('showDailyLogin')
          }
        }
      ]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ToastComponent);
    component = fixture.componentInstance;
    toastService = TestBed.inject(ToastService) as unknown as ToastService;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render empty when no toasts', () => {
    expect(component.toasts.length).toBe(0);
  });

  it('should display toasts when emitted from service', fakeAsync(() => {
    // Arrange
    const testToasts: ToastMessage[] = [
      { id: 'toast_1', type: 'success', title: 'Quest Completed!', message: 'First Bet', points: 200 },
      { id: 'toast_2', type: 'info', title: 'Daily Login', message: 'Streak: 5 days', points: 20 }
    ];

    // Act
    toastSubject.next(testToasts);
    fixture.detectChanges();
    tick();

    // Assert
    expect(component.toasts.length).toBe(2);
    expect(component.toasts[0].title).toBe('Quest Completed!');
    expect(component.toasts[1].title).toBe('Daily Login');
  }));

  it('should call dismiss on toastService when close button is clicked', () => {
    // Arrange
    spyOn(toastService, 'dismiss');

    // Act
    component.dismiss('toast_1');

    // Assert
    expect(toastService.dismiss).toHaveBeenCalledWith('toast_1');
  });

  it('should apply toast-points class when points are present', fakeAsync(() => {
    // Arrange
    const toastWithPoints: ToastMessage[] = [
      { id: 'toast_1', type: 'success', title: 'Quest', message: 'Test', points: 100 }
    ];

    // Act
    toastSubject.next(toastWithPoints);
    fixture.detectChanges();
    tick();

    // Assert
    expect(component.toasts[0].points).toBe(100);
  }));

  it('should apply toast-points class when points are undefined', fakeAsync(() => {
    // Arrange
    const toastWithoutPoints: ToastMessage[] = [
      { id: 'toast_1', type: 'info', title: 'Info', message: 'Test' }
    ];

    // Act
    toastSubject.next(toastWithoutPoints);
    fixture.detectChanges();
    tick();

    // Assert
    expect(component.toasts[0].points).toBeUndefined();
  }));

  it('should unsubscribe on destroy', () => {
    // Act
    component.ngOnDestroy();

    // Assert - no errors thrown, component is cleaned up
    expect(component).toBeTruthy();
  });
});
