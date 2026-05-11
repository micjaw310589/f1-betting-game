import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../services/admin.service';
import { AdminUserDto, PagedResult } from '../models/admin.models';
import { AdjustPointsModalComponent } from '../components/adjust-points-modal/adjust-points-modal.component';
import { SuspendUserModalComponent } from '../components/suspend-user-modal/suspend-user-modal.component';

@Component({
    selector: 'app-admin-user-management',
    standalone: true,
    imports: [CommonModule, FormsModule, AdjustPointsModalComponent, SuspendUserModalComponent],
    templateUrl: './admin-user-management.component.html',
    styleUrl: './admin-user-management.component.css',
})
export class AdminUserManagementComponent implements OnInit {
    // Data
    users: AdminUserDto[] = [];
    isLoading = true;
    hasError = false;
    errorMessage = '';

    // Pagination
    page = 1;
    pageSize = 20;
    totalItems = 0;
    totalPages = 0;

    // Filters
    filterIsActive: boolean | null = null;
    searchTerm = '';

    // Modal state
    showAdjustPoints = false;
    showSuspendUser = false;
    selectedUserId: number | null = null;
    selectedUsername = '';

    constructor(
        private adminService: AdminService,
        private cdr: ChangeDetectorRef
    ) {}

    ngOnInit(): void {
        this.loadUsers();
    }

    loadUsers(): void {
        this.isLoading = true;
        this.hasError = false;

        this.adminService
            .getAllUsers(this.page, this.pageSize, this.filterIsActive !== null ? this.filterIsActive : undefined, this.searchTerm || undefined)
            .subscribe({
                next: (result) => {
                    this.users = result.items;
                    this.totalItems = result.totalItems;
                    this.totalPages = result.totalPages;
                    this.isLoading = false;
                },
                error: (error) => {
                    console.error('Error loading users:', error);
                    this.hasError = true;
                    this.errorMessage = error.message || 'Failed to load users';
                    this.isLoading = false;
                },
            });
    }

    onPageChange(page: number): void {
        this.page = page;
        this.loadUsers();
    }

    onFilterChange(): void {
        this.page = 1;
        this.loadUsers();
    }

    onSearch(): void {
        this.page = 1;
        this.loadUsers();
    }

    onSearchKeyDown(event: KeyboardEvent): void {
        if (event.key === 'Enter') {
            this.onSearch();
        }
    }

    clearSearch(): void {
        this.searchTerm = '';
        this.page = 1;
        this.loadUsers();
    }

    // --- Adjust Points ---

    openAdjustPointsModal(user: AdminUserDto): void {
        this.selectedUserId = user.id;
        this.selectedUsername = user.username;
        this.showAdjustPoints = true;
    }

    onAdjustPointsSuccess(): void {
        this.showAdjustPoints = false;
        this.selectedUserId = null;
        this.loadUsers();
    }

    onAdjustPointsCancel(): void {
        this.showAdjustPoints = false;
        this.selectedUserId = null;
    }

    // --- Suspend/Reactivate ---

    openSuspendUserModal(user: AdminUserDto): void {
        this.selectedUserId = user.id;
        this.selectedUsername = user.username;
        this.showSuspendUser = true;
    }

    onSuspendUserSuccess(): void {
        this.showSuspendUser = false;
        this.selectedUserId = null;
        this.loadUsers();
    }

    onSuspendUserCancel(): void {
        this.showSuspendUser = false;
        this.selectedUserId = null;
    }

    // --- Utility ---

    formatDate(date: Date | null): string {
        if (!date) return 'Never';
        return new Date(date).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    }

    get totalPagesForPagination(): number {
        return Math.max(1, this.totalPages);
    }

    get pageNumbers(): number[] {
        const pages: number[] = [];
        const maxVisible = 5;
        let start = Math.max(1, this.page - Math.floor(maxVisible / 2));
        let end = Math.min(this.totalPagesForPagination, start + maxVisible - 1);
        if (end - start + 1 < maxVisible) {
            start = Math.max(1, end - maxVisible + 1);
        }
        for (let i = start; i <= end; i++) {
            pages.push(i);
        }
        return pages;
    }

    getFilterLabel(): string {
        if (this.filterIsActive === null) return 'All Users';
        return this.filterIsActive ? 'Active Users' : 'Suspended Users';
    }
}
