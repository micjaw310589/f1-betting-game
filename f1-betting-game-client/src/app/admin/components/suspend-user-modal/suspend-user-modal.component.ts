import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../services/admin.service';

@Component({
    selector: 'app-suspend-user-modal',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './suspend-user-modal.component.html',
    styleUrl: './suspend-user-modal.component.css',
})
export class SuspendUserModalComponent {
    userId = input.required<number>();
    username = input.required<string>();
    isActive = input.required<boolean>();
    success = output<void>();
    cancel = output<void>();

    reason = '';
    isLoading = false;
    errorMessage = '';
    successMessage = '';

    get actionText(): string {
        return this.isActive() ? 'Suspend' : 'Reactivate';
    }

    get confirmationText(): string {
        return this.isActive()
            ? `Are you sure you want to suspend "${this.username()}"? This user will no longer be able to access the platform.`
            : `Are you sure you want to reactivate "${this.username()}"? This user will be able to access the platform again.`;
    }

    constructor(private adminService: AdminService) {}

    submit(): void {
        this.isLoading = true;
        this.errorMessage = '';
        this.successMessage = '';

        this.adminService
            .changeUserStatus(this.userId(), {
                isActive: !this.isActive(),
                reason: this.reason || undefined,
            })
            .subscribe({
                next: (result) => {
                    this.successMessage = `User ${this.isActive() ? 'suspended' : 'reactivated'} successfully.`;
                    this.isLoading = false;
                    setTimeout(() => {
                        this.success.emit();
                    }, 1500);
                },
                error: (error) => {
                    this.errorMessage = error.message || 'Failed to change user status.';
                    this.isLoading = false;
                },
            });
    }

    onCancel(): void {
        this.cancel.emit();
    }
}
