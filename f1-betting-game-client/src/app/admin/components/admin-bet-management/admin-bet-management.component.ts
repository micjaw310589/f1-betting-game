import { ChangeDetectorRef, Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../services/admin.service';
import {
    AdminBetResponseDto,
    CreateBetDto,
    UpdateBetDto,
    PagedResult,
    BetType,
    BetStatus,
    BET_STATUSES,
    BET_TYPES,
    BET_STATUS_CLASSES,
} from '../../models/admin.models';

@Component({
    selector: 'app-admin-bet-management',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './admin-bet-management.component.html',
    styleUrl: './admin-bet-management.component.css',
})
export class AdminBetManagementComponent implements OnInit, OnDestroy {
    // Data
    bets: AdminBetResponseDto[] = [];
    isLoading = true;
    hasError = false;
    errorMessage = '';

    // Pagination
    page = 1;
    pageSize = 500;
    totalItems = 0;
    totalPages = 0;

    // Filters
    filterStatus: BetStatus | null = null;
    searchTerm = '';

    // --- Create Bet Modal ---
    showCreateModal = false;
    createForm = {
        userId: '',
        raceId: '',
        driverId: '',
        amount: 0,
        betType: 'RaceWinner' as BetType,
    };
    isCreating = false;
    createSuccess = false;
    createError = '';
    availableRaces: { id: number; name: string }[] = [];
    availableDrivers: { id: number; name: string; teamName: string }[] = [];
    isLoadingOptions = true;

    // --- Edit Bet Modal ---
    showEditModal = false;
    editingBet: AdminBetResponseDto | null = null;
    editForm = {
        driverId: 0,
        amount: 0,
        betType: 'RaceWinner' as BetType,
        status: 'Pending' as BetStatus,
        winnings: 0,
    };
    isEditing = false;
    editSuccess = false;
    editError = '';
    isPartialEdit = false;

    // --- Delete Confirmation ---
    showDeleteConfirm = false;
    deletingBetId: number | null = null;
    isDeleting = false;
    deleteError = '';

    // --- Status display ---
    betStatuses = BET_STATUSES;
    betTypes = BET_TYPES;
    betStatusClasses = BET_STATUS_CLASSES;

    private betTypeLabels: Record<BetType, string> = {
        RaceWinner: 'Race Winner',
        PodiumFinish: 'Podium Finish',
        Top10Finish: 'Top 10 Finish',
        FastestLap: 'Fastest Lap',
    };

    constructor(
        private adminService: AdminService,
        private cdr: ChangeDetectorRef
    ) {}

    ngOnInit(): void {
        this.loadBetsInternal();
        this.loadOptions();
    }

    /**
     * Loads bets and options. Call this when the bets tab becomes active.
     */
    loadBets(): void {
        this.loadBetsInternal();
        this.loadOptions();
    }

    private loadBetsInternal(): void {
        this.isLoading = true;
        this.hasError = false;

        this.adminService
            .getAllBets(
                this.page,
                this.pageSize,
                this.filterStatus !== null ? this.filterStatus : undefined,
                this.searchTerm || undefined
            )
            .subscribe({
                next: (result: { items: AdminBetResponseDto[]; page: number; pageSize: number; totalItems: number; totalPages: number }) => {
                    this.bets = result.items;
                    this.totalItems = result.totalItems;
                    this.totalPages = result.totalPages;
                    this.isLoading = false;
                    this.cdr.markForCheck();
                },
                error: (error: any) => {
                    console.error('Error loading bets:', error);
                    this.hasError = true;
                    this.errorMessage = error.message || 'Failed to load bets';
                    this.isLoading = false;
                    this.cdr.markForCheck();
                },
            });
    }

    ngOnDestroy(): void {}

    loadOptions(): void {
        this.isLoadingOptions = true;

        // Load races for dropdown
        this.adminService.getAllRaces().subscribe({
            next: (races: { id: number; name: string; circuit: string; raceDate: Date; country: string; status: string; season: number; flag: string; odds: Record<number, number>; isManuallyOverridden: boolean }[]) => {
                this.availableRaces = races.map((r: { id: number; name: string; circuit: string; raceDate: Date; country: string; status: string; season: number; flag: string; odds: Record<number, number>; isManuallyOverridden: boolean }) => ({
                    id: r.id,
                    name: r.name,
                }));
                this.cdr.markForCheck();
            },
            error: (err: any) => {
                console.error('Error loading races:', err);
                this.cdr.markForCheck();
            },
        });

        // Load drivers for dropdown
        this.adminService.getAllDrivers().subscribe({
            next: (drivers: { id: number; name: string; abbreviation: string; teamId: number; teamName: string }[]) => {
                this.availableDrivers = drivers.map((d: { id: number; name: string; abbreviation: string; teamId: number; teamName: string }) => ({
                    id: d.id,
                    name: d.name,
                    teamName: d.teamName,
                }));
                this.isLoadingOptions = false;
                this.cdr.markForCheck();
            },
            error: (err: any) => {
                console.error('Error loading drivers:', err);
                this.isLoadingOptions = false;
                this.cdr.markForCheck();
            },
        });
    }

    // ========================
    // Pagination & Filtering
    // ========================

    onPageChange(page: number): void {
        this.page = page;
        this.loadBets();
    }

    onFilterChange(): void {
        this.page = 1;
        this.loadBets();
    }

    onSearch(): void {
        this.page = 1;
        this.loadBets();
    }

    onSearchKeyDown(event: KeyboardEvent): void {
        if (event.key === 'Enter') {
            this.onSearch();
        }
    }

    clearSearch(): void {
        this.searchTerm = '';
        this.page = 1;
        this.loadBets();
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
        if (this.filterStatus === null) return 'All Statuses';
        return this.betStatuses.find((s: { value: BetStatus; label: string }) => s.value === this.filterStatus)?.label ?? 'All Statuses';
    }

    // ========================
    // Create Bet
    // ========================

    openCreateModal(): void {
        this.showCreateModal = true;
        this.createSuccess = false;
        this.createError = '';
        this.createForm = {
            userId: '',
            raceId: '',
            driverId: '',
            amount: 0,
            betType: 'RaceWinner',
        };
    }

    closeCreateModal(): void {
        this.showCreateModal = false;
    }

    createBet(): void {
        // Validation
        if (!this.createForm.userId) {
            this.createError = 'User ID is required.';
            return;
        }
        if (!this.createForm.raceId) {
            this.createError = 'Race is required.';
            return;
        }
        if (!this.createForm.driverId) {
            this.createError = 'Driver is required.';
            return;
        }
        if (this.createForm.amount <= 0) {
            this.createError = 'Bet amount must be greater than zero.';
            return;
        }

        const userId = parseInt(this.createForm.userId, 10);
        const raceId = parseInt(this.createForm.raceId, 10);
        const driverId = parseInt(this.createForm.driverId, 10);

        if (isNaN(userId) || isNaN(raceId) || isNaN(driverId)) {
            this.createError = 'Invalid numeric values.';
            return;
        }

        const dto: CreateBetDto = {
            userId,
            raceId,
            driverId,
            amount: this.createForm.amount,
            betType: this.createForm.betType,
        };

        this.isCreating = true;
        this.createSuccess = false;
        this.createError = '';

        this.adminService.createBet(dto).subscribe({
            next: () => {
                this.isCreating = false;
                this.createSuccess = true;
                this.closeCreateModal();
                this.loadBets();
                this.cdr.markForCheck();
                setTimeout(() => {
                    this.createSuccess = false;
                    this.cdr.markForCheck();
                }, 5000);
            },
            error: (error: any) => {
                this.isCreating = false;
                this.createError = error.message || 'Failed to create bet';
                this.cdr.markForCheck();
            },
        });
    }

    // ========================
    // Edit Bet
    // ========================

    openEditModal(bet: AdminBetResponseDto): void {
        this.editingBet = { ...bet };
        this.editForm = {
            driverId: bet.driverId,
            amount: bet.amount,
            betType: bet.betType,
            status: bet.status,
            winnings: bet.winnings ?? 0,
        };
        this.isPartialEdit = false;
        this.showEditModal = true;
        this.editSuccess = false;
        this.editError = '';
    }

    closeEditModal(): void {
        this.showEditModal = false;
        this.editingBet = null;
    }

    onEditFormChange(): void {
        // Mark as partial edit when any field is changed
        if (this.editingBet) {
            this.isPartialEdit = true;
        }
    }

    updateBet(): void {
        if (!this.editingBet) return;

        const dto: UpdateBetDto = {};

        if (this.isPartialEdit) {
            dto.driverId = this.editForm.driverId;
            dto.amount = this.editForm.amount;
            dto.betType = this.editForm.betType;
            dto.status = this.editForm.status;
            dto.winnings = this.editForm.winnings;
        }

        if (Object.keys(dto).length === 0) {
            this.editError = 'No changes to save.';
            return;
        }

        this.isEditing = true;
        this.editSuccess = false;
        this.editError = '';

        this.adminService.updateBet(this.editingBet.id, dto).subscribe({
            next: () => {
                this.isEditing = false;
                this.editSuccess = true;
                this.closeEditModal();
                this.loadBets();
                this.cdr.markForCheck();
                setTimeout(() => {
                    this.editSuccess = false;
                    this.cdr.markForCheck();
                }, 5000);
            },
            error: (error: any) => {
                this.isEditing = false;
                this.editError = error.message || 'Failed to update bet';
                this.cdr.markForCheck();
            },
        });
    }

    // ========================
    // Delete Bet
    // ========================

    openDeleteConfirm(betId: number): void {
        this.deletingBetId = betId;
        this.showDeleteConfirm = true;
        this.deleteError = '';
    }

    closeDeleteConfirm(): void {
        this.showDeleteConfirm = false;
        this.deletingBetId = null;
    }

    deleteBet(): void {
        if (!this.deletingBetId) return;

        this.isDeleting = true;
        this.deleteError = '';

        this.adminService.deleteBet(this.deletingBetId).subscribe({
            next: () => {
                this.isDeleting = false;
                this.showDeleteConfirm = false;
                this.deletingBetId = null;
                this.loadBets();
                this.cdr.markForCheck();
            },
            error: (error: any) => {
                this.isDeleting = false;
                this.deleteError = error.message || 'Failed to delete bet';
                this.cdr.markForCheck();
            },
        });
    }

    // ========================
    // Utility Methods
    // ========================

    formatDate(date: string | null): string {
        if (!date) return '—';
        return new Date(date).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    }

    getBetTypeLabel(betType: BetType): string {
        return this.betTypeLabels[betType] || betType;
    }

    getDriverName(driverId: number): string {
        return this.availableDrivers.find((d) => d.id === driverId)?.name ?? `Driver ${driverId}`;
    }

    getRaceName(raceId: number): string {
        return this.availableRaces.find((r) => r.id === raceId)?.name ?? `Race ${raceId}`;
    }

    getUserName(userId: number): string {
        const bet = this.bets.find((b) => b.userId === userId);
        return bet?.username ?? `User ${userId}`;
    }

    getStatusClass(status: BetStatus): string {
        return this.betStatusClasses[status] || 'status-pending';
    }
}
