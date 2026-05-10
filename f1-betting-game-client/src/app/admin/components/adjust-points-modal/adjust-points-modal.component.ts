import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../services/admin.service';

@Component({
    selector: 'app-adjust-points-modal',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './adjust-points-modal.component.html',
    styleUrl: './adjust-points-modal.component.css',
})
export class AdjustPointsModalComponent {
    userId = input.required<number>();
    username = input.required<string>();
    success = output<void>();
    cancel = output<void>();

    points = 0;
    reason = '';
    isLoading = false;
    errorMessage = '';
    successMessage = '';

    constructor(private adminService: AdminService) {}

    submit(): void {
        if (this.points === 0) {
            this.errorMessage = 'Points adjustment must be non-zero.';
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';
        this.successMessage = '';

        this.adminService
            .adjustUserPoints(this.userId(), {
                points: this.points,
                reason: this.reason || undefined,
            })
            .subscribe({
                next: (result) => {
                    this.successMessage = `Successfully ${this.points > 0 ? 'added' : 'removed'} ${Math.abs(this.points)} points from ${this.username()}. New balance: ${result.newBalance}`;
                    this.isLoading = false;
                    setTimeout(() => {
                        this.success.emit();
                    }, 1500);
                },
                error: (error) => {
                    this.errorMessage = error.message || 'Failed to adjust points.';
                    this.isLoading = false;
                },
            });
    }

    onCancel(): void {
        this.cancel.emit();
    }
}
