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
1. Gracz loguje się do systemu i przechodzi do modułu kalendarza wyścigów (Races).
2. Gracz wybiera nadchodzący wyścig o statusie *Scheduled*.
3. System wyświetla komponent zawierający formularz z dostępnymi typami zakładów oraz aktualnymi kursami.
4. Gracz uzupełnia swoje typy i wprowadza stawkę punktową (w ramach dostępnego salda konta).
5. Gracz klika przycisk "Place Bet".
6. System przeprowadza walidację (czy wyścig się nie rozpoczął, czy stawka <= saldo).
7. System zapisuje zakład w bazie ze statusem *Pending*, potrąca punkty z salda gracza i wyświetla komunikat o sukcesie.

### Scenariusz 2: Automatyczne rozliczenie wyścigu po jego zakończeniu (System)
1. System w tle co 5 minut sprawdza stan rzeczywistego wyścigu w OpenF1 API.
2. Wykryte zostaje zakończenie wyścigu – system zmienia wewnętrzny status wyścigu na *Finished*.
3. Automatycznie po wykryciu zakończenia wyścigu system pobiera oficjalne wyniki.
4. System wyszukuje w bazie danych wszystkie zakłady o statusie *Pending* powiązane z tym wyścigiem.
5. Dla każdego zakładu system porównuje typowania użytkownika z oficjalnymi wynikami:
   - W przypadku trafienia zmienia status na *Won* i aktualizuje saldo użytkownika o oblicza wygraną: `stawka * kurs`.
   - W przypadku braku trafienia zmienia status na *Lost*.
6. Zmiana statusu wyścigu na *ResultsProcessed* kończy proces. Użytkownicy mogą sprawdzić statusy swoich zakładów w swoim profilu.

### Scenariusz 3: Interwencja Administratora w przypadku błędu kursu (Administrator)
1. Administrator loguje się i wchodzi do panelu administracyjnego systemu a następnie do zakładki "Bets".
3. Za pomocą filtrów odnajduje nierozliczone zakłady (*Pending*) dla wybranego wyścigu, w których doszło do awarii algorytmu generowania kursów (np. kurs wyniósł 500.0 zamiast 5.0).
4. Administrator klika opcję "Anuluj zakład" przy błędnych pozycjach.
5. System wyświetla okno modalne z żądaniem potwierdzenia.
6. Po zatwierdzeniu system usuwa zakład.

## 6. Wymagania funkcjonalne
Sekcja zawiera szczegółowy opis wymagań funkcjonalnych systemu z podziałem na 13 modułów i komponentów technicznych zdefiniowanych w architekturze aplikacji:

### 6.1 Przeglądanie listy wyścigów
* **Opis**: Użytkownik (zarówno zalogowany, jak i gość) musi mieć możliwość przeglądania pełnego kalendarza wyścigów Formuły 1 zaimportowanego z OpenF1 API.
* **Wymagania szczegółowe**:
  * Prezentacja wyścigów w formie czytelnej listy lub kafelków, zawierających: nazwę Grand Prix, nazwę toru (circuit), datę odbycia oraz aktualny status (*Scheduled*/*Finished*/*ResultsProcessed*).
  * Dla wyścigów o statusie *Scheduled* powinien znajdować się przycisk przekierowujący do formularza utworzenia zakładu dla danego wyścigu. Dla wyścigów o innych statusach przycisk ten ma być ukryty.
  * Udostępnienie paska filtrowania z zakładkami: "Wszystkie", "Nadchodzące" (status *Scheduled*), "Zakończone" (status *Finished* lub *ResultsProcessed*).
  * Implementacja paginacji danych w celu optymalizacji renderowania przy dużej liczbie rekordów w sezonie.

### 6.2 Wyświetlanie szczegółów wyścigu
* **Opis**: Wyświetlenie dodatkowych informacji o konkretnym weekendzie wyścigowym wybranym przez użytkownika.
* **Wymagania szczegółowe**:
  * Widok musi prezentować: pełną nazwę wydarzenia, status, nazwę toru, sezon oraz datę odbycia wyścigu.
  * Dla wyścigów nadchodzących osadza moduł zawierania przycisk przekierowujący do formularza utworzenia zakładu. Dla wyścigów zakończonych (*Finished*/*ResultsProcessed*) zamiast tego przycisku jest wyświetlana prezentacja oficjalnych wyników.

### 6.3 Obstawianie zakładów
* **Opis**: Interfejs formularza umożliwiający graczom alokację posiadanych wirtualnych punktów na konkretne zdarzenia w nadchodzącym wyścigu.
* **Wymagania szczegółowe**:
  * Formularz udostępnia pola wyboru dla kierowców i zespołów pobieranych z bazy.
  * Rodzaje typowań: wygrany, miejsce w TOP 3, miejsce w TOP 10, najszybsze okrążenie (Fastest Lap).
  * Prezentowanie aktualnego wskaźnika kursu oraz wyliczenie potencjalnej wygranej w punktach na podstawie wprowadzonej stawki przed zatwierdzeniem.

### 6.4 Zarządzanie profilem użytkownika
* **Opis**: Centralny kokpit (dashboard) użytkownika prezentujący jego status w grze, tożsamość oraz postępy w grywalizacji.
* **Wymagania szczegółowe**:
  * Prezentacja: Nazwy użytkownika, adresu e-mail, aktualnego całkowitego salda punktów wirtualnych, daty utworzenia konta.
  * Wyświetlanie sekcji z podsumowaniem aktywnych zadań (Quests), z informacją o statusie dziennego bonusu za logowanie (Daily Streak Info), historii zmian salda użytkownika oraz historii zakładów.
  * Przekierowanie do widoków: zaawansowanej historii zakładów (z filtrami), statystyk użytkownika, danych analitycznych.

### 6.5 Historia zakładów użytkownika
* **Opis**: Chronologiczna ewidencja wszystkich zakładów zawartych przez danego użytkownika od momentu utworzenia konta.
* **Wymagania szczegółowe**:
  * Widok wyświetla: Listę zakładów użytkownika oraz formularz do ich filtrowania.
  * Lista obsługuje pełne stronicowanie po stronie serwera w celu minimalizacji zużycia pamięci.
  * Gracz ma do dyspozycji filtry statusu: "Wszystkie", "Wygrane" (*Won*), "Przegrane" (*Lost*), "Oczekujące" (*Pending*), "Anulowane" (*Cancelled*).
  * Możliwość eksportu historii do pliku CSV.

### 6.6 Statystyki i Analityka
* **Opis**: Moduł dostarczający użytkownikowi matematycznej i statystycznej analizy jego zachowań oraz skuteczności w typowaniu wyników.
* **Wymagania szczegółowe**:
  * Kalkulacja i prezentacja wskaźników: Procent wygranych (Win Rate), całkowity wskaźnik zwrotu (ROI).
  * Wizualizacja rozkładu zysków i strat w podziale na kierowców, rodzaj zakładu, czas dnia oraz miesiąc za pomocą tabel.

### 6.7 Uwierzytelnianie użytkownika
* **Opis**: Moduł odpowiedzialny za kontrolę dostępu do systemu, rejestrację nowych kont i bezpieczne logowanie.
* **Wymagania szczegółowe**:
  * Formularz rejestracji wymaga podania unikalnej nazwy użytkownika, poprawnego adresu e-mail oraz silnego hasła spełniającego kryteria bezpieczeństwa (przynajmniej wymóg min. 8 znaków).
  * Formularz logowania uwierzytelnia użytkownika na podstawie e-maila i hasła.
  * Walidacja pól (np. poprawność formatu e-mail regex, zgodność haseł) odbywa się w czasie rzeczywistym przed wciśnięciem przycisku "Log In"/"Register".
  * Obsługa Edge Case (Błędne poświadczenia): Wprowadzenie złego hasła lub e-maila skutkuje wyświetleniem komunikatu błędu o nieprawidłowych danych logowania. Pola formularza nie są czyszczone, umożliwiając szybką poprawkę.
  * Obsługa Edge Case (Zajęty adres e-mail): Próba rejestracji na e-mail istniejący w bazie zwraca komunikat o tym, że e-mail jest już zajęty.

### 6.8 Zarządzanie użytkownikami
* **Opis**: Panel administracyjny dedykowany do kontroli kont użytkowników i moderacji społeczności gry.
* **Wymagania szczegółowe**:
  * Wyświetlanie pełnej listy zarejestrowanych użytkowników z wyszukiwarką po nazwie i adresie e-mail oraz filtrami statusu konta (Aktywne lub Zawieszone).
  * Udostępnienie akcji moderacyjnych: korekta salda punktów użytkownika, zawieszenie konta użytkownika.
  * Obsługa Edge Case (Autoblokada): System posiada twarde zabezpieczenie uniemożliwiające zalogowanemu administratorowi wykonanie akcji zablokowania, zawieszenia lub usunięcia uprawnień administratora wobec samego siebie lub innych kont z rolą `SuperAdmin`. Przycisk akcji dla tych pozycji jest ukryty lub nieaktywny.

### 6.9 Zarządzanie systemem
* **Opis**: Konsola operacyjna pozwalająca na ręczną kontrolę stanów integracji oraz danych sportowych.
* **Wymagania szczegółowe**:
  * Udostępnienie przycisku do manualnego wyzwolenia zadania synchronizacji systemu z API OpenF1.
  * Interfejs do wprowadzania ręcznych korekt lub wpisania oficjalnych wyników wyścigu w przypadku awarii API OpenF1 (formularz Override).

### 6.10 Zarządzanie zakładami
* **Opis**: Narzędzie nadzorcze pozwalające na monitorowanie globalnego wolumenu zakładów i eliminowanie błędów kursowych.
* **Wymagania szczegółowe**:
  * Widok prezentujący zestawienie wszystkich zakładów zawartych w systemie z możliwością filtrowania po nazwie użytkownika oraz statusie zakładu.
  * Funkcja bezpiecznego anulowania kuponu ze statusem *Pending* przed rozliczeniem wyścigu.
  * Anulowanie zakładu skutkuje bezwarunkowym zwrotem 100% postawionych punktów wirtualnych na konto gracza.
  * Obsługa Edge Case (Blokada anulowania rozliczonego zakładu): Jeśli zakład posiada już status *Won* lub *Lost* (został formalnie rozliczony), przycisk "Anuluj" obok tego rekordu zostaje permanentnie zablokowany. Próba przesłania żądania anulowania rozliczonego zakładu bezpośrednio na endpoint API zwraca błąd krytyczny biznesowy, informując, że rozliczonych transakcji nie można cofnąć bez procedury korekty salda w module użytkowników.
 
### 6.11 Zarządzenie zadaniami (Quests)
  * **Opis**: Narzędzie nadzorcze służące do monitorowania, tworzenia, edycji i usuwania zadań (Quests).
  * **Wymagania szczegółowe**:
    * Widok prezentujący zestawienie wszystkich istniejących zadań.
    * Możliwość edycji, usunięcia, wyłączenia oraz sprawdzenia postępów każdego z zadań.
    * Filtrowanie listy zadań według statusu oraz nazwy zadania.
    * Możliwość utworzenia nowego zadania.
    * Możliwość zresetowania zadań tygodniowych.

### 6.12 Bezpieczeństwo panelu
* **Opis**: Programistyczna ochrona zasobów administracyjnych przed nieuprawnionym dostępem.
* **Wymagania szczegółowe**:
  * Dostęp do zasobów panelu jest przyznawany wyłącznie użytkownikom posiadającym rolę administracyjną.
  * Obsługa Edge Case (Próba nieautoryzowanego dostępu): Jeżeli zwykły gracz lub użytkownik niezalogowany spróbuje ręcznie wpisać w przeglądarce adres `/admin/system` lub `/admin/users`, to system natychmiast przerywa renderowanie komponentu, anuluje nawigację i automatycznie przekierowuje użytkownika na stronę logowania.

### 6.13 Tablica zadań
* **Opis**: Centralny moduł grywalizacji prezentujący graczom dostępne misje krótko- i długoterminowe.
* **Wymagania szczegółowe**:
  * Wyświetlanie kafelków zadań z podziałem na zakładki tematyczne: *Betting* (zadania związane z obstawianiem), *Engagement* (regularność logowania), *Achievement* (kamienie milowe punktowe).
  * Każde zadanie prezentuje: nazwę, opis warunków, ikonę statusu, wartość nagrody punktowej oraz graficzny pasek postępu (np. "Postaw 3 zakłady na GP: 2/3").
  * Obsługa Edge Case (Użytkownik Niezalogowany): Jeśli widok zostanie wywołany przez gościa, system poprawnie renderuje nazwy zadań i opisy nagród, ale ukrywa wszystkie elementy powiązane z postępem indywidualnym (progress bar, stan licznika).

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
* Projekt i implementacja aplikacji webowej obsługującej komputery stacjonarne, laptopy oraz opcjonalnie urządzenia mobilne.
* Pełna automatyzacja pobierania danych kalendarza, kierowców, zespołów i wyników za pośrednictwem cyklicznej integracji z zewnętrznym systemem OpenF1 API.
* Moduł grywalizacji: system naliczania punktów, tablica zadań (Quests), dzienne bonusy (Daily Streak).
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
* **Czas ładowania strony (Page Load Time)** – Czas renderowania i pełnej gotowości interfejsu dla kluczowych widoków (Dashboard, Lista Wyścigów) nie może przekroczyć 2 sekund przy przepustowości łączą sieciowego na poziomie 3G. (Nie dotyczy wersji roboczej osadzonej na darmowych serwisach hostingowych: czas ten będzie wydłużony do nawet 1 minuty z powodu tzw. "spin-down" serwera).
* **Czas odpowiedzi API (API Response Time)** – 95% wszystkich zapytań HTTP kierowanych do backendu musi zostać obsłużonych w czasie poniżej 500 ms (wyłączając zapytania bezpośrednio przekierowywane asynchronicznie do zewnętrznego API).
* **Współbieżność (Concurrency)** – System w konfiguracji bazowej musi bezawaryjnie obsługiwać minimum 1000 zalogowanych użytkowników jednocześnie wykonujących operacje w bazie danych podczas weekendu wyścigowego. (Nie dotyczy wersji roboczej przez ograniczenia darmowej wersji hostingu).

### 10.2 Bezpieczeństwo (Security)
* **Ochrona haseł** – Hasła użytkowników podlegają bezwzględnemu haszowaniu przed zapisem w bazie danych. Wprowadzanie otwartego tekstu do bazy jest zabronione.
* **Szyfrowanie komunikacji** – Całość ruchu sieciowego pomiędzy klientem a serwerem API musi być szyfrowana przy użyciu protokołu HTTPS (TLS 1.3). Żądania HTTP bez szyfrowania są automatycznie przekierowywane.

### 10.3 Niezawodność (Reliability)
* **Dostępność (Uptime)** – Docelowy wskaźnik dostępności platformy w skali roku wynosi 99.9% (z wyłączeniem planowanych okien serwisowych zapowiadanych z wyprzedzeniem).
