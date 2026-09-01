# LIA Books Test

This is a full-stack coding test project for a LIA internship application.

## Tech Stack

- Angular 20 frontend
- .NET 9 C# REST API backend
- JWT authentication (planned)
- CRUD for books and quotes
- Bootstrap
- Font Awesome
- Netlify deployment for frontend (planned)

## Development Environment

This project targets .NET 9 because the coding test requires a .NET 9 C# REST API.

Installed locally during development:

- .NET SDK 9.0.317
- Node.js 20.20.2
- npm 10.8.2
- Git 2.51.0

.NET 10 is also installed on the machine, but this project is locked to .NET 9 through `global.json`.

Older .NET SDKs such as 3.1 and 5.0 are not required for this project.

## Project Structure

```text
backend/
  BookQuote.Api/

frontend/

global.json
README.md
```

## Implemented

- Angular application shell with Bootstrap and Font Awesome
- EF Core models and SQLite migration for books and quotes
- Validated REST CRUD endpoints for books and nested quotes
- Development CORS for the Angular application at `http://localhost:4200`
- OpenAPI document in development

## API Routes

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/books` | List books with quote counts |
| `GET` | `/api/books/{id}` | Get one book and its quotes |
| `POST` | `/api/books` | Create a book |
| `PUT` | `/api/books/{id}` | Update a book |
| `DELETE` | `/api/books/{id}` | Delete a book and its quotes |
| `GET` | `/api/books/{bookId}/quotes` | List quotes for a book |
| `GET` | `/api/books/{bookId}/quotes/{id}` | Get one quote |
| `POST` | `/api/books/{bookId}/quotes` | Add a quote to a book |
| `PUT` | `/api/books/{bookId}/quotes/{id}` | Update a quote |
| `DELETE` | `/api/books/{bookId}/quotes/{id}` | Delete a quote |

## Run Locally

Start the backend:

```bash
cd backend/BookQuote.Api
dotnet run
```

The SQLite migration is applied automatically when the API starts. Requests for all
endpoints are available in `backend/BookQuote.Api/BookQuote.Api.http`.

Start the frontend in a second terminal:

```bash
cd frontend
npm ci
npm start
```

## Next Milestones

- Connect the Angular UI to the books and quotes API
- Add JWT authentication and protect write operations
- Add backend integration tests and frontend interaction tests
- Configure production API hosting and Netlify environment settings
