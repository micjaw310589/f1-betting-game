# Specyfikacja Funkcjonalna – F1 Betting Game

## 1. Słownik
W niniejszym dokumencie zastosowano następujące definicje i skróty terminologii branżowej oraz systemowej:
* **Wirtualne punkty (Virtual Points)** – Wewnętrzna waluta gry przydzielana użytkownikom po rejestracji (stan początkowy: 10 000 punktów), służąca wyłącznie do zawierania zakładów wewnątrz platformy. Punkty te nie posiadają żadnej wartości pieniężnej i nie mogą być wymieniane na prawdziwą walutę.
* **Typ zakładu (Bet Type)** – Zdefiniowana kategoria przewidywania wyniku sportowego (np. zwycięzca wyścigu, TOP 3 kierowców, TOP 10 kierowców, najszybsze okrążenie).
* **Kurs (Odds)** – Stały współczynnik określający mnożnik wygranej punktowej w przypadku poprawnego wytypowania wyniku.
* **OpenF1 API** – Zewnętrzny publiczny interfejs programistyczny aplikacji (API), stanowiący jedyne źródło prawdy dla danych dotyczących kalendarza F1, wyników wyścigów, klasyfikacji generalnej oraz szczegółów dotyczących kierowców i zespołów.
* **Status wyścigu (Race Status)** – Stan, w jakim znajduje się dany wyścig w bazie danych systemu. Dopuszczalne wartości to: *Scheduled* (Zaplanowany), *Finished* (Zakończony), *ResultsProcessed* (Rozliczony).
* **Status zakładu (Bet Status)** – Stan, w jakim znajduje się zakład użytkownika. Dopuszczalne wartości to: *Pending* (Oczekujący), *Won* (Wygrany), *Lost* (Przegrany), *Cancelled* (Anulowany).
* **Sezon (Season)** – Okres rywalizacji pokrywający się z rzeczywistym sezonem Mistrzostw Świata Formuły 1, kończący się resetem kont i tabeli przez Administratora.
* **JWT (JSON Web Token)** – Standard bezpiecznego przekazywania informacji autoryzacyjnych użytkownika pomiędzy frontendem a backendem w postaci podpisanego cyfrowo tokenu.
* **ROI (Return on Investment)** – Wskaźnik zwrotu z inwestycji punktowej, określający efektywność obstawiania użytkownika, wyrażony w procentach.
* **DNF (Did Not Finish)** – Status kierowcy, który rozpoczął wyścig, lecz nie ukończył go z powodu awarii mechanicznej, wypadku lub innej sytuacji losowej.
* **Zadanie/Wyzwanie (Quest)** – Aktywne wyzwanie systemowe podzielone na kategorie (Betting, Engagement, Achievement), którego ukończenie nagradzane jest dodatkowymi wirtualnymi punktami.
* **Interceptor HTTP (AuthInterceptor)** – Mechanizm frontendowy automatycznie przechwytujący każde wychodzące żądanie HTTP w celu dołączenia nagłówka autoryzacyjnego z tokenem JWT oraz obsługi błędów uwierzytelniania.
* **Strażnik trasy (Guard / adminGuard)** – Kod zabezpieczający we frontendzie, blokujący ładowanie komponentów i nawigację do tras zastrzeżonych (np. panelu administratora) dla użytkowników bez wymaganych uprawnień (ról).

## 2. Założenia projektu
Projekt `f1-betting-game` to internetowa platforma bazodanowa o charakterze grywalizacyjnym, przeznaczona dla fanów Formuły 1. Głównym celem systemu jest dostarczenie angażującej, bezpłatnej rozrywki polegającej na typowaniu wyników weekendów wyścigowych przy użyciu punktów wirtualnych.
System opiera się na architekturze rozproszonej: nowoczesnym kliencie webowym oraz stabilnym i bezpiecznym API backendowym z bazą danych. Kluczowym elementem działania systemu jest zautomatyzowana integracja z OpenF1 API, co eliminuje konieczność ręcznego wprowadzania danych o kierowcach, torach i wynikach przez obsługę, zapewniając jednocześnie realizację założeń w czasie zbliżonym do rzeczywistego. Platforma dostarcza również podgląd statystyk indywidualnych graczy poprzez analitykę własnych wyników oraz system motywacyjny oparty na zadaniach (Quests) i bonusach za regularne logowanie.

## 3. Interesariusze
W procesie tworzenia, wdrażania oraz eksploatacji systemu F1 Betting Game zdefiniowano następujących interesariuszy:
1. **Gracze (Zarejestrowani Użytkownicy)** – Osoby fizyczne korzystające z aplikacji za pośrednictwem przeglądarek internetowych. Ich celem jest zabawa, testowanie wiedzy o F1, zdobywanie punktów oraz analiza własnych statystyk.
2. **Administratorzy Systemu** – Osoby odpowiedzialne za utrzymanie ciągłości działania platformy, moderację użytkowników, nadzór nad poprawnością rozliczeń zakładów oraz zarządzanie parametrami globalnymi systemu (np. reset sezonu, manualne korekty).
3. **Zewnętrzny Dostawca Danych (OpenF1 API)** – Podmiot dostarczający publiczne, darmowe API z danymi telemetrycznymi i wynikowymi Formuły 1, którego stabilność i struktura danych mają bezpośredni wpływ na automatyczne procesy aplikacji.
4. **Właściciel Projektu / Deweloperzy** – Zespół odpowiedzialny za techniczną implementację, skalowalność, bezpieczeństwo danych oraz rozwój kodu zgodnie z przyjętą architekturą.

## 4. Aktorzy
System wchodzi w interakcję z czterema głównymi aktorami (trzema ludzkimi oraz jednym systemowym):
* **Gość (Użytkownik Niezalogowany)** – Może przeglądać publiczne sekcje aplikacji: kalendarz wyścigów, klasyfikacja kierowców, tablicę zadań (ale bez podglądu postępów). Nie ma możliwości obstawiania ani wglądu w profil.
* **Gracz (Użytkownik Zalogowany)** – Posiada pełen dostęp do funkcji gry: obstawianie wyścigów przed ich rozpoczęciem, dostęp do szczegółów własnego profilu, przeglądanie historii zakładów, odbieranie bonusów za dzienne logowanie, śledzenie postępów w zadaniach oraz analiza własnych statystyk.
* **Administrator** – Posiada uprawnienia Gracza oraz pełen dostęp do zabezpieczonego panelu `/admin/*`. Może przeglądać i blokować konta użytkowników, korygować salda, anulować zakłady z błędnymi kursami, ręcznie wymusić synchronizację danych oraz nadpisać oficjalne wyniki wyścigu w sytuacjach awaryjnych.
* **System** – Cykliczne procesy działające w tle backendu. Odpowiadają za pobieranie danych z OpenF1 API, wykrywanie zakończenia wyścigów, automatyczne rozliczanie zakładów, modyfikację sald, aktualizację tabeli liderów oraz wysyłanie powiadomień.

## 5. Scenariusze
Poniżej przedstawiono kluczowe scenariusze użycia systemu (Use Cases) opisujące interakcje aktorów z aplikacją:

### Scenariusz 1: Obstawienie nowego zakładu (Gracz)
1. Gracz loguje się do systemu i przechodzi do modułu kalendarza wyścigów.
2. Gracz wybiera nadchodzący wyścig o statusie *Scheduled*.
3. System wyświetla komponent `BetPlacementComponent` zawierający formularz z dostępnymi typami zakładów (Zwycięzca, TOP 3, Fastest Lap itp.) oraz aktualnymi kursami.
4. Gracz uzupełnia swoje typy i wprowadza stawkę punktową (w ramach dostępnego salda konta).
5. Gracz klika przycisk "Place Bet".
6. System przeprowadza walidację (czy wyścig się nie rozpoczął, czy stawka <= saldo).
7. System zapisuje zakład w bazie ze statusem *Pending*, potrąca punkty z salda gracza i wyświetla komunikat o sukcesie.

### Scenariusz 2: Automatyczne rozliczenie wyścigu po jego zakończeniu (System)
1. Proces tła *Race Status Monitor* co 5 minut sprawdza stan rzeczywistego wyścigu w OpenF1 API.
2. Wykryte zostaje zakończenie wyścigu – system zmienia wewnętrzny status na *Finished*.
3. Uruchamiany jest *Result Processing Job*, który pobiera oficjalne wyniki (pozycje kierowców, najszybsze okrążenie, DNF).
4. System wyszukuje w bazie danych wszystkie zakłady o statusie *Pending* powiązane z tym wyścigiem.
5. Dla każdego zakładu system porównuje typowania użytkownika z oficjalnymi wynikami:
   - W przypadku pełnego trafienia oblicza wygraną: `stawka * kurs`.
   - W przypadku zakładu wielopozycyjnego (np. podium) oblicza częściową wygraną zgodnie z regułami biznesowymi.
   - W przypadku braku trafienia zmienia status na *Lost*.
6. System aktualizuje saldo punktowe użytkowników, którzy wygrali, oraz zmienia statusy zakładów na *Won* lub *Lost*.
7. System przelicza pozycje w tabeli liderów (`LeaderboardHistory`).
8. Zmiana statusu wyścigu na *ResultsProcessed* kończy proces. Po zalogowaniu użytkownicy otrzymują powiadomienia o rozliczeniu.

### Scenariusz 3: Interwencja Administratora w przypadku błędu kursu (Administrator)
1. Administrator loguje się i wchodzi do panelu administracyjnego pod adres `/admin/bets`.
2. Za pomocą filtrów odnajduje nierozliczone zakłady (*Pending*) dla wybranego wyścigu, w których doszło do awarii algorytmu generowania kursów (np. kurs wyniósł 500.0 zamiast 5.0).
3. Administrator klika opcję "Anuluj zakład" przy błędnych pozycjach.
4. System wyświetla okno modalne z żądaniem potwierdzenia i podania przyczyny.
5. Po zatwierdzeniu system zmienia status zakładu na *Cancelled*.
6. Wirtualne punkty stanowiące stawkę anulowanego zakładu zostają automatycznie zwrócone na konta poszkodowanych graczy.
7. System zapisuje zdarzenie w logach audytowych.

## 6. Wymagania funkcjonalne
Sekcja zawiera szczegółowy opis wymagań funkcjonalnych systemu z podziałem na 13 modułów i komponentów technicznych zdefiniowanych w architekturze aplikacji:

### 6.1 Przeglądanie listy wyścigów (`RaceListComponent`)
* **Opis**: Użytkownik (zarówno zalogowany, jak i gość) musi mieć możliwość przeglądania pełnego kalendarza wyścigów Formuły 1 zaimportowanego z OpenF1 API.
* **Wymagania szczegółowe**:
  * Prezentacja wyścigów w formie czytelnej listy lub kafelków, zawierających: nazwę Grand Prix, nazwę toru (circuit), datę i godzinę rozpoczęcia oraz aktualny status.
  * Udostępnienie paska filtrowania z zakładkami: "Wszystkie", "Nadchodzące" (status *Scheduled*), "Zakończone" (status *Finished* lub *ResultsProcessed*).
  * Implementacja paginacji danych oraz mechanizmu "wirtualnej listy" (virtual scrolling) w celu optymalizacji renderowania przy dużej liczbie rekordów w sezonie.
* **Kryteria akceptacji**:
  * Filtry działają natychmiastowo, poprawnie separując wyścigi na podstawie daty systemowej i statusu w bazie danych.
  * W przypadku braku rekordów spełniających kryteria, komponent wyświetla wycentrowany komunikat: *"Brak dostępnych wyścigów"*.
  * **Obsługa Edge Case (Błąd API / Bazy)**: W przypadku awarii sieci lub braku odpowiedzi serwera, lista ukrywa pusty stan, wyświetla dedykowany alert o błędzie połączenia oraz udostępnia widoczny przycisk *"Ponów próbę"*, który ponownie inicjuje żądanie HTTP.

### 6.2 Wyświetlanie szczegółów wyścigu (`RaceDetailComponent`)
* **Opis**: Wyświetlenie kompletnych, pogłębionych informacji o konkretnym weekendzie wyścigowym wybranym przez użytkownika.
* **Wymagania szczegółowe**:
  * Widok musi prezentować: pełną nazwę wydarzenia, mapę/schemat toru (opcjonalnie jako asset), szczegółowy harmonogram (treningi, kwalifikacje, sprint, wyścig główny) dostosowany do strefy czasowej użytkownika.
  * Komponent stanowi kontener, który dla wyścigów nadchodzących osadza moduł zawierania zakładów (`BetPlacementComponent`), a dla wyścigów zakończonych – moduł prezentacji oficjalnych wyników.
* **Kryteria akceptacji**:
  * Wszystkie daty sesji są poprawnie sformatowane i wyświetlane zgodnie z lokalnymi ustawieniami regionalnymi przeglądarki.
  * **Obsługa Edge Case (Nieprawidłowy URL / ID)**: Jeżeli użytkownik ręcznie zmodyfikuje pasek adresu URL i wprowadzi nieistniejące ID wyścigu (np. `/races/99999` lub `/races/abc`), system nie może wygenerować błędu skryptu (crash aplikacji). Router Angular musi przechwycić błąd unikalnego identyfikatora, wyświetlić komunikat o braku zasobu i automatycznie przekierować użytkownika na stronę błędu 404 lub powrócić do listy wyścigów z komunikatem typu Toast.

### 6.3 Obstawianie zakładów (`BetPlacementComponent`)
* **Opis**: Interfejs formularza umożliwiający graczom alokację posiadanych wirtualnych punktów na konkretne zdarzenia w nadchodzącym wyścigu.
* **Wymagania szczegółowe**:
  * Formularz udostępnia pola wyboru dla kierowców i zespołów pobieranych dynamicznie z bazy (synchronizacja z OpenF1).
  * Rodzaje typowań: Wygrany, TOP 3 (podium), Najszybsze okrążenie (Fastest Lap), Pozycje w TOP 10, Liczba DNF.
  * Dynamiczne prezentowanie aktualnego wskaźnika kursu oraz wyliczenie potencjalnej wygranej w punktach na podstawie wprowadzonej stawki przed zatwierdzeniem.
* **Kryteria akceptacji**:
  * Każde pomyślne zatwierdzenie formularza wysyła transakcję do backendu, która natychmiast odejmuje zadeklarowaną stawkę punktową z salda użytkownika w pamięci aplikacji i bazie danych.
  * **Obsługa Edge Case (Typowanie po czasie / Zablokowanie)**: Jeśli użytkownik otworzy formularz przed wyścigiem, ale kliknie "Zatwierdź" po rzeczywistej godzinie startu sesji, backend odrzuca żądanie z błędem walidacji. Na froncie przycisk "Obstaw" staje się natychmiast nieaktywny (*disabled*), a nad nim pojawia się czerwony komunikat: *"Obstawianie tego wyścigu zostało zamknięte"*.
  * **Obsługa Edge Case (Użytkownik Niezalogowany)**: Próba wejścia w interakcję z formularzem przez użytkownika niezalogowanego skutkuje natychmiastowym przerwaniem operacji, zapisaniem intencji powrotu (ReturnURL) i przekierowaniem na ekran logowania.

### 6.4 Zarządzanie profilem użytkownika (`UserProfileComponent`)
* **Opis**: Centralny kokpit (dashboard) użytkownika prezentujący jego status w grze, tożsamość oraz postępy w grywalizacji.
* **Wymagania szczegółowe**:
  * Prezentacja: Nazwy użytkownika, adresu e-mail, aktualnego całkowitego salda punktów wirtualnych.
  * Wyświetlanie sekcji z podsumowaniem aktywnych zadań (Quests) i informacją o statusie dziennego bonusu za logowanie (Daily Streak Info).
* **Kryteria akceptacji**:
  * Wszystkie dane profilowe oraz stan konta są aktualne i pobierane przy każdym wejściu na widok.
  * **Obsługa Edge Case (Brak aktywnych wyzwań)**: Jeśli w danym okresie system nie posiada włączonych zadań dla gracza, sekcja zadań nie może pozostać pusta ani zaburzać układu graficznego – system wyświetla dedykowany element zastępczy (placeholder) z tekstem: *"Brak aktywnych zadań w tym tygodniu"*.
  * **Obsługa Edge Case (Błąd połączenia z API)**: W przypadku awarii komunikacji sieciowej podczas pobierania profilu, aplikacja blokuje interfejs maską błędu z informacją o problemie technicznym oraz wyświetla przycisk ponownego ładowania.

### 6.5 Historia zakładów użytkownika (`UserBetsComponent`)
* **ID**: RF-PROF-02
* **Nazwa**: Przegląd historii zakładów.
* **Opis**: Chronologiczna ewidencja wszystkich kuponów i zakładów zawartych przez danego użytkownika od momentu utworzenia konta.
* **Kryteria akceptacji**:
  * Lista obsługuje pełne stronicowanie (paginację po stronie serwera – Server-Side Pagination) w celu minimalizacji zużycia pamięci.
  * Gracz ma do dyspozycji filtry statusu: "Wszystkie", "Wygrane" (*Won*), "Przegrane" (*Lost*), "Oczekujące" (*Pending*), "Anulowane" (*Withdrawn*/*Cancelled*).
  * **Obsługa Edge Case (Pusta historia)**: Jeśli nowy użytkownik wchodzi do tego modułu po raz pierwszy i nie zawarł jeszcze żadnego zakładu, system ukrywa nagłówki tabeli i generuje czytelny komunikat: *"Nie posiadasz jeszcze historii zakładów. Przejdź do kalendarza, aby obstawić najbliższy wyścig"*.
  * **Obsługa Edge Case (Awaria bazy danych)**: Błąd ładowania danych skutkuje wyświetleniem komunikatu: *"Wystąpił problem podczas pobierania danych o zakładach. Spróbuj ponownie później"*.

### 6.6 Statystyki i Analityka (`UserStatsComponent` & `UserAnalyticsComponent`)
* **Opis**: Moduł dostarczający użytkownikowi matematycznej i statystycznej analizy jego zachowań oraz skuteczności w typowaniu wyników.
* **Wymagania szczegółowe**:
  * Kalkulacja i prezentacja wskaźników: Procent wygranych (Win Rate), całkowity wskaźnik zwrotu (ROI), najdłuższa seria zwycięstw (Winning Streak).
  * Wizualizacja rozkładu zysków i strat w podziale na kierowców, zespoły konstruktorów oraz tory wyścigowe za pomocą wykresów lub tabel rankingowych.
  * Możliwość filtrowania statystyk według zakresu dat (TimeRangePicker).
* **Kryteria akceptacji**:
  * Wszystkie obliczenia matematyczne muszą być odporne na błędy logiczne i prezentowane w przejrzysty sposób.
  * **Obsługa Edge Case (Brak danych / Nowe konto)**: W przypadku braku zawartych lub rozliczonych zakładów (brak dzielnika w operacjach matematycznych), system chroni aplikację przed krytycznym błędem `Division by zero`. Wszystkie wskaźniki procentowe (Win Rate, ROI) muszą w takim scenariuszu bezpiecznie wyświetlać wartość `"0%"` lub komunikat `"Brak danych do kalkulacji"`.
  * **Obsługa Edge Case (Nieprawidłowy zakres dat)**: Jeśli w filtrze czasu użytkownik wybierze datę końcową wcześniejszą niż data początkowa, mechanizm walidacji formularza na frontendzie blokuje wysyłanie zapytania, podświetla pola na czerwono i wyświetla komunikat: *"Data końcowa nie może być wcześniejsza niż data początkowa"*.

### 6.7 Uwierzytelnianie użytkownika (`LoginComponent` & `RegisterComponent`)
* **Opis**: Moduł odpowiedzialny za kontrolę dostępu do systemu, rejestrację nowych kont i bezpieczne logowanie.
* **Wymagania szczegółowe**:
  * Formularz rejestracji wymaga podania unikalnej nazwy użytkownika, poprawnego adresu e-mail oraz silnego hasła spełniającego kryteria bezpieczeństwa (min. 8 znaków, cyfra, znak specjalny, wielka litera).
  * Formularz logowania uwierzytelnia użytkownika na podstawie e-maila i hasła, zwracając token JWT i Refresh Token.
  * Klient Angular bezpiecznie przechowuje tokeny w pamięci aplikacji lub bezpiecznych ciasteczkach (Secure/SameSite Cookies).
* **Kryteria akceptacji**:
  * Walidacja pól (np. poprawność formatu e-mail regex, zgodność haseł) odbywa się w czasie rzeczywistym przed aktywacją przycisku wysyłania.
  * **Obsługa Edge Case (Błędne poświadczenia)**: Wprowadzenie złego hasła lub e-maila skutkuje wyświetleniem komunikatu błędu z backendu: *"Nieprawidłowy adres e-mail lub hasło"*. Pola formularza nie są czyszczone, umożliwiając szybką poprawkę.
  * **Obsługa Edge Case (Zajęty adres e-mail)**: Próba rejestracji na e-mail istniejący w bazie zwraca kod 400 z komunikatem: *"Ten adres e-mail jest już zarejestrowany w systemie"*.
  * **Obsługa Edge Case (Utrata sieci podczas autoryzacji)**: Odcięcie internetu w trakcie wysyłania formularza generuje dedykowany komunikat o braku stabilnego połączenia sieciowego.

### 6.8 Zarządzanie sesją i bezpieczeństwo (`AuthInterceptor`)
* **Opis**: Przechwytywacz HTTP (Interceptor) działający na poziomie rdzenia aplikacji, automatyzujący zarządzanie nagłówkami bezpieczeństwa.
* **Wymagania szczegółowe**:
  * Interceptor automatycznie wstrzykuje nagłówek `Authorization: Bearer <JWT_TOKEN>` do każdego zapytania wychodzącego do chronionych punktów końcowych API.
  * Implementacja nasłuchiwania błędów odpowiedzi – w przypadku przechwycenia kodu HTTP 401 (Unauthorized), interceptor wstrzymuje wykonywanie kolejnych zapytań i inicjuje asynchroniczne żądanie do punktu `/api/auth/refresh` w celu odświeżenia sesji za pomocą Refresh Tokena.
* **Kryteria akceptacji**:
  * Po udanym odświeżeniu pierwotne zapytanie HTTP, które zwróciło błąd 401, jest ponawiane z nowym tokenem JWT, a użytkownik nie doświadcza przerwy w pracy z aplikacją.
  * **Obsługa Edge Case (Wygasły token odświeżania)**: Jeśli proces odświeżania tokena nie powiedzie się (np. Refresh Token również wygasł lub został unieważniony), interceptor musi natychmiast wyczyścić lokalny stan sesji, wywołać procedurę wylogowania i przekierować użytkownika do ekranu logowania z komunikatem Toast: *"Sesja wygasła. Zaloguj się ponownie"*.
  * **Obsługa Edge Case (Pętla zapytań / Zapobieganie deadlockom)**: W przypadku jednoczesnego wysłania wielu zapytań asynchronicznych (np. 5 żądań na raz przy ładowaniu pulpitu), interceptor musi użyć flagi blokującej `isRefreshing` oraz obiektu `Subject`. Pierwsze zapytanie inicjuje odświeżanie, a pozostałe 4 oczekują na nowy token, zapobiegając wygenerowaniu lawiny powtarzalnych żądań odświeżenia sesji i przeciążeniu serwera.

### 6.9 Zarządzanie użytkownikami (`AdminUserManagementComponent`)
* **Opis**: Panel administracyjny dedykowany do kontroli kont użytkowników i moderacji społeczności gry.
* **Wymagania szczegółowe**:
  * Wyświetlanie pełnej listy zarejestrowanych użytkowników z wyszukiwarką po nazwie i adresie e-mail oraz filtrami statusu konta (Aktywne, Zablokowane, Zawieszone).
  * Udostępnienie akcji moderacyjnych: "Zawieś konto" (określenie ram czasowych), "Zablokuj permanentnie", "Koryguj saldo punktów".
* **Kryteria akceptacji**:
  * Każda akcja modyfikująca status użytkownika jest natychmiast odzwierciedlana w bazie danych i skutkuje unieważnieniem tokenów JWT sesji zbanowanego użytkownika.
  * **Obsługa Edge Case (Autoblokada)**: System posiada twarde zabezpieczenie uniemożliwiające zalogowanemu administratorowi wykonanie akcji zablokowania, zawieszenia lub usunięcia uprawnień administratora wobec samego siebie lub innych kont z rolą `SuperAdmin`. Przycisk akcji dla tych pozycji jest ukryty lub nieaktywny.
  * **Obsługa Edge Case (Błąd pobierania danych)**: Awaria bazy danych generuje komunikat z opcją przeładowania struktury tabeli.

### 6.10 Zarządzanie systemem (`AdminSystemManagementComponent`)
* **Opis**: Konsola operacyjna pozwalająca na ręczną kontrolę stanów integracji oraz danych sportowych.
* **Wymagania szczegółowe**:
  * Udostępnienie przycisków do manualnego wyzwolenia zadań synchronizacji (Sync Wyścigów, Sync Klasyfikacji, Sync Kierowców).
  * Interfejs do wprowadzania ręcznych korekt lub wpisania oficjalnych wyników wyścigu w przypadku awarii API OpenF1 (formularz Override).
  * Dostęp do wbudowanego modułu przeglądania logów systemowych i błędów synchronizacji.
* **Kryteria akceptacji**:
  * Ręczne wprowadzenie wyników uruchamia kaskadowe rozliczenie zakładów graczy dokładnie tak samo, jak automatyczny proces tła.
  * **Obsługa Edge Case (Nadpisywanie w trakcie rozliczeń)**: Jeśli administrator próbuje wywołać funkcję manualnego nadpisania wyników (*Override*) dla wyścigu, którego status wskazuje na trwający proces automatycznego rozliczania zakładów przez serwer, system przerywa akcję, wyświetla ostrzeżenie w oknie modalnym i wymaga wpisania specjalnego kodu autoryzacyjnego lub jawnego potwierdzenia, zapobiegając uszkodzeniu spójności relacji bazodanowych.

### 6.11 Zarządzanie zakładami (`AdminBetManagementComponent`)
* **Opis**: Narzędzie nadzorcze pozwalające na monitorowanie globalnego wolumenu zakładów i eliminowanie błędów kursowych.
* **Wymagania szczegółowe**:
  * Widok prezentujący zestawienie wszystkich zakładów zawartych w systemie z możliwością filtrowania po ID wyścigu, ID użytkownika oraz statusie.
  * Funkcja bezpiecznego anulowania kuponu ze statusem *Pending* przed rozliczeniem wyścigu.
* **Kryteria akceptacji**:
  * Anulowanie zakładu skutkuje bezwarunkowym zwrotem 100% postawionych punktów wirtualnych na konto gracza.
  * **Obsługa Edge Case (Blokada anulowania rozliczonego zakładu)**: Jeśli zakład posiada już status *Won* lub *Lost* (został formalnie rozliczony), przycisk "Anuluj" obok tego rekordu zostaje permanentnie zablokowany. Próba przesłania żądania anulowania rozliczonego zakładu bezpośrednio na endpoint API zwraca błąd krytyczny biznesowy, informując, że rozliczonych transakcji nie można cofnąć bez procedury korekty salda w module użytkowników.

### 6.12 Bezpieczeństwo panelu (`adminGuard`)
* **Opis**: Programistyczna ochrona zasobów administracyjnych przed nieuprawnionym dostępem (zarówno z poziomu UI, jak i manipulacji routingiem).
* **Wymagania szczegółowe**:
  * Implementacja mechanizmu `CanActivate` zintegrowanego z modułem routingu Angular dla ścieżki `/admin`.
  * Guard dekoduje token JWT zapisany w sesji i weryfikuje obecność wartości `Admin` w tablicy ról użytkownika.
* **Kryteria akceptacji**:
  * Dostęp do zasobów panelu jest przyznawany wyłącznie użytkownikom posiadającym rolę administracyjną.
  * **Obsługa Edge Case (Próba nieautoryzowanego dostępu)**: Jeżeli zwykły gracz lub użytkownik niezalogowany spróbuje ręcznie wpisać w przeglądarce adres `/admin/system` lub `/admin/users`, `adminGuard` natychmiast przerywa renderowanie komponentu, anuluje nawigację, wywołuje alert systemowy typu Toast z treścią: *"Brak wymaganych uprawnień do wyświetlenia tej strony"* i automatycznie przekierowuje użytkownika na bezpieczną stronę główną (dashboard).

### 6.13 Tablica zadań (`QuestBoardComponent`)
* **Opis**: Centralny moduł grywalizacji prezentujący graczom dostępne misje krótko- i długoterminowe.
* **Wymagania szczegółowe**:
  * Wyświetlanie kafelków zadań z podziałem na zakładki tematyczne: *Betting* (zadania związane z obstawianiem), *Engagement* (regularność logowania), *Achievement* (kamienie milowe punktowe).
  * Każde zadanie prezentuje: nazwę, opis warunków, ikonę statusu, wartość nagrody punktowej oraz graficzny pasek postępu (np. "Postaw 3 zakłady na GP: 2/3").
* **Kryteria akceptacji**:
  * Dane są automatycznie odświeżane w tle w określonych interwałach czasowych, aby zapewnić aktualny podgląd postępu.
  * **Obsługa Edge Case (Nowy sezon / Brak zadań)**: W sytuacji wyczyszczenia bazy danych lub braku aktywnych definicji zadań w bazie, komponent ukrywa paski postępu i wyświetla komunikat: *"No active quests available"*.
  * **Obsługa Edge Case (Błąd dzielenia przez zero w konfiguracji serwera)**: Jeśli administrator popełni błąd w konfiguracji bazy danych i zdefiniuje zadanie z parametrem docelowym (target) równym `0` (np. wymagana liczba zakładów = 0), frontend musi obsłużyć tę sytuację – pasek postępu bezpiecznie przyjmuje wartość 100% lub 0% (zależnie od logiki biznesowej), uniemożliwiając wystąpienie błędu `NaN` (Not a Number) lub wygaszenie komponentu.
  * **Obsługa Edge Case (Użytkownik Niezalogowany)**: Jeśli widok zostanie wywołany przez gościa, system poprawnie renderuje nazwy zadań i opisy nagród, ale ukrywa wszystkie elementy powiązane z postępem indywidualnym (progress bar, stan licznika) i wyświetla informację: *"Zaloguj się, aby zacząć realizować zadania i zdobywać punkty"*.

---

## 7. Reguły biznesowe
Reguły biznesowe definiują logikę działania gry i algorytmy obliczeniowe platformy:
1. **Początkowy Kapitał** – Każdy nowo zarejestrowany użytkownik otrzymuje jednorazowo bezzwrotny pakiet startowy w wysokości 10 000 wirtualnych punktów.
2. **Czas Zamknięcia Zakładów** – Możliwość zawierania, edycji lub anulowania zakładów dla danego Grand Prix zostaje bezwzględnie zablokowana w bazie danych w milisekundzie oficjalnego rozpoczęcia wyścigu głównego, zależnie od konfiguracji reguł zawodów.
3. **Zwrot Punktów (Anulowanie)** – Anulowanie zakładu (zarówno przez użytkownika przed czasem blokady, jak i przez Administratora) skutkuje natychmiastowym i pełnym zwrotem 100% postawionych punktów na konto gracza.
4. **Algorytm Obliczania Wygranej** – Podstawowa wygrana punktowa wyliczana jest ze wzoru: `W = S * K`, gdzie `W` to wygrana, `S` to stawka zakładu, a `K` to kurs w momencie zatwierdzenia zakładu.
5. **Reguła Częściowych Trafień (Podium/TOP10)** – W przypadku zakładów złożonych (np. wytypuj skład TOP 3), poprawne wytypowanie 2 z 3 pozycji premiowane jest wypłatą pocieszenia stanowiącą zdefiniowany procent pełnej wygranej (np. 33%). Wytypowanie 1 z 3 pozycji nie generuje zwrotu (zakład przegrany).
6. **Reset Sezonu** – Wywołanie procedury resetu sezonu przez administratora powoduje bezwarunkowe zarchiwizowanie bieżącej tabeli liderów do tabeli historycznej, wyzerowanie aktywnych rankingów, usunięcie historii starych zakładów, wyczyszczenie postępów zadań i ustawienie salda każdego konta użytkownika z powrotem na wartość 10 000 punktów.
7. **Polityka Odświeżania Tokenów** – Token JWT zachowuje ważność przez 24 godziny. Refresh Token pozwala na automatyczne odnowienie sesji bez udziału użytkownika pod warunkiem, że konto nie zostało w międzyczasie zablokowane przez administratora.

---

## 8. Zakres projektu

### 8.1 W zakresie (In Scope)
* Projekt i implementacja responsywnej aplikacji webowej (RWD) obsługującej komputery stacjonarne, laptopy oraz urządzenia mobilne.
* Implementacja kluczowych modułów frontendowych i odpowiadających im punktów końcowych API opisanych w niniejszej specyfikacji.
* Pełna automatyzacja pobierania danych kalendarza, kierowców, zespołów i wyników za pośrednictwem cyklicznej integracji z zewnętrznym systemem OpenF1 API.
* Moduł grywalizacji: system naliczania punktów, obsługa tabeli liderów, tablica zadań (Quests), dzienne bonusy (Daily Streak).
* Bezpieczny system uwierzytelniania.
* Panel administracyjny umożliwiający zarządzanie użytkownikami (blokowanie, korekta salda), zakładami (anulowanie) oraz systemem (nadpisanie wyników, manualny sync).

### 8.2 Poza zakresem (Out of Scope)
* **Brak operacji na prawdziwych pieniądzach** – Platforma ma charakter wyłącznie rozrywkowy, nie obsługuje płatności elektronicznych, bramkami PayU/PayPal, kryptowalutami ani nie posiada licencji hazardowej.
* **Brak zakładów na żywo (In-Play Betting)** – Obstawianie jest możliwe wyłącznie przed rozpoczęciem wydarzenia. System nie przetwarza danych telemetrycznych w czasie rzeczywistym podczas trwania wyścigu w celu aktualizacji kursów na żywo.
* **Brak funkcji społecznościowych** – System nie będzie wyposażony w czat publiczny, system dodawania znajomych, wiadomości prywatne ani bezpośrednią integrację z profilami Social Media (Facebook, X).
* **Brak natywnej aplikacji mobilnej** – Projekt nie zakłada tworzenia dedykowanych aplikacji na systemy iOS (Swift) czy Android (Kotlin) – dostęp realizowany jest wyłącznie przez przeglądarkę (RWD).
* **Brak zaawansowanej analityki predykcyjnej** – System udostępnia surowe statystyki ROI i Win Rate użytkownika, ale nie zawiera modułów sztucznej inteligencji (AI/ML) sugerujących graczom, jak powinni obstawiać.
* **Obsługa tylko jednego języka** – Pierwsza wersja systemu będzie dystrybuowana wyłącznie w języku angielskim (zgodnie z danymi wejściowymi z OpenF1 API).

---

## 9. Ryzyka
W tabeli przedstawiono zidentyfikowane ryzyka projektowe wraz z odpowiadającymi im strategiami mitygacji:

| ID Ryzyka | Opis ryzyka | Wpływ | Prawdopodobieństwo | Strategia mitygacji (Zapobieganie/Obsługa) |
|-----------|-------------|-------|-------------------|--------------------------------------------|
| **R-01** | Niedostępność lub nagła zmiana struktury danych w OpenF1 API. | Krytyczny | Średnie | Wdrożenie warstwy pamięci podręcznej zawierającej ostatnie poprawne dane. Automatyczne alerty e-mail do administratorów o awarii integracji. Udostępnienie panelu manualnego nadpisywania wyników (*Override*). |
| **R-02** | Zakleszczenie aplikacji lub pętla zapytań o odświeżenie tokena przy wielu żądaniach asynchronicznych. | Wysoki | Średnie | Implementacja w komponencie `AuthInterceptor` flagi blokującej `isRefreshing` oraz kolejkowania zapytań za pomocą obiektu `Subject` z biblioteki RxJS. |
| **R-03** | Błędy matematyczne (Division by zero / NaN) w statystykach graczy bez historii zakładów. | Niski | Wysokie | Zastosowanie ścisłej walidacji frontendowej i backendowej. W przypadku braku mianownika w ułamku, kod wymusza podstawienie wartości domyślnej `0` lub tekstu zastępczego. |
| **R-04** | Próby obstawiania wyścigu, który już się rozpoczął (manipulacja czasem lokalnym klienta). | Krytyczny | Niskie | Bezwzględna walidacja czasu i statusu wyścigu po stronie serwera (Backend API) w oparciu o czas serwera NTP, niezależnie od danych przesłanych przez przeglądarkę klienta. |
| **R-05** | Podatności bezpieczeństwa (SQL Injection, XSS, nadużycia API). | Krytyczny | Średnie | Wykorzystanie ORM Entity Framework Core (automatyczna parametryzacja zapytań SQL), sanityzacja danych wejściowych w kliencie frontend przed renderowaniem HTML, wdrożenie globalnego mechanizmu Rate Limitingu na serwerze API. |

---

## 10. Wymagania niefunkcjonalne

### 10.1 Wydajność (Performance)
* **Czas ładowania strony (Page Load Time)** – Czas renderowania i pełnej gotowości interfejsu dla kluczowych widoków (Dashboard, Lista Wyścigów) nie może przekroczyć 2 sekund przy przepustowości łączą sieciowego na poziomie 3G. (Dla wersji roboczej osadzonej na darmowych serwisach hostingowych czas ten będzie wydłużony do nawet 1 minuty z powodu tzw. "spin-down" serwera).
* **Czas odpowiedzi API (API Response Time)** – 95% wszystkich zapytań HTTP kierowanych do backendu musi zostać obsłużonych w czasie poniżej 500 ms (wyłączając zapytania bezpośrednio przekierowywane asynchronicznie do zewnętrznego API).
* **Współbieżność (Concurrency)** – System w konfiguracji bazowej musi bezawaryjnie obsługiwać minimum 1000 zalogowanych użytkowników jednocześnie wykonujących operacje w bazie danych podczas weekendu wyścigowego. (Ponownie - niewykonalne na darmowym hostingu).

### 10.2 Bezpieczeństwo (Security)
* **Ochrona haseł** – Hasła użytkowników podlegają bezwzględnemu haszowaniu przed zapisem w bazie danych. Wprowadzanie otwartego tekstu do bazy jest zabronione.
* **Szyfrowanie komunikacji** – Całość ruchu sieciowego pomiędzy klientem a serwerem API musi być szyfrowana przy użyciu protokołu HTTPS (TLS 1.3). Żądania HTTP bez szyfrowania są automatycznie przekierowywane.
* **Autoryzacja** – Przechowywanie tokenów autoryzacyjnych JWT musi odbywać się w sposób uniemożliwiający ataki typu XSS i CSRF.

### 10.3 Niezawodność (Reliability)
* **Dostępność (Uptime)** – Docelowy wskaźnik dostępności platformy w skali roku wynosi 99.9% (z wyłączeniem planowanych okien serwisowych zapowiadanych z wyprzedzeniem).

### 10.4 Użyteczność (Usability) i Dostępność
* **Responsywność (RWD)** – Układ graficzny aplikacji musi płynnie dostosowywać się do rozdzielczości ekranu w przedziale od 320px (małe smartfony) do 2560px (monitory UltraWide).

### 10.5 Konserwowalność (Maintainability)
* **Architektura Kodu** – Backend musi ściśle przestrzegać zasad czystej architektury (Clean Architecture) z wyraźnym podziałem na warstwy: Domain, Application, Infrastructure, Presentation. Frontend Angular musi być zorganizowany modułowo z wydzieleniem modułów ładowanych leniwie (*Lazy Loading*).
* **Testy Automatyczne** – Pokrycie kodu testami jednostkowymi i integracyjnymi dla kluczowej logiki biznesowej (obliczanie punktów, walidacja zakładów, interceptor) musi wynosić minimum 80%.

---

## 11. Wdrożenie systemu
Aplikacja zostanie wdrożona w oparciu o architekturę kontenerową za pomocą narzędzia **Docker** i pliku konfiguracyjnego `docker-compose.yml`. W skład środowiska wchodzą:
* Kontener Frontendu – Serwer Nginx serwujący skompilowaną do czystego HTML/JS/CSS aplikację Angular.
* Kontener Backend API – Środowisko uruchomieniowe .NET 8 dla aplikacji ASP.NET Core.
* Kontener Bazy Danych – Instancja serwera bazodanowego PostgreSQL.
