# Mini E-Commerce REST API

[![CI](https://github.com/Berkilic41/ecommerce-api/actions/workflows/ci.yml/badge.svg)](https://github.com/Berkilic41/ecommerce-api/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)

## Why I Built This

E-commerce APIs are one of the most common interview topics — and one of the hardest to build correctly. I built this project as a **production-ready reference implementation** that goes beyond the typical tutorial: JWT refresh-token rotation, per-IP rate limiting, memory-cache decorators, structured logging with Serilog, Docker containerisation, and full CI/CD. Every architectural decision (raw ADO.NET over EF, decorator pattern for caching, fixed-window rate limiting) is deliberate and documented.

## 🔑 Technical Highlights

- **JWT auth** — 15-min access token + 7-day refresh token with DB-stored revocation; symmetric HMAC-SHA512 signing
- **Rate limiting** — 5 req/min per IP on auth endpoints, 30 req/min per user on writes (ASP.NET Core built-in)
- **Cache decorator** — `CachedCategoryService` and `CachedProductService` implement the Decorator pattern over `IMemoryCache` with write-through invalidation
- **Health check** — `GET /api/health` probes DB connectivity; returns `{ status, database, timestamp }` with 200/503
- **Correlation IDs** — Every request gets an `X-Correlation-ID` header injected into Serilog's `LogContext`
- **36+ tests** — Unit tests (AuthService, CartService, ProductService) + integration tests (WebApplicationFactory, health/auth/products)
- **CI/CD** — GitHub Actions: build → test → coverage → Docker smoke build
- **Containerized** — Multi-stage Dockerfile (non-root user) + docker-compose with SQL Server 2022 healthcheck

ASP.NET Core 8 Web API with SQL Server, ADO.NET (no ORM), JWT authentication, and a vanilla JS frontend. Production-ready scaffolding: tests, CI, Docker, structured logging, rate limiting, and caching.

---

## Tech stack

| Layer | Choice |
|---|---|
| Backend | ASP.NET Core 8, C# 12 |
| DB | SQL Server / LocalDB, ADO.NET raw SQL (parameterized) |
| Auth | JWT (15-min access) + Refresh Token (7-day, DB-stored) |
| Docs | Swagger / OpenAPI |
| Frontend | Vanilla JS, single HTML file |
| **Tests** | xUnit + Moq + FluentAssertions (24 tests) |
| **CI** | GitHub Actions: build + test + coverage |
| **Container** | Multi-stage Dockerfile + docker-compose with SQL Server 2022 |
| **Observability** | Serilog structured logging + request logging |
| **Security** | Rate limiting (5/min auth, 30/min writes) |
| **Performance** | `IMemoryCache` via decorator on read-heavy paths |

---

## Quick start

### Option A — Docker (everything in one command)

```bash
docker compose up -d
```

This boots SQL Server 2022, runs the schema + seed scripts, and starts the API on `http://localhost:8080`.

### Option B — Local with LocalDB

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i Database/001_Schema.sql
sqlcmd -S "(localdb)\mssqllocaldb" -i Database/002_SeedData.sql
dotnet run --project ECommerceAPI
```

### Run the test suite

```bash
dotnet test
```

24 unit tests covering: password hashing, auth flows (register/login/refresh), product CRUD edge cases, cart stock validation.

---

## API endpoints

### Auth — `/api/auth` *(rate-limited: 5 req/min/IP)*

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/register` | — | Register new user |
| POST | `/login` | — | Login, returns token pair |
| POST | `/refresh` | — | Refresh access token |
| POST | `/logout` | Bearer | Revoke refresh token |

### Categories — `/api/categories` *(reads cached 5 min)*

| Method | Path | Auth |
|--------|------|------|
| GET | `/` | — |
| GET | `/{id}` | — |
| POST/PUT/DELETE | `/{...}` | Admin |

### Products / Cart / Orders
See full endpoint reference below or run the app and visit `/swagger`.

---

## Architecture

```
┌────────────────────────────────────────────────────────────┐
│  HTTP Pipeline                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ SerilogRequestLogging                                │  │
│  │ → ExceptionHandlingMiddleware (global error JSON)    │  │
│  │ → CORS                                               │  │
│  │ → RateLimiter ([EnableRateLimiting("auth")] etc.)    │  │
│  │ → JWT Bearer Authentication                          │  │
│  │ → Authorization                                      │  │
│  │ → Controllers                                        │  │
│  └──────────────────────────────────────────────────────┘  │
│                          │                                  │
│                          ▼                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Services (business logic)                            │  │
│  │   - AuthService                                       │  │
│  │   - ProductService                                    │  │
│  │   - CategoryService ←── CachedCategoryService (decor) │  │
│  │   - CartService                                       │  │
│  │   - OrderService                                      │  │
│  └──────────────────────────────────────────────────────┘  │
│                          │                                  │
│                          ▼                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Repositories (ADO.NET, parameterized SQL)            │  │
│  │ DbConnectionFactory (singleton, reads connstring)    │  │
│  └──────────────────────────────────────────────────────┘  │
│                          │                                  │
│                          ▼                                  │
│                    SQL Server                               │
└────────────────────────────────────────────────────────────┘
```

### Folder layout

```
ECommerceAPI/
├── .github/workflows/ci.yml         ← build + test + coverage on every PR/push
├── Database/
│   ├── 001_Schema.sql               ← tables + indexes
│   └── 002_SeedData.sql             ← 5 categories + 10 products
├── ECommerceAPI/                    ← API project
│   ├── Controllers/
│   ├── Services/
│   │   └── Decorators/              ← CachedCategoryService
│   ├── Repositories/
│   ├── Middleware/
│   ├── DTOs/
│   ├── Models/
│   ├── Data/                        ← DbConnectionFactory
│   ├── Program.cs                   ← composition root
│   └── appsettings.json
├── tests/ECommerceAPI.Tests/        ← xUnit + Moq + FluentAssertions
│   └── Services/
├── Dockerfile                       ← multi-stage, runs as non-root
├── docker-compose.yml               ← API + SQL Server + auto schema/seed
├── .editorconfig                    ← code style enforcement
├── .gitignore
├── CONTRIBUTING.md                  ← branch naming, commit style, PR flow
└── LICENSE                          ← MIT
```

---

## Senior practices applied

| Concern | What's in the repo |
|---|---|
| **Tests** | xUnit project with 24 unit tests, mocks for external dependencies, FluentAssertions for readable assertions |
| **CI/CD** | GitHub Actions: cached NuGet, `/warnaserror`, coverage upload, Docker smoke build on `main` |
| **Containerization** | Multi-stage Dockerfile (~210MB final), non-root user, healthcheck, layer-cached restore |
| **Compose** | Idempotent SQL bootstrap via separate init container (`db-init` waits for `db.healthy`) |
| **Logging** | Serilog with structured properties, request logging middleware, bootstrap logger captures host startup failures |
| **Rate limiting** | `.NET 8` built-in `RateLimiter` with separate policies for auth (per-IP) and writes (per-user) |
| **Caching** | Decorator pattern over `ICategoryService` — cache logic isolated, controllers untouched, easy to swap to Redis later |
| **Code style** | `.editorconfig` enforced, `/warnaserror` in CI |
| **Conventional commits** | `feat:`, `fix:`, `chore:`, `docs:` — see `git log --oneline` |
| **Feature branches** | Each concern landed via its own branch with `--no-ff` merge |

---

## Roadmap

These are intentional next-steps documented for an interview discussion:

- [ ] Replace cookie-style refresh tokens with HTTP-only cookie + sliding window
- [ ] Add `/health` endpoint with DB connectivity probe
- [ ] Migrate from filtered `IMemoryCache` to Redis when horizontally scaling
- [ ] Add OpenTelemetry traces / metrics export
- [ ] Add integration tests with `WebApplicationFactory` + Testcontainers
- [ ] Migrate the vanilla JS frontend to React + TypeScript
- [ ] Add OAuth (Google / Microsoft) provider via OIDC

---

## License

MIT — see [LICENSE](LICENSE).
