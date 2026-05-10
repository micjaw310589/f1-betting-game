import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../services/admin.service';
import {
    SyncResultDto,
    AdminRaceDto,
    UpdateRaceMetadataDto,
    RACE_STATUSES,
    RaceResultDto,
    PositionItemDto,
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
    // --- Tab Navigation ---
    activeTab: 'sync' | 'results' | 'metadata' = 'sync';

    // --- Sync Section ---
    isSyncing = false;
    syncResult: SyncResultDto | null = null;
    syncError = '';

    // --- Race Selection ---
    races: AdminRaceDto[] = [];
    isLoadingRaces = true;
    selectedRaceId: number | null = null;
    selectedRace: AdminRaceDto | null = null;

    // --- Race Results Override ---
    raceResults: RaceResultDto | null = null;
    isFetchingResults = false;
    isSavingResults = false;
    resultsSaveSuccess = false;
    resultsSaveError = '';
    showResultsConfirmModal = false;
    currentPositions: PositionItemDto[] = [];
    fastestLapDriverId: number | null = null;

    // --- Race Metadata Override ---
    isSavingMetadata = false;
    metadataSaveSuccess = false;
    metadataSaveError = '';
    showMetadataConfirmModal = false;
    metadataForm: UpdateRaceMetadataDto = {};

    // --- Driver List for Select ---
    availableDrivers: { id: number; name: string; teamId: number; teamName: string }[] = [];
    isLoadingDrivers = false;

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
    // Tab Navigation
    // ========================

    switchTab(tab: 'sync' | 'results' | 'metadata'): void {
        this.activeTab = tab;
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
            this.metadataForm = {};
            this.metadataSaveSuccess = false;
            this.metadataSaveError = '';
        }
    }

    selectRace(race: AdminRaceDto): void {
        this.selectedRaceId = race.id;
        this.selectedRace = race;
        this.metadataSaveSuccess = false;
        this.metadataSaveError = '';
        this.showMetadataConfirmModal = false;
        this.resultsSaveSuccess = false;
        this.resultsSaveError = '';
        this.showResultsConfirmModal = false;

        // Reset form
        this.metadataForm = {};
        this.buildMetadataForm(race);

        // Load race results if on results tab
        if (this.activeTab === 'results') {
            this.loadRaceResults(race.id);
        }
    }

    // ========================
    // Metadata Override Form
    // ========================

    private buildMetadataForm(race: AdminRaceDto): void {
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
    // Race Results Methods
    // ========================

    loadRaceResults(raceId: number): void {
        this.isFetchingResults = true;
        this.raceResults = null;
        this.resultsSaveError = '';

        this.adminService.getRaceResults(raceId).subscribe({
            next: (results) => {
                this.raceResults = results;
                this.isFetchingResults = false;

                // Initialize editable positions from existing results
                this.currentPositions = results.positions.map((p) => ({
                    position: p.position,
                    driverId: p.driverId,
                    driverName: p.driverName,
                    teamId: p.teamId,
                    teamName: p.teamName,
                    points: p.points,
                    fastestLap: p.fastestLap,
                    pitStopTime: p.pitStopTime,
                }));

                // Set fastest lap driver
                this.fastestLapDriverId = results.fastestLapDriverId || null;
            },
            error: (error) => {
                console.error('Error loading race results:', error);
                this.isFetchingResults = false;
                this.resultsSaveError = error.message || 'Failed to load race results';
            },
        });
    }

    onResultsRaceSelect(): void {
        if (this.selectedRaceId) {
            this.loadRaceResults(this.selectedRaceId);
        } else {
            this.raceResults = null;
            this.currentPositions = [];
            this.resultsSaveError = '';
        }
    }

    openResultsConfirmModal(): void {
        if (!this.selectedRaceId) {
            this.resultsSaveError = 'Please select a race first.';
            return;
        }
        if (this.currentPositions.length === 0) {
            this.resultsSaveError = 'At least one position is required.';
            return;
        }
        this.showResultsConfirmModal = true;
    }

    closeResultsConfirmModal(): void {
        this.showResultsConfirmModal = false;
    }

    confirmResultsOverride(): void {
        if (!this.selectedRaceId) return;

        this.isSavingResults = true;
        this.resultsSaveSuccess = false;
        this.resultsSaveError = '';
        this.showResultsConfirmModal = false;

        const dto: OverrideRaceResultDto = {
            positions: this.currentPositions.map((p) => ({
                position: p.position,
                driverId: p.driverId,
            })),
            fastestLapDriverId: this.fastestLapDriverId,
        };

        this.adminService.overrideRaceResults(this.selectedRaceId, dto).subscribe({
            next: () => {
                this.resultsSaveSuccess = true;
                this.isSavingResults = false;

                // Reload results and races
                this.loadRaceResults(this.selectedRaceId!);
                this.loadRaces();

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

    // --- Helper methods for results form ---

    getDriverName(driverId: number): string {
        if (!this.raceResults) return 'Unknown';
        const pos = this.raceResults.positions.find((p) => p.driverId === driverId);
        return pos ? pos.driverName : 'Unknown';
    }

    getTeamName(driverId: number): string {
        if (!this.raceResults) return '';
        const pos = this.raceResults.positions.find((p) => p.driverId === driverId);
        return pos ? pos.teamName : '';
    }

    getPointsForPosition(position: number): number {
        return position <= 10 ? [25, 18, 15, 12, 10, 8, 6, 4, 2, 1][position - 1] : 0;
    }

    onPositionChange(positionIndex: number, driverId: number): void {
        if (this.currentPositions[positionIndex]) {
            this.currentPositions[positionIndex].driverId = driverId;
        }
    }

    onFastestLapChange(driverId: number | null): void {
        this.fastestLapDriverId = driverId;
    }

    getExistingDriverIds(): number[] {
        if (!this.raceResults) return [];
        return this.raceResults.positions.map((p) => p.driverId);
    }

    getAvailableDrivers(): { id: number; name: string; teamName: string }[] {
        if (!this.raceResults) return [];
        const existingIds = this.getExistingDriverIds();
        // Get all unique drivers from the positions list
        const allDrivers = this.raceResults.positions.map((p) => ({
            id: p.driverId,
            name: p.driverName,
            teamName: p.teamName,
        }));
        // Remove duplicates
        return allDrivers.filter(
            (driver, index, self) =>
                index === self.findIndex((d) => d.id === driver.id)
        );
    }
}
