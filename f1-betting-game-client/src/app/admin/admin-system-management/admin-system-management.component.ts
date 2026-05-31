import { ChangeDetectorRef, Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { AdminBetManagementComponent } from '../components/admin-bet-management/admin-bet-management.component';
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
    QuestDefinitionDto,
    CreateQuestDefinitionDto,
    UpdateQuestDefinitionDto,
    QUEST_CATEGORIES,
    ResetWeekResponseDto,
    PagedResult,
} from '../models/admin.models';

@Component({
    selector: 'app-admin-system-management',
    standalone: true,
    imports: [CommonModule, FormsModule, AdminBetManagementComponent],
    templateUrl: './admin-system-management.component.html',
    styleUrl: './admin-system-management.component.css',
})
export class AdminSystemManagementComponent implements OnInit, OnDestroy {
    // --- Tab Navigation ---
    activeTab: 'sync' | 'results' | 'metadata' | 'races' | 'bets' | 'quests' = 'bets';

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
    allDrivers: { id: number; name: string; teamName: string }[] = [];
    isLoadingDrivers = true;

    // --- Race Management ---
    showCreateRaceForm = false;
    createRaceForm = {
        name: '',
        date: '',
        circuit: '',
        country: '',
        season: 2025,
    };
    isCreatingRace = false;
    createRaceSuccess = false;
    createRaceError = '';
    deleteRaceId: number | null = null;
    showDeleteConfirm = false;
    isDeletingRace = false;
    deleteRaceError = '';

    private syncTimeout: ReturnType<typeof setTimeout> | null = null;

    @ViewChild(AdminBetManagementComponent)
    private betManagementComponent!: AdminBetManagementComponent;

    // ========================
    // Quest Management
    // ========================
    questDefinitions: QuestDefinitionDto[] = [];
    isLoadingQuests = true;
    questPage = 1;
    questPageSize = 20;
    questTotalItems = 0;
    questTotalPages = 0;
    questFilterActive: boolean | null = null;
    questSearchTerm = '';

    // Quest form
    showQuestForm = false;
    isEditingQuest = false;
    editingQuestId: number | null = null;
    questForm: CreateQuestDefinitionDto = {
        questId: '',
        name: '',
        description: '',
        category: 'Betting',
        isOneTime: true,
        target: 1,
        pointsReward: 100,
        order: 1,
        isActive: true,
    };
    isSavingQuest = false;
    questFormError = '';
    questFormSuccess = false;

    // Quest delete
    deleteQuestId: number | null = null;
    showDeleteQuestConfirm = false;
    isDeletingQuest = false;
    deleteQuestError = '';

    // Quest toggle
    togglingQuestId: number | null = null;

    // Quest reset
    showResetQuestConfirm = false;
    isResettingQuests = false;
    resetQuestSuccess = '';
    resetQuestError = '';

    // Quest detail view
    viewQuestId: string | null = null;
    viewQuestCompletedCount = 0;
    isViewingQuest = false;

    constructor(
        private adminService: AdminService,
        private cdr: ChangeDetectorRef
    ) {}

    ngOnInit(): void {
        this.loadRaces();
        this.loadAllDrivers();
    }

    ngOnDestroy(): void {
        if (this.syncTimeout) {
            clearTimeout(this.syncTimeout);
        }
    }

    // ========================
    // Tab Navigation
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

                // Reload races after successful sync
                this.loadRaces();

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
    // Driver Loading Methods
    // ========================

    loadAllDrivers(): void {
        this.isLoadingDrivers = true;

        this.adminService.getAllDrivers().subscribe({
            next: (drivers) => {
                this.allDrivers = drivers.map((d) => ({
                    id: d.id,
                    name: d.name,
                    teamName: d.teamName,
                }));
                this.isLoadingDrivers = false;
                this.cdr.markForCheck();
            },
            error: (error) => {
                console.error('Error loading drivers:', error);
                this.isLoadingDrivers = false;
            },
        });
    }

    // ========================
    // Race Management Methods
    // ========================

