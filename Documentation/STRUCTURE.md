F1BettingApp/
├── F1BettingApp.API/            # Warstwa Prezentacji (Web API)
│   ├── Controllers/             # Endpointy (BetsController, RacesController, AuthController)
│   ├── Middleware/              # Obsługa błędów, Auth logic
│   ├── Program.cs               # Konfiguracja DI, Swaggera i potoku HTTP
│   └── appsettings.json
├── F1BettingApp.Application/    # Warstwa Logiki Biznesowej (Core)
│   ├── Interfaces/              # Interfejsy serwisów i repozytoriów
│   ├── Services/                # Implementacja logiki (BettingService, RankingService)
│   ├── DTOs/                    # Obiekty transferu danych (RaceDto, UserPointsDto)
│   ├── Mappings/                # Konfiguracja AutoMapper
│   └── Validators/              # Walidacja (np. FluentValidation - czy wyścig już wystartował?)
├── F1BettingApp.Infrastructure/ # Warstwa Danych i Zewnętrznych Integracji
│   ├── Persistence/             # Entity Framework Core (AppDbContext, Migrations)
│   ├── Repositories/            # Implementacja dostępu do bazy
│   ├── OpenF1/                  # Klient do API OpenF1 (HttpClient, modele API)
│   └── BackgroundJobs/          # Hangfire lub BackgroundService (RaceResultWorker)
└── F1BettingApp.Domain/         # Warstwa Domenowa (Encje)
    ├── Entities/                # User, Bet, Race, Driver, Team
    ├── Enums/                   # BetStatus (Pending, Won, Lost), RaceStatus
    └── Exceptions/              # Specyficzne błędy domenowe

src/app/
├── core/                        # Singletony (serwisy API, Guardy, Interceptory)
│   ├── services/                # ApiService, AuthService, SignalRService
│   ├── guards/                  # AuthGuard
│   └── interceptors/            # AuthInterceptor (dodawanie tokena JWT)
├── shared/                      # Komponenty współdzielone (Navbar, Footer, UI-kit, Pipes)
├── features/                    # Moduły funkcjonalne (lazy-loaded)
│   ├── auth/                    # Logowanie i rejestracja
│   ├── betting/                 # Lista wyścigów i formularz obstawiania
│   ├── leaderboard/             # Tabela wyników i profile użytkowników
│   └── race-details/            # Szczegółowe info o danym GP
├── models/                      # Interfejsy TypeScript (Race, Bet, User)
└── styles/                      # Globalne style i zmienne SASS