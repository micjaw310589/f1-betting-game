### Specyfikacja projektu: `f1-betting-game`

---

#### 1. Cel i ogólny zarys aplikacji
Aplikacja `f1-betting-game` to internetowa gra bazodanowa pozwalająca użytkownikom na obstawianie wyników wyścigów Formuły 1 na punkty. Każdy zarejestrowany gracz otrzymuje wirtualne saldo, którym dysponuje w celu dokonywania zakładów związanych z przebiegiem wyścigów. Dzięki integracji z publicznym API OpenF1, aplikacja jest na bieżąco z aktualnym kalendarzem, stanem mistrzostw oraz wynikami.

#### 2. Stos technologiczny
* **Frontend:** Angular (SPA).
* **Backend:** ASP.NET Core Web API (C#).
* **Baza Danych:** Microsoft SQL Server (Relacyjna baza danych).
* **Zewnętrzne integracje:** API OpenF1.

---

#### 3. Główne funkcjonalności
System oferuje użytkownikom szereg możliwości w ramach gry oraz przeglądania statystyk:
* **Obstawianie wyścigów:** Możliwość typowania wielu zdarzeń, w tym: TOP 3 kierowców w wyścigu, kierowcy z najszybszym okrążeniem, zespołu z najszybszym pit stopem oraz liczby kierowców, którzy nie ukończą zawodów (DNF). Zestaw tych kategorii może być w przyszłości rozbudowywany.
* **Dane F1:** Dostęp do informacji o nadchodzących, trwających i zakończonych wyścigach oraz podgląd stanu mistrzostw świata. 
* **Statystyki gracza:** Śledzenie własnej historii zakładów oraz przeglądanie spersonalizowanych statystyk.
* **Rankingi (Leaderboards):** Wyświetlanie tabeli wyników graczy w ujęciu ogólnym oraz w określonych przedziałach czasowych (np. z ostatniego miesiąca).

---

#### 4. Architektura systemu i podział na moduły
Projekt bazuje na architekturze klient-serwer z wyraźnym podziałem odpowiedzialności. 

**Frontend (Angular)**
Aplikacja typu Single Page Application (SPA) podzielona na moduły funkcjonalne (tzw. _lazy-loaded features_):
* **Auth Module (`auth`):** Zarządzanie profilem, rejestracja oraz logowanie.
* **Betting Module (`betting`):** Formularze umożliwiające obstawianie zdarzeń (zwycięzcy, DNF itp.) oraz widok nadchodzących wyścigów.
* **Dashboard / Leaderboard Module (`leaderboard`):** Prezentacja tabel punktacyjnych, historii zakładów oraz statystyk globalnych.
* **F1 Data Module (`race-details`):** Karta informacyjna z kalendarzem wyścigów i danymi zespołów (tryb tylko do odczytu).
* *Komunikacja:* Aplikacja wykorzystuje interceptory HTTP do wstrzykiwania tokenów JWT i globalnego zarządzania błędami.

**Backend (C# ASP.NET Core)**
Zaprojektowany w oparciu o architekturę trójwarstwową (Clean Architecture):
* **Warstwa Prezentacji (API):** Odpowiada za wystawianie endpointów RESTful (np. `BetsController`, `RacesController`) oraz uwierzytelnianie na bazie JWT.
* **Warstwa Logiki Biznesowej (Application/Core):** Miejsce na zasady gry, walidację (np. czy wyścig już wystartował) i zasady punktacji (dokładne i częściowe trafienia). Przechowuje interfejsy, DTOs i serwisy.
* **Warstwa Danych i Infrastruktury (Infrastructure):** Komunikuje się z bazą SQL Server poprzez Entity Framework Core jako ORM oraz implementuje klienta do API OpenF1.
* **Warstwa Domenowa (Domain):** Zawiera modele encji (User, Bet, Race, Driver, Team), enumeratory definiujące statusy oraz specyficzne wyjątki domenowe.

---

#### 5. Przetwarzanie asynchroniczne (Background Jobs)
Kluczowym założeniem architektury jest odciążenie zapytań użytkowników poprzez zastosowanie procesów w tle (BackgroundService lub Hangfire).
* **Job 1 (Cykliczny monitor):** Sprawdza status wyścigów. Po zakończeniu Grand Prix i ogłoszeniu oficjalnych wyników, pobiera je z API OpenF1.
* **Job 2 (Procesowanie wyników):** Wykorzystuje dane pobrane przez Job 1, przechodzi przez nierozstrzygnięte zakłady w systemie, kalkuluje punkty i dodaje je do rankingów graczy.

---

#### 6. Struktura bazy danych
Baza danych (Microsoft SQL Server) służy jako podstawowe repozytorium danych użytkowników oraz jako warstwa _cache_ dla informacji pobieranych z OpenF1. Główne encje w systemie to:
* `Users`: Id, Username, Email, PasswordHash, TotalPoints.
* `Drivers` / `Teams`: Dane zsynchronizowane z OpenF1, niezbędne do działania formularzy typowania.
* `Races`: Id, Name, Date, Status (Scheduled, Finished, ResultsProcessed).
* `Bets`: Id, UserId, RaceId, DriverId_Prediction, FastLap_Prediction, PointsAwarded, Status.
* `LeaderboardHistory`: Przechowuje archiwalne pozycje graczy po każdym wyścigu, co pozwala na generowanie m.in. wykresów formy.