    openCreateRaceForm(): void {
        this.showCreateRaceForm = true;
        this.createRaceSuccess = false;
        this.createRaceError = '';
        // Default to tomorrow at 14:00
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        tomorrow.setHours(14, 0, 0, 0);
        const defaultDate = tomorrow.toISOString().slice(0, 16);

        this.createRaceForm = {
            name: '',
            date: defaultDate,
            circuit: '',
            country: '',
            season: 2025,
        };
    }

    closeCreateRaceForm(): void {
        this.showCreateRaceForm = false;
    }

    createRace(): void {
        if (!this.createRaceForm.name?.trim()) {
            this.createRaceError = 'Race name is required.';
            return;
        }
        if (!this.createRaceForm.circuit?.trim()) {
            this.createRaceError = 'Circuit name is required.';
            return;
        }
        if (!this.createRaceForm.country?.trim()) {
            this.createRaceError = 'Country is required.';
            return;
        }

        this.isCreatingRace = true;
        this.createRaceSuccess = false;
        this.createRaceError = '';

        this.adminService.createRace(this.createRaceForm).subscribe({
            next: () => {
                this.createRaceSuccess = true;
                this.isCreatingRace = false;
                this.showCreateRaceForm = false;
                this.loadRaces();
                setTimeout(() => {
                    this.createRaceSuccess = false;
                }, 5000);
            },
            error: (error) => {
                this.createRaceError = error.message || 'Failed to create race';
                this.isCreatingRace = false;
            },
        });
    }

    openDeleteConfirm(raceId: number): void {
        this.deleteRaceId = raceId;
        this.showDeleteConfirm = true;
        this.deleteRaceError = '';
    }

    closeDeleteConfirm(): void {
        this.showDeleteConfirm = false;
        this.deleteRaceId = null;
    }

    deleteRace(): void {
        if (!this.deleteRaceId) return;

        this.isDeletingRace = true;
        this.deleteRaceError = '';

        this.adminService.deleteRace(this.deleteRaceId).subscribe({
            next: () => {
                this.isDeletingRace = false;
                this.showDeleteConfirm = false;
                this.deleteRaceId = null;
                this.loadRaces();
            },
            error: (error) => {
                this.deleteRaceError = error.message || 'Failed to delete race';
                this.isDeletingRace = false;
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
                this.cdr.markForCheck();
            },
            error: (error) => {
                console.error('Error loading races:', error);
                this.isLoadingRaces = false;
                this.cdr.markForCheck();
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

                // Reload races and refresh the form with fresh data
                this.loadRaces();
                // Find the updated race from the refreshed list
                setTimeout(() => {
                    const updatedRace = this.races.find(r => r.id === this.selectedRaceId);
                    if (updatedRace) {
                        this.selectRace(updatedRace);
                    }
                }, 100);

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
        this.resultsSaveError = '';

        this.adminService.getRaceResults(raceId).subscribe({
            next: (results) => {
                this.raceResults = results;
                this.cdr.markForCheck();

                // Initialize editable positions from existing results
                if (results.positions.length > 0) {
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
                } else {
                    // No existing results - start with one empty row
                    this.currentPositions = [{
                        position: 1,
                        driverId: null,
                        driverName: '',
                        teamId: 0,
                        teamName: '',
                        points: 0,
                        fastestLap: null,
                        pitStopTime: null,
                    }];
                }

                // Set fastest lap driver
                this.fastestLapDriverId = results.fastestLapDriverId || null;
            },
            error: (error) => {
                console.error('Error loading race results:', error);
                this.resultsSaveError = error.message || 'Failed to load race results';
            },
        });
    }

    onResultsRaceSelect(): void {
        if (this.selectedRaceId) {
            const race = this.races.find((r) => r.id === this.selectedRaceId);
            if (race) {
                this.selectRace(race);
            }
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

        // Check for duplicate drivers
        const driverIds = this.currentPositions
            .filter((p) => p.driverId != null)
            .map((p) => p.driverId as number);
        const duplicates = driverIds.filter((id, index) => driverIds.indexOf(id) !== index);
        if (duplicates.length > 0) {
            const uniqueDuplicates = [...new Set(duplicates)];
            this.resultsSaveError = `The following drivers are assigned to multiple positions: ${uniqueDuplicates.join(', ')}. Each driver can only occupy one position.`;
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
            positions: this.currentPositions
                .filter((p) => p.driverId != null)
                .map((p) => ({
                    position: p.position,
                    driverId: p.driverId as number,
                })),
            fastestLapDriverId: this.fastestLapDriverId,
        };

        console.log('[Override] Sending DTO:', JSON.stringify(dto, null, 2));
        console.log('[Override] Positions count:', dto.positions.length);

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

    getDriverName(driverId: number | null): string {
        if (!driverId || !this.raceResults) return '—';
        const pos = this.raceResults.positions.find((p) => p.driverId === driverId);
        return pos ? pos.driverName : '—';
    }

    getTeamName(driverId: number | null): string {
        if (!driverId) return '';
        // Try to find from existing results first
        if (this.raceResults) {
            const pos = this.raceResults.positions.find((p) => p.driverId === driverId);
            if (pos) return pos.teamName;
        }
        // Fallback to allDrivers list
        const driver = this.allDrivers.find((d) => d.id === driverId);
        return driver ? driver.teamName : '';
    }

    getPointsForPosition(position: number): number {
        return position <= 10 ? [25, 18, 15, 12, 10, 8, 6, 4, 2, 1][position - 1] : 0;
    }

    onPositionChange(positionIndex: number, driverId: number | null): void {
        if (this.currentPositions[positionIndex]) {
            this.currentPositions[positionIndex].driverId = driverId;
            // Update team name based on selected driver
            if (driverId != null && this.raceResults) {
                const driver = this.raceResults.positions.find((p) => p.driverId === driverId);
                if (driver) {
                    this.currentPositions[positionIndex].teamName = driver.teamName;
                }
            }
        }
    }

    addPosition(): void {
        const newPosition: PositionItemDto = {
            position: this.currentPositions.length + 1,
            driverId: null,
            driverName: '',
            teamId: 0,
            teamName: '',
            points: 0,
            fastestLap: null,
            pitStopTime: null,
        };
        this.currentPositions.push(newPosition);
        this.cdr.markForCheck();
    }

    removePosition(index: number): void {
        this.currentPositions.splice(index, 1);
        // Renumber remaining positions
        this.currentPositions.forEach((pos, i) => {
            pos.position = i + 1;
        });
        this.cdr.markForCheck();
    }

    onFastestLapChange(driverId: number | null): void {
        this.fastestLapDriverId = driverId;
    }

    getExistingDriverIds(): (number | null)[] {
        if (!this.raceResults) return [];
        return this.raceResults.positions.map((p) => p.driverId);
    }

    getAvailableDrivers(): { id: number | null; name: string; teamName: string }[] {
        if (this.isLoadingDrivers) return [];
        return this.allDrivers;
    }

    // ========================
    // Quest Management Methods
    // ========================

    switchTab(tab: 'sync' | 'results' | 'metadata' | 'races' | 'bets' | 'quests'): void {
        this.activeTab = tab;
        this.showDeleteConfirm = false;
        this.showCreateRaceForm = false;
        this.showMetadataConfirmModal = false;
        this.showResultsConfirmModal = false;
        this.showQuestForm = false;
        this.showDeleteQuestConfirm = false;
        this.showResetQuestConfirm = false;
        this.isViewingQuest = false;

        // Load quests when switching to quests tab
        if (tab === 'quests') {
            this.loadQuestDefinitions();
        }
    }

    loadQuestDefinitions(): void {
        this.isLoadingQuests = true;

        this.adminService.getAllQuestDefinitions(
            this.questPage,
            this.questPageSize,
            this.questFilterActive,
            this.questSearchTerm || undefined
        ).subscribe({
            next: (result) => {
                this.questDefinitions = result.items as QuestDefinitionDto[];
                this.questTotalItems = result.totalItems;
                this.questTotalPages = result.totalPages;
                this.isLoadingQuests = false;
                this.cdr.markForCheck();
            },
            error: (error) => {
                console.error('Error loading quest definitions:', error);
                this.isLoadingQuests = false;
            },
        });
    }

    onQuestSearch(): void {
        this.questPage = 1;
        this.loadQuestDefinitions();
    }

    onQuestFilterChange(): void {
        this.questPage = 1;
        this.loadQuestDefinitions();
    }

    onQuestPageChange(page: number): void {
        this.questPage = page;
        this.loadQuestDefinitions();
    }

    openCreateQuestForm(): void {
        this.isEditingQuest = false;
        this.editingQuestId = null;
        this.questForm = {
            questId: '',
            name: '',
            description: '',
            category: 'Betting',
            isOneTime: true,
            target: 1,
            pointsReward: 100,
            order: 1,
            isActive: true,
        };
        this.questFormError = '';
        this.questFormSuccess = false;
        this.showQuestForm = true;
    }

    openEditQuestForm(quest: QuestDefinitionDto): void {
        this.isEditingQuest = true;
        this.editingQuestId = quest.id;
        this.questForm = {
            questId: quest.questId,
            name: quest.name,
            description: quest.description,
            category: quest.category,
            isOneTime: quest.isOneTime,
            target: quest.target,
            pointsReward: quest.pointsReward,
            order: quest.order,
            isActive: quest.isActive,
        };
        this.questFormError = '';
        this.questFormSuccess = false;
        this.showQuestForm = true;
    }

    closeQuestForm(): void {
        this.showQuestForm = false;
        this.isEditingQuest = false;
        this.editingQuestId = null;
    }

    saveQuest(): void {
        // Validate
        if (!this.questForm.questId?.trim()) {
            this.questFormError = 'Quest ID is required.';
            return;
        }
        if (!/^[a-z_]+$/.test(this.questForm.questId)) {
            this.questFormError = 'Quest ID must contain only lowercase letters and underscores.';
            return;
        }
        if (!this.questForm.name?.trim()) {
            this.questFormError = 'Name is required.';
            return;
        }
        if (!this.questForm.description?.trim()) {
            this.questFormError = 'Description is required.';
            return;
        }
        if (!this.questForm.category) {
            this.questFormError = 'Category is required.';
            return;
        }
        if (this.questForm.target <= 0) {
            this.questFormError = 'Target must be greater than 0.';
            return;
        }
        if (this.questForm.pointsReward < 0) {
            this.questFormError = 'Points reward must be greater than or equal to 0.';
            return;
        }

        this.isSavingQuest = true;
        this.questFormError = '';
        this.questFormSuccess = false;

        if (this.isEditingQuest && this.editingQuestId !== null) {
            // Update existing quest
            const updateDto: UpdateQuestDefinitionDto = {
                name: this.questForm.name,
                description: this.questForm.description,
                category: this.questForm.category,
                isOneTime: this.questForm.isOneTime,
                target: this.questForm.target,
                pointsReward: this.questForm.pointsReward,
                order: this.questForm.order,
                isActive: this.questForm.isActive,
            };

            this.adminService.updateQuestDefinition(this.editingQuestId, updateDto).subscribe({
                next: () => {
                    this.questFormSuccess = true;
                    this.isSavingQuest = false;
                    this.showQuestForm = false;
                    this.loadQuestDefinitions();
                    setTimeout(() => { this.questFormSuccess = false; }, 5000);
                },
                error: (error) => {
                    this.questFormError = error.message || 'Failed to update quest';
                    this.isSavingQuest = false;
                },
            });
        } else {
            // Create new quest
            this.adminService.createQuestDefinition(this.questForm).subscribe({
                next: () => {
                    this.questFormSuccess = true;
                    this.isSavingQuest = false;
                    this.showQuestForm = false;
                    this.loadQuestDefinitions();
                    setTimeout(() => { this.questFormSuccess = false; }, 5000);
                },
                error: (error) => {
                    this.questFormError = error.message || 'Failed to create quest';
                    this.isSavingQuest = false;
                },
            });
        }
    }

    openDeleteQuestConfirm(questId: number): void {
        this.deleteQuestId = questId;
        this.showDeleteQuestConfirm = true;
        this.deleteQuestError = '';
    }

    closeDeleteQuestConfirm(): void {
        this.showDeleteQuestConfirm = false;
        this.deleteQuestId = null;
    }

    deleteQuest(): void {
        if (!this.deleteQuestId) return;

        this.isDeletingQuest = true;
        this.deleteQuestError = '';

        this.adminService.deleteQuestDefinition(this.deleteQuestId).subscribe({
            next: () => {
                this.isDeletingQuest = false;
                this.showDeleteQuestConfirm = false;
                this.deleteQuestId = null;
                this.loadQuestDefinitions();
            },
            error: (error) => {
                this.deleteQuestError = error.message || 'Failed to delete quest';
                this.isDeletingQuest = false;
            },
        });
    }

    toggleQuestActive(quest: QuestDefinitionDto): void {
        this.togglingQuestId = quest.id;
        this.adminService.toggleQuestActive(quest.id, !quest.isActive).subscribe({
            next: (updated) => {
                quest.isActive = updated.isActive;
                this.togglingQuestId = null;
                this.cdr.markForCheck();
            },
            error: (error) => {
                this.togglingQuestId = null;
                console.error('Failed to toggle quest:', error);
            },
        });
    }

    openViewQuestProgress(quest: QuestDefinitionDto): void {
        this.viewQuestId = quest.questId;
        this.isViewingQuest = true;
        this.adminService.getQuestCompletedCount(quest.questId).subscribe({
            next: (result) => {
                this.viewQuestCompletedCount = result.completedCount;
            },
            error: (error) => {
                console.error('Failed to get completed count:', error);
            },
        });
    }

    closeViewQuestProgress(): void {
        this.isViewingQuest = false;
        this.viewQuestId = null;
    }

    openResetQuestConfirm(): void {
        this.showResetQuestConfirm = true;
        this.resetQuestSuccess = '';
        this.resetQuestError = '';
    }

    closeResetQuestConfirm(): void {
        this.showResetQuestConfirm = false;
    }

    resetWeeklyQuests(): void {
        this.isResettingQuests = true;
        this.resetQuestSuccess = '';
        this.resetQuestError = '';

        this.adminService.resetWeeklyQuests().subscribe({
            next: (result) => {
                this.resetQuestSuccess = result.message;
                this.isResettingQuests = false;
                this.closeResetQuestConfirm();
                setTimeout(() => { this.resetQuestSuccess = ''; }, 5000);
            },
            error: (error) => {
                this.resetQuestError = error.message || 'Failed to reset weekly quests';
                this.isResettingQuests = false;
            },
        });
    }

    getQuestCategoryEmoji(category: string): string {
        const cat = QUEST_CATEGORIES.find(c => c.value === category);
        return cat ? cat.label.split(' ')[0] : '❓';
    }

    getQuestCategoryColor(category: string): string {
        switch (category) {
            case 'Betting': return '#00d4ff';
            case 'Engagement': return '#4caf50';
            case 'Achievement': return '#ffc107';
            default: return '#888';
        }
    }

    // Expose QUEST_CATEGORIES to the template
    get QUEST_CATEGORIES(): { value: string; label: string }[] {
        return QUEST_CATEGORIES;
    }

    // Generate page numbers for pagination display
    getQuestPageNumbers(): number[] {
        const pages: number[] = [];
        const total = this.questTotalPages;
        const current = this.questPage;

        if (total <= 7) {
            for (let i = 1; i <= total; i++) pages.push(i);
        } else {
            pages.push(1);
            if (current > 3) pages.push(-1); // ellipsis
            const start = Math.max(2, current - 1);
            const end = Math.min(total - 1, current + 1);
            for (let i = start; i <= end; i++) pages.push(i);
            if (current < total - 2) pages.push(-2); // ellipsis
            pages.push(total);
        }
        return pages;
    }
}
