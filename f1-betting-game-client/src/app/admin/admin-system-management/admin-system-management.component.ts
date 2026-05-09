import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../services/admin.service';
import {
    SyncResultDto,
    AdminRaceDto,
    AdminRaceResultDto,
    PositionEntryDto,
    OverrideRaceResultDto,
} from '../models/admin.models';

@Component({
    selector: 'app-admin-system-management',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './admin-system-management.component.html',
    styleUrl: './admin-system-management.component.css',
})
export class AdminSystemManagementComponent implements OnInit, OnDestroy {
    // --- Sync Section ---
    isSyncing = false;
    syncResult: SyncResultDto | null = null;
    syncError = '';

    // --- Race Override Section ---
    races: AdminRaceDto[] = [];
    isLoadingRaces = true;
    selectedRaceId: number | null = null;
    selectedRace: AdminRaceDto | null = null;
    currentResults: AdminRaceResultDto | null = null;
    isSavingResults = false;
    saveSuccess = false;
    saveError = '';

    // Override form data
    overridePositions: PositionEntryDto[] = [];
    fastestLapDriverId: number | null = null;

    // Driver options for the override form
    availableDrivers: { id: number; name: string }[] = [];

    // Modal state
    showOverrideConfirm = false;

    private syncTimeout: ReturnType<typeof setTimeout> | null = null;

    constructor(private adminService: AdminService) {}

    ngOnInit(): void {
        this.loadRaces();
    }

    ngOnDestroy(): void {
        if (this.syncTimeout) {
            clearTimeout(this.syncTimeout);
        }
    }

    // ========================
    // Sync Methods
    // ========================

    triggerSync(): void {
        this.isSyncing = true;
        this.syncResult = null;
        this.syncError = '';

        this.adminService.triggerSync().subscribe({
            next: (result) => {
                this.syncResult = result;
                this.isSyncing = false;

                // Auto-clear sync result after 10 seconds
                this.syncTimeout = setTimeout(() => {
                    this.syncResult = null;
                }, 10000);
            },
            error: (error) => {
                this.syncError = error.message || 'Sync failed';
                this.isSyncing = false;
            },
        });
    }

    // ========================
    // Race Loading Methods
    // ========================

    loadRaces(): void {
        this.isLoadingRaces = true;

        this.adminService.getAllRaces().subscribe({
            next: (races) => {
                this.races = races;
                this.isLoadingRaces = false;
            },
            error: (error) => {
                console.error('Error loading races:', error);
                this.isLoadingRaces = false;
            },
        });
    }

    onRaceSelect(): void {
        if (this.selectedRaceId) {
            const race = this.races.find((r) => r.id === this.selectedRaceId);
            if (race) {
                this.selectRace(race);
            }
        } else {
            this.selectedRace = null;
            this.currentResults = null;
            this.overridePositions = [];
            this.fastestLapDriverId = null;
            this.saveSuccess = false;
            this.saveError = '';
        }
    }

    selectRace(race: AdminRaceDto): void {
        this.selectedRaceId = race.id;
        this.selectedRace = race;
        this.saveSuccess = false;
        this.saveError = '';
        this.showOverrideConfirm = false;

        // Load current results
        this.adminService.getRaceResults(race.id).subscribe({
            next: (results) => {
                this.currentResults = results;
                this.buildOverrideForm(results);
            },
            error: (error) => {
                console.error('Error loading race results:', error);
                this.currentResults = null;
                // Create empty form for races without results
                this.overridePositions = [];
                this.fastestLapDriverId = null;
            },
        });
    }

    private buildOverrideForm(results: AdminRaceResultDto): void {
        // Build positions from existing results - we need to fetch the actual results
        // For now, we'll build an empty form that the admin can fill in
        this.overridePositions = [];

        // Try to extract positions from the existing data if available
        // The RaceResultDto has winner info but not full positions
        // We'll let the admin fill in all positions
    }

    // ========================
    // Override Form Methods
    // ========================

    addPosition(): void {
        const nextPosition = this.overridePositions.length + 1;
        this.overridePositions.push({
            position: nextPosition,
            driverId: 0,
        });
    }

    removePosition(index: number): void {
        this.overridePositions.splice(index, 1);
        // Reposition remaining entries
        this.overridePositions = this.overridePositions.map((p, i) => ({
            ...p,
            position: i + 1,
        }));
    }

    openConfirmModal(): void {
        if (this.overridePositions.length === 0) {
            this.saveError = 'Please add at least one position.';
            return;
        }
        this.showOverrideConfirm = true;
    }

    closeConfirmModal(): void {
        this.showOverrideConfirm = false;
    }

    confirmOverride(): void {
        if (!this.selectedRaceId) return;

        this.isSavingResults = true;
        this.saveSuccess = false;
        this.saveError = '';
        this.showOverrideConfirm = false;

        const dto: OverrideRaceResultDto = {
            positions: this.overridePositions,
            fastestLapDriverId: this.fastestLapDriverId,
        };

        this.adminService.overrideRaceResults(this.selectedRaceId, dto).subscribe({
            next: () => {
                this.saveSuccess = true;
                this.isSavingResults = false;

                // Reload races and results
                this.loadRaces();
                this.selectRace(this.selectedRace!);

                // Auto-clear success message after 5 seconds
                setTimeout(() => {
                    this.saveSuccess = false;
                }, 5000);
            },
            error: (error) => {
                this.saveError = error.message || 'Failed to override race results';
                this.isSavingResults = false;
            },
        });
    }

    // ========================
    // Utility Methods
    // ========================

    formatDate(date: Date | null): string {
        if (!date) return 'N/A';
        return new Date(date).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    }

    getStatusClass(status: string): string {
        switch (status.toLowerCase()) {
            case 'scheduled':
                return 'status-scheduled';
            case 'inprogress':
                return 'status-inprogress';
            case 'finished':
                return 'status-finished';
            case 'resultsprocessed':
                return 'status-processed';
            default:
                return '';
        }
    }

    getStatusLabel(status: string): string {
        return status
            .replace(/([A-Z])/g, ' $1')
            .trim()
            .toUpperCase();
    }

    hasResults(race: AdminRaceDto): boolean {
        return race.isManuallyOverridden;
    }

    getDriverName(driverId: number): string {
        const driver = this.availableDrivers.find((d) => d.id === driverId);
        return driver ? driver.name : `Driver #${driverId}`;
    }
}
