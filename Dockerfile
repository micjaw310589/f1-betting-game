# Etap budowania (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Skopiuj pliki projektów (.csproj) i przywróć zależności (Nugety)
COPY ["F1BettingApp/F1BettingApp.API/F1BettingApp.API.csproj", "F1BettingApp/F1BettingApp.API/"]
COPY ["F1BettingApp/F1BettingApp.Application/F1BettingApp.Application.csproj", "F1BettingApp/F1BettingApp.Application/"]
COPY ["F1BettingApp/F1BettingApp.Domain/F1BettingApp.Domain.csproj", "F1BettingApp/F1BettingApp.Domain/"]
COPY ["F1BettingApp/F1BettingApp.Infrastructure/F1BettingApp.Infrastructure.csproj", "F1BettingApp/F1BettingApp.Infrastructure/"]
RUN dotnet restore "F1BettingApp/F1BettingApp.API/F1BettingApp.API.csproj"

# Skopiuj całą resztę kodu
COPY . .
WORKDIR "/src/F1BettingApp/F1BettingApp.API"

# Zbuduj aplikację w trybie Release
RUN dotnet build "F1BettingApp.API.csproj" -c Release -o /app/build
RUN dotnet publish "F1BettingApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etap uruchamiania (tylko środowisko uruchomieniowe - mniejszy obraz)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Ustawienie domyślnego portu dla Render.com
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "F1BettingApp.API.dll"]