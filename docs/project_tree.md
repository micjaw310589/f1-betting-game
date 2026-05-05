# F1 Betting Game - Project Structure

This document represents the complete folder and file tree of the F1 Betting Game project.

## Root Directory

```
f1-betting-game/
├── .gitignore
├── Dockerfile
├── f1-betting-game.sln
├── plantuml.jar
├── README.md
├── docs/
├── f1-betting-game-client/
└── F1BettingApp/
```

---

## docs/ - Documentation

```
docs/
├── SPECIFICATION.md
├── architecture/
│   ├── component_diagram.puml
│   ├── deployment_diagram.puml
│   ├── class_diagram.puml
│   ├── F1BettingGame_DomainModel.png
│   └── project_tree.puml
├── sequences/
│   ├── bet_placement.puml
│   ├── leaderboard_update.puml
│   ├── race_result_processing.puml
│   └── user_registration.puml
└── tasks/
```

---

## f1-betting-game-client/ - Angular Frontend

```
f1-betting-game-client/
├── .editorconfig
├── .gitignore
├── .prettierrc
├── angular.json
├── package.json
├── package-lock.json
├── proxy.conf.json
├── README.md
├── tsconfig.app.json
├── tsconfig.json
├── tsconfig.spec.json
├── vercel.json
├── public/
│   └── favicon.ico
└── src/
    ├── index.html
    ├── main.ts
    ├── styles.css
    ├── app/
    │   ├── app.config.ts
    │   ├── app.css
    │   ├── app.html
    │   ├── app.routes.ts
    │   ├── app.spec.ts
    │   ├── app.ts
    │   └── race/
    └── environments/
        ├── environment.development.ts
        └── environment.ts
```

---

## F1BettingApp/ - ASP.NET Core Backend

```
F1BettingApp/
├── F1BettingApp.API/
│   ├── appsettings.json
│   ├── F1BettingApp.API.csproj
│   ├── Program.cs
│   └── Controllers/
│       ├── AuthController.cs
│       ├── BetsController.cs
│       ├── LeaderboardController.cs
│       ├── RacesController.cs
│       └── UsersController.cs
├── F1BettingApp.Application/
│   ├── F1BettingApp.Application.csproj
│   ├── DTOs/
│   │   ├── AuthDto.cs
│   │   ├── BetDto.cs
│   │   ├── BetHistoryDto.cs
│   │   ├── BetHistoryResponseDto.cs
│   │   ├── BetResponseDto.cs
│   │   ├── HistoricalLeaderboardDto.cs
│   │   ├── LeaderboardEntryDto.cs
│   │   ├── NotificationDto.cs
│   │   ├── PlaceBetDto.cs
│   │   ├── RaceDetailDto.cs
│   │   ├── RaceDto.cs
│   │   ├── RaceResultDto.cs
│   │   ├── RaceSummaryDto.cs
│   │   ├── UserDto.cs
│   │   ├── UserPointsDto.cs
│   │   ├── UserProfileDto.cs
│   │   ├── UserRankingDto.cs
│   │   └── UserStatisticsDto.cs
│   ├── Exceptions/
│   │   └── BettingExceptions.cs
│   ├── Interfaces/
│   │   ├── IBettingService.cs
│   │   ├── ILeaderboardService.cs
│   │   ├── INotificationService.cs
│   │   ├── IRaceService.cs
│   │   └── IUserService.cs
│   └── Services/
│       ├── BettingService.cs
│       ├── LeaderboardService.cs
│       ├── NotificationService.cs
│       ├── RaceService.cs
│       └── UserService.cs
├── F1BettingApp.Domain/
├── F1BettingApp.Infrastructure/
└── F1BettingApp.Tests/
```

---

## Project Overview

| Component | Technology | Description |
|-----------|------------|-------------|
| **docs/** | Markdown/PlantUML | Documentation, diagrams, and specifications |
| **f1-betting-game-client/** | Angular 17+ | Responsive web frontend with lazy-loaded modules |
| **F1BettingApp.API/** | ASP.NET Core 8 | RESTful API endpoints |
| **F1BettingApp.Application/** | .NET 8 | Business logic, DTOs, and service layer |
| **F1BettingApp.Domain/** | .NET 8 | Domain entities and models |
| **F1BettingApp.Infrastructure/** | .NET 8 | Data access and external API integration |
| **F1BettingApp.Tests/** | .NET 8 | Unit and integration tests |