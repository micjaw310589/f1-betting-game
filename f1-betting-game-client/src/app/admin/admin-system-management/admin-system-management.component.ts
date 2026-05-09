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
    UpdateRaceMetadataDto,
    RACE_STATUSES,
} from '../models/admin.models';

@Component({
    selector: 'app-admin-system-management',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './admin-system-management.component.html',
    styleUrl: './admin-system-management.component.css',
})
export class AdminSystemManagementComponent implements OnInit, OnDestroy {
    // --- Tab Navigation ---
    activeTab: 'sync' | 'results' | 'metadata' = 'results';

    // --- Sync Section ---
    isSyncing = false;
    syncResult: SyncResultDto | null = null;
    syncError = '';

    // --- Race Selection ---
    races: AdminRaceDto[] = [];
    isLoadingRaces = true;
    selectedRaceId: number | null = null;
    selectedRace: AdminRaceDto | null = null;
    currentResults: AdminRaceResultDto | null = null;

    // --- Race Results Override ---
    isSavingResults = false;
    resultsSaveSuccess = false;
    resultsSaveError = '';
    overridePositions: PositionEntryDto[] = [];
    fastestLapDriverId: number | null = null;
    showResultsConfirmModal = false;
    availableDrivers: { id: number; name: string }[] = [];

    // --- Race Metadata Override ---
    isSavingMetadata = false;
    metadataSaveSuccess = false;
    metadataSaveError = '';
    showMetadataConfirmModal = false;
    metadataForm: UpdateRaceMetadataDto = {};

    // --- Driver options for the override form ---
    driverOptions: { id: number; name: string }[] = [];

    // Modal state for results
    showOverrideConfirm = false;

    private syncTimeout: ReturnType<typeof setTimeout> | null = null;

    constructor(private adminService: AdminService) {}

    ngOnInit(): void {
        this.loadRaces();
        // Load driver options for forms
        this.loadDriverOptions();
    }

    ngOnDestroy(): void {
        if (this.syncTimeout) {
            clearTimeout(this.syncTimeout);
        }
    }

    // ========================
    // Tab Navigation
    // ========================

    switchTab(tab: 'sync' | 'results' | 'metadata'): void {
        this.activeTab = tab;
    }

    // ========================
    // Driver Options
    // ========================

    loadDriverOptions(): void {
        // Pre-populate driver options (1-20) for the override forms
        this.driverOptions = Array.from({ length: 20 }, (_, i) => ({
            id: i + 1,
            name: `Driver ${i + 1}`,
        }));
    }

    getDriverName(driverId: number): string {
        const driver = this.driverOptions.find((d) => d.id === driverId);
        return driver ? driver.name : `Driver #${driverId}`;
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
            this.metadataForm = {};
            this.resultsSaveSuccess = false;
            this.resultsSaveError = '';
            this.metadataSaveSuccess = false;
            this.metadataSaveError = '';
        }
    }

    selectRace(race: AdminRaceDto): void {
        this.selectedRaceId = race.id;
        this.selectedRace = race;
        this.resultsSaveSuccess = false;
        this.resultsSaveError = '';
        this.metadataSaveSuccess = false;
        this.metadataSaveError = '';
        this.showResultsConfirmModal = false;
        this.showMetadataConfirmModal = false;

        // Reset forms
        this.overridePositions = [];
        this.fastestLapDriverId = null;
        this.metadataForm = {};

        // Load current results
        this.adminService.getRaceResults(race.id).subscribe({
            next: (results) => {
                this.currentResults = results;
                this.buildResultsOverrideForm(results);
                this.buildMetadataForm(race, results);
            },
            error: (error) => {
                console.error('Error loading race results:', error);
                this.currentResults = null;
                this.overridePositions = [];
                this.fastestLapDriverId = null;
            },
        });
    }

    // ========================
    // Results Override Form
    // ========================

    private buildResultsOverrideForm(results: AdminRaceResultDto): void {
        // Build positions from existing results - populate with current data if available
        this.overridePositions = [];

        // We'll let the admin fill in all positions
        // Pre-populate with a default set of positions (1-10)
        for (let i = 1; i <= 10; i++) {
            this.overridePositions.push({
                position: i,
                driverId: i,
            });
        }
    }

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

    openResultsConfirmModal(): void {
        if (this.overridePositions.length === 0) {
            this.resultsSaveError = 'Please add at least one position.';
            return;
        }
        this.showOverrideConfirm = true;
    }

    closeResultsConfirmModal(): void {
        this.showOverrideConfirm = false;
    }

    confirmResultsOverride(): void {
        if (!this.selectedRaceId) return;

        this.isSavingResults = true;
        this.resultsSaveSuccess = false;
        this.resultsSaveError = '';
        this.showOverrideConfirm = false;

        const dto: OverrideRaceResultDto = {
            positions: this.overridePositions,
            fastestLapDriverId: this.fastestLapDriverId,
        };

        this.adminService.overrideRaceResults(this.selectedRaceId, dto).subscribe({
            next: () => {
                this.resultsSaveSuccess = true;
                this.isSavingResults = false;

                // Reload races and results
                this.loadRaces();
                this.selectRace(this.selectedRace!);

                // Auto-clear success message after 5 seconds
                setTimeout(() => {
                    this.resultsSaveSuccess = false;
                }, 5000);
            },
            error: (error) => {
                this.resultsSaveError = error.message || 'Failed to override race results';
                this.isSavingResults = false;
            },
        });
    }

    // ========================
    // Metadata Override Form
    // ========================

    private buildMetadataForm(race: AdminRaceDto, results: AdminRaceResultDto): void {
        // Format date for datetime-local input (YYYY-MM-DDTHH:MM)
        let formattedDate: string | null = null;
        if (race.raceDate) {
            const d = new Date(race.raceDate);
            formattedDate = d.toISOString().slice(0, 16);
        }
        this.metadataForm = {
            name: race.name,
            date: formattedDate,
            circuit: race.circuit,
            country: race.country,
            status: race.status,
        };
    }

    openMetadataConfirmModal(): void {
        if (!this.selectedRaceId) {
            this.metadataSaveError = 'Please select a race first.';
            return;
        }
        if (!this.metadataForm.name?.trim()) {
            this.metadataSaveError = 'Race name is required.';
            return;
        }
        if (!this.metadataForm.circuit?.trim()) {
            this.metadataSaveError = 'Circuit name is required.';
            return;
        }
        if (!this.metadataForm.country?.trim()) {
            this.metadataSaveError = 'Country is required.';
            return;
        }
        this.showMetadataConfirmModal = true;
    }

    closeMetadataConfirmModal(): void {
        this.showMetadataConfirmModal = false;
    }

    confirmMetadataUpdate(): void {
        if (!this.selectedRaceId) return;

        this.isSavingMetadata = true;
        this.metadataSaveSuccess = false;
        this.metadataSaveError = '';
        this.showMetadataConfirmModal = false;

        this.adminService.updateRaceMetadata(this.selectedRaceId, this.metadataForm).subscribe({
            next: () => {
                this.metadataSaveSuccess = true;
                this.isSavingMetadata = false;

                // Reload races and refresh the form
                this.loadRaces();
                this.selectRace(this.selectedRace!);

                // Auto-clear success message after 5 seconds
                setTimeout(() => {
                    this.metadataSaveSuccess = false;
                }, 5000);
            },
            error: (error) => {
                this.metadataSaveError = error.message || 'Failed to update race metadata';
                this.isSavingMetadata = false;
            },
        });
    }

    // ========================
    // Utility Methods
    // ========================

    formatDate(date: Date | string | null | undefined): string {
        if (!date) return 'N/A';
        const d = new Date(date);
        return d.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    }

    formatDateTime(date: Date | string | null): string {
        if (!date) return '';
        const d = new Date(date);
        return d.toISOString().slice(0, 16);
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
            case 'cancelled':
                return 'status-cancelled';
            case 'postponed':
                return 'status-postponed';
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

    getRaceStatusOptions(): typeof RACE_STATUSES {
        return RACE_STATUSES;
    }
}
