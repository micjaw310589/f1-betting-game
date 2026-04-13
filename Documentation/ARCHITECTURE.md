# Architektura

tl;dr:
- Architektura klient-serwer.
- Frontend: **Angular**; podzielony na 4 główne moduły.
- Backend: **ASP.NET Core Web API (C#)**, architektura 3-warstwowa (Clean Architecture) + Background Workers
- Repozytorium: Relacyjna baza danych **Microsoft SQL Server** + połączenie z zewnętrznym API (OpenF1)

## Frontend
**Angular**; Single Page Application (SPA).

Podział na moduły:
- **Auth Module**: Rejestracja, logowanie, zarządzanie profilem.
- **Betting Module**: Widok nadchodzących wyścigów, formularze obstawiania (kto wygra, kto złoży najszybsze okrążenie, DNF itp.).
- **Dashboard / Leaderboard Module**: Tabela punktacji graczy, historia ich zakładów, globalne statystyki.
- **F1 Data Module**: Kalendarz wyścigów, informacje o kierowcach i zespołach (dane tylko do odczytu).

Komunikacja:\
\
Interceptory HTTP (do automatycznego dodawania tokenów JWT do zapytań backendowych oraz globalnej obsługi błędów).

## Backend
**ASP.NET Core Web API** (obsługa zapytań użytkowników) oraz procesy w tle (**Background Workers**) do komunikacji z API OpenF1.

Podział na warstwy:
- Warstwa Prezentacji - **Web API**: Wystawia endpointy RESTful dla Angulara. Rozpoznaje użytkowników na podstawie tokenów JWT.
- Warstwa Logiki Biznesowej - **Core/Services**: Tutaj znajdują się zasady gry:
  - _Czy użytkownik zdążył obstawić przed startem wyścigu? (Walidacja czasu)._
  - _Jak punktowane są dokładne trafienia, a jak częściowe?_
- Warstwa Danych - **Infrastructure**: Wykorzystanie **Entity Framework Core** jako ORM do komunikacji z bazą danych.

### Background Jobs - KLUCZOWY ELEMENT!
Zamiast pobierać dane z OpenF1 kiedy użytkownik odświeża stronę, zostanie wykorzystane procesowanie w tle (IHostedService / BackgroundService).
- Job 1 (Cykliczny): Sprawdza status wyścigu. Jeśli wyścig się zakończył (oficjalne wyniki), pobiera dane z OpenF1.
- Job 2 (Procesujący): Na podstawie pobranych wyników, iteruje po wszystkich nierozstrzygniętych zakładach w Twojej bazie, oblicza punkty i aktualizuje rankingi graczy.

## Baza danych
**Microsoft SQL Server**.
Baza będzie pełniła rolę repozytorium na dane użytkowników oraz zakładów, a także będzie służyć jako cache dla danych pobieranych z OpenF1.

Przykładowa struktura:
- `Users` (Id, Username, Email, PasswordHash, TotalPoints)
- `Drivers` / `Teams` (Zsynchronizowane z OpenF1, żeby móc przypisywać je do zakładów na własnym backendzie).
- `Races` (Id, Name, Date, Status - np. Scheduled, Finished, ResultsProcessed).
- `Bets` (Id, UserId, RaceId, DriverId_Prediction, FastLap_Prediction, PointsAwarded, Status).
- `LeaderboardHistory` (Zapisywanie pozycji graczy po każdym wyścigu, żeby np. rysować na frontendzie wykres formy gracza).
