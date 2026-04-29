# f1-betting-game

`https://f1-betting-game-qy5l.vercel.app/`

## Opis skrócony projektu
Aplikacja internetowa/bazodanowa pełniąca rolę gry w obstawianie wyścigów F1 na punkty.

Frontend: Angular;
Backend: ASP.NET Core (C#)
Repozytorium: Microsoft SQL Server + OpenF1 API

### Wysokopoziomowy opis aplikacji
Zarejestrowany w systemie użytkownik otrzymuje dostęp do obstawiania wyścigów F1.\
Każdy użytkownik posiada swoje saldo - punkty, którymi obstawia wyścigi.\
Dzięki połączeniu aplikacji z publicznym API OpenF1 aplikacja uzyskuje informacje o nadchodzących, trwających oraz zakończonych wyścigach, a także o stanie mistrzostw świata itp.\

Użytkownik może obstawiać zawody w różnych kategoriach - najważniejsze z nich:
- TOP 3 kierowców w wyścigu;
- Kierowca z najszybszym czasem okrążenia wyścigu;
- Zespół z najszybszym pit stopem;
- Liczba kierowców, którzy nie dojechali do mety (DNF);
i inne (jest to pole do rozwoju aplikacji po wdrożeniu).

Ponadto system przechowuje historię zakładów i udostępnia użytkownikom ich personalne statystyki związane z grą, jak i również prezentuje rankingi graczy (ogólne i okresowe, np. z 1 miesiąca).
