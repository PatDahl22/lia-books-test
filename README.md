# LIA Books Test

[![CI](https://github.com/PatDahl22/lia-books-test/actions/workflows/ci.yml/badge.svg)](https://github.com/PatDahl22/lia-books-test/actions/workflows/ci.yml)

En responsiv fullstack-applikation för böcker och favoritcitat, byggd som ett LIA-kodtest med Angular 20 och ett .NET 9 REST API.

## Funktioner

- Registrering och inloggning med JWT
- Hashade lösenord med ASP.NET Core `PasswordHasher`
- Skyddade API-anrop med Bearer-token
- CRUD för böcker
- Personlig CRUD-sida för citat
- Fem startcitat för varje ny användare
- Responsiv Bootstrap-layout med Angular-styrd mobilmeny
- Font Awesome-ikoner i navigering, formulär och åtgärder
- Ljust och mörkt tema med sparat användarval
- SQLite lokalt och PostgreSQL vid Render-publicering
- OpenAPI-dokument i utvecklingsmiljö
- Integrationstester och GitHub Actions

## Kravöversikt

| Krav | Lösning |
| --- | --- |
| Lista alla böcker | `/books` hämtar och visar böcker som responsiva kort |
| Lägg till bok | `/books/new` med validerat formulär och omdirigering |
| Redigera bok | `/books/:id/edit` laddar och uppdaterar vald bok |
| Radera bok | Bekräftad radering direkt från boklistan |
| Registrering och inloggning | `/register` och `/login` mot JWT-API |
| Tokenhantering | Token sparas i `localStorage` och bifogas via HTTP-interceptor |
| Skyddade CRUD-anrop | `[Authorize]` på böcker och citat i API:t |
| Mina citat | Egen vy, användarspecifik data och full CRUD |
| Fem citat | Skapas automatiskt när en användare registreras |
| Responsiv meny | Menyn kollapsar under Bootstrap `lg`-brytpunkten |
| Bootstrap och Font Awesome | Globala CSS-resurser och semantiska komponentklasser |
| Ljust/mörkt tema | Temaknapp i navigeringen, sparad i `localStorage` |

## Teknik

- Angular 20.3, TypeScript, RxJS och Reactive Forms
- Bootstrap 5.3 och Font Awesome 7
- .NET 9, ASP.NET Core Web API och Entity Framework Core
- JWT Bearer Authentication
- SQLite för lokal utveckling
- PostgreSQL för kostnadsfri Render-demo
- Docker multi-stage build

## Projektstruktur

```text
backend/
  BookQuote.Api/          API, autentisering, EF Core och migrations
  BookQuote.Api.Tests/    Integrationstester
frontend/
  src/app/core/           Guards, interceptor, modeller och tjänster
  src/app/features/       Auth-, bok- och citatvyer
.github/workflows/ci.yml  Automatisk build och test
Dockerfile                Produktionsbuild för hela applikationen
render.yaml               Render Blueprint för webbapp och PostgreSQL
```

## Köra lokalt

Förutsättningar:

- .NET SDK 9.0.317
- Node.js 20.20.2
- npm 10 eller senare

Starta API:t:

```bash
cd backend/BookQuote.Api
dotnet restore
dotnet run
```

API:t körs på `http://localhost:5272`. SQLite-migrationerna appliceras automatiskt.

Starta Angular i en andra terminal:

```bash
cd frontend
npm ci
npm start
```

Öppna `http://localhost:4200`. Angulars proxy skickar `/api` till .NET-API:t.

Om en äldre lokal `books.db` skapades innan autentiseringen infördes, radera den en gång och starta API:t igen så byggs testdatabasen om.

## Testa

Backend:

```bash
dotnet test backend/BookQuote.Api.Tests/BookQuote.Api.Tests.csproj
```

Frontend:

```bash
cd frontend
npm test -- --watch=false --browsers=ChromeHeadless
npm run build
```

`BookQuote.Api.http` innehåller även manuella anrop för registrering, token, böcker och citat.

## API

Alla routes för böcker och citat kräver `Authorization: Bearer <token>`.

| Method | Route | Beskrivning |
| --- | --- | --- |
| `POST` | `/api/auth/register` | Registrera användare och returnera JWT |
| `POST` | `/api/auth/login` | Logga in och returnera JWT |
| `GET` | `/api/auth/me` | Hämta aktuell användare |
| `GET` | `/api/books` | Lista böcker |
| `GET` | `/api/books/{id}` | Hämta bok |
| `POST` | `/api/books` | Skapa bok |
| `PUT` | `/api/books/{id}` | Uppdatera bok |
| `DELETE` | `/api/books/{id}` | Radera bok |
| `GET` | `/api/quotes` | Lista användarens citat |
| `GET` | `/api/quotes/{id}` | Hämta användarens citat |
| `POST` | `/api/quotes` | Skapa citat |
| `PUT` | `/api/quotes/{id}` | Uppdatera citat |
| `DELETE` | `/api/quotes/{id}` | Radera citat |
| `GET` | `/health` | Publik hälsokontroll |

## Säkerhet

- Lösenord lagras aldrig i klartext.
- Användarnamn normaliseras och har ett unikt databasindex.
- JWT validerar signatur, issuer, audience och livslängd.
- Citat filtreras med användar-ID från validerad token.
- Inloggningsfel avslöjar inte om ett visst användarnamn finns.
- Produktionens JWT-nyckel genereras av Render och ligger inte i repositoryt.

För kodtestet lagras sessionen i `localStorage`, vilket uttryckligen tillåts i uppgiften. I en större produktionslösning bör refresh-token och HttpOnly-cookie övervägas.

## Publicera på Render

`render.yaml` skapar både Docker-webbappen och en kostnadsfri PostgreSQL-databas i Frankfurt.

[![Deploy to Render](https://render.com/images/deploy-to-render-button.svg)](https://render.com/deploy?repo=https://github.com/PatDahl22/lia-books-test)

Render använder port `10000`, kör hälsokontroll på `/health` och publicerar först efter godkända CI-kontroller. Den kostnadsfria PostgreSQL-databasen är avsedd för demo/test och upphör enligt Renders nuvarande villkor efter 30 dagar.
