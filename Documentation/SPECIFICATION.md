# Specyfikacja Projektu: f1-betting-game

## 1. Cel projektu i ogólny zarys aplikacji
Aplikacja to internetowa gra bazodanowa pozwalająca użytkownikom na obstawianie wyników wyścigów Formuły 1 na punkty. Każdy zarejestrowany gracz otrzymuje wirtualne saldo, którym dysponuje w celu dokonywania zakładów. Dzięki integracji z publicznym API OpenF1, aplikacja uzyskuje informacje o nadchodzących, trwających oraz zakończonych wyścigach, a także o stanie mistrzostw świata. 

Użytkownicy mogą obstawiać zawody w kilku kategoriach, np.:
* TOP 3 kierowców w wyścigu.
* Kierowca z najszybszym czasem okrążenia wyścigu.
* Zespół z najszybszym pit stopem.
* Liczba kierowców, którzy nie dojechali do mety (DNF).

System przechowuje historię zakładów, udostępnia personalne statystyki oraz prezentuje rankingi graczy (ogólne i okresowe).

---

## 2. Stos Technologiczny (Tech Stack)
* **Frontend:** Angular (aplikacja typu Single Page Application).
* **Backend:** ASP.NET Core Web API w języku C#.
* **Baza Danych:** Relacyjna baza danych Microsoft SQL Server. W komunikacji z bazą wykorzystany zostanie Entity Framework Core jako ORM.
* **Integracje z zewnątrz:** Publiczne API OpenF1 do pobierania wyników.
* **Zadania w tle:** Wbudowany `BackgroundService` lub biblioteka Hangfire.

---

## 3. Wymagania Niefunkcjonalne
Dla zachowania prostoty we wczesnej fazie projektu przyjąto optymalne, ale nieskomplikowane założenia:
* **Wydajność (Performance):** Aplikacja musi sprawnie przetwarzać dużą ilość zapytań o zapis zakładu na krótko przed startem wyścigu. Pula połączeń do bazy (Connection Pooling) w EF Core powinna to zapewnić. Przetwarzanie i przeliczanie punktów odbywa się całkowicie asynchronicznie, co zapobiega spowolnieniom widocznych dla użytkownika zapytań HTTP.
* **Bezpieczeństwo (Security):** System uwierzytelniania wykorzystuje mechanizm JWT (JSON Web Tokens), dodawany do zapytań przez interceptory HTTP w Angularze. Zastosowane zostaną standardowe Guardy chroniące poszczególne widoki. Przechowywane hasła użytkowników muszą być bezpiecznie hashowane (`PasswordHash` w bazie).
* **Skalowalność (Scalability):** Podział na niezależne warstwy (Frontend SPA, Web API, Worker w tle, Baza danych). W pierwszej fazie aplikacja może działać na jednym serwerze (skalowalność wertykalna).
* **Kompatybilność:** Responsywny interfejs użytkownika w Angularze dostosowany do urządzeń desktopowych oraz mobilnych.

---

## 4. Architektura, Przepływ Danych i Endpointy API

Aplikacja zbudowana jest w architekturze klient-serwer. Backend podzielony jest na trzy warstwy (Clean Architecture): Prezentacji (API), Logiki Biznesowej (Application) i Danych (Infrastructure).

**Przykładowe endpointy Web API:**
Zgodnie ze strukturą kontrolerów, wystawione zostaną:
* `POST /api/auth/register` - rejestracja konta.
* `POST /api/auth/login` - logowanie i wydanie tokena JWT.
* `GET /api/races` - pobranie kalendarza wyścigów dla modułu `F1 Data Module`.
* `POST /api/bets` - utworzenie nowego zakładu.
* `GET /api/leaderboard` - pobranie ogólnego rankingu dla modułu `Dashboard`.

**Przepływ Danych (Zapis i Rozliczanie Zakładów):**
1. **Zapis:** Klient Angular wysyła żądanie z tokenem JWT poprzez interceptor. API w warstwie prezentacji odbiera żądanie, po czym warstwa logiki weryfikuje zasady (np. walidacja czasu zakładu). Na końcu EF Core zapisuje encję `Bet` w tabeli `Bets`.
2. **Przetwarzanie w tle (Background Jobs):** System wykorzystuje zadania uruchamiane w tle zamiast reagować na odświeżanie strony przez użytkowników. `Job 1` cyklicznie sprawdza status wyścigu i po jego zakończeniu pobiera oficjalne wyniki z OpenF1. Następnie uruchamiany jest `Job 2`, który iteruje po nierozstrzygniętych zakładach, oblicza punkty i aktualizuje pola `TotalPoints` graczy oraz zapisuje stan w tabeli `LeaderboardHistory`.

---

## 5. Plan Testowania

Biorąc pod uwagę rozmiar projektu i integrację zewnętrzną, przyjmiemy uproszczoną, ale skuteczną piramidę testów:
* **Testy Jednostkowe (Unit Testing):** Weryfikacja warstwy logiki biznesowej (`F1BettingApp.Application`). Testowanie walidatorów (np. blokada zakładu sekundę po starcie) oraz kalkulatora punktacji (poprawne naliczanie przy trafnym typie Top 3 vs częściowym trafnym typie).
* **Testy Integracyjne (Integration Testing):** Testy na styku warstw: weryfikacja czy zapytania do bazy poprzez Entity Framework (ORM) zapisują prawidłowe relacje między zakładami a użytkownikami. Weryfikacja parsera danych odbierającego mockowane odpowiedzi z API OpenF1.
* **Testy Wydajnościowe (Performance Testing):** Skrypty obciążeniowe (np. za pomocą k6) symulujące duży ruch na endpoincie `/api/bets` na kilka minut przed zablokowaniem sesji zakładów.
* **Testy Akceptacyjne (User Acceptance Testing - UAT):** Ręczne przejście przez krytyczne ścieżki w aplikacji z perspektywy gracza: założenie konta, wpłacenie punktów, poprawne wyświetlanie wyścigów w kalendarzu, oddanie ważnego zakładu i weryfikacja salda po symulowanym zakończeniu wyścigu.

---

## 6. Plan Deploymentu

Zaproponowano najprostszy model wdrożeniowy, pozwalający zredukować koszty utrzymania bez użycia złożonych usług chmurowych (jak Kubernetes), opierający się na jednym maszynie:
* **Serwer:** Standardowy wirtualny serwer prywatny (VPS) z systemem Linux (np. Ubuntu).
* **Baza Danych:** Zespół użyje instancji Microsoft SQL Server Express Edition zainstalowanej bezpośrednio na środowisku docelowym.
* **Certyfikaty:** Darmowe certyfikaty Let's Encrypt (Certbot) do zabezpieczenia całości komunikacji szyfrowaniem HTTPS.
