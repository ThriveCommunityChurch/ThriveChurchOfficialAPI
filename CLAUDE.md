# ThriveChurchOfficialAPI — CLAUDE.md

Primary backend for Thrive sermon platform. Manages sermon series datastore, provides public API for sermon distribution, handles authentication and API versioning.

## Tech Stack

- **Framework:** C# / .NET (6.0+)
- **Database:** MongoDB (mongo-thrive-production in prod; mongo-localhost or local instance in dev)
- **API Versioning:** URL-based (/v1/, /v2/, etc.) — ensures backward compatibility
- **Testing:** MSTest, Moq, integration tests against MongoDB
- **Deployment:** GitHub Actions → Docker → AWS (ECS/Lambda)

## Project Structure

```
ThriveChurchOfficialAPI/
├── Controllers/          # HTTP endpoints (thin; delegate to services)
├── Services/             # Business logic (interfaces in Abstract/, implementations in Services/)
├── Repositories/         # Data access (inherit from RepositoryBase<T>)
├── Core/                 # Domain models, DTOs, enums
├── Tests/                # Unit + integration tests (must achieve 100% branch coverage)
├── Startup.cs            # DI container, middleware
└── .worktrees/           # Feature branches (local, gitignored)
```

## Local Development

### 1. MongoDB Setup

```bash
# Start MongoDB locally
mongod --dbpath=C:\data\mongodb

# Connection string (appsettings.json in Dev):
# mongodb://localhost:27017/SermonSeries
```

### 2. Run the API

```bash
dotnet restore
dotnet run --configuration Debug

# Listens on: https://localhost:5001
# Swagger available at: https://localhost:5001/swagger
```

### 3. Run Tests

```bash
dotnet test --configuration Debug

# Enforce 100% branch coverage on Services/ and Core/
# Integration tests connect to local MongoDB (auto-seeded from fixtures)
```

## Code Patterns

**N-tier architecture (strict):** Controllers → Services → Repositories → Core

**Service pattern:**
- Accept 3–7 repository/service dependencies via constructor injection
- Validate request → call repos/services → compose result → return `SystemResponse<T>`
- Example: `SermonService.GetSeriesByIdAsync(id)` validates, queries repo, formats DTO

**Repository pattern:**
- Inherit from `RepositoryBase<T>`
- Implement specific queries (beyond CRUD)
- Implement `IRepository<T>` interface
- Example: `SermonSeriesRepository.GetAllWithEpisodesAsync()`

**DTOs:**
- Separate request/response models
- Map from domain → DTO in Service layer
- Use AutoMapper or manual mapping

## Deployment

- **Branch:** `master` → automatic build & deploy to production
- **CI/CD:** GitHub Actions → Docker build → AWS ECS
- **Trigger:** Immediate on merge to master (no approval gates)
- **Rollback:** Revert commit, re-push to master (redeploy triggered automatically)
- **Monitoring:** CloudWatch logs, X-Ray for request tracing

## Shared Standards & References

For patterns shared across all projects, see the Docs repository:

- **Shared patterns** — `Docs/Shared/`
  - `N-Tier-Architecture.md` — Service/Repository/DbContext pattern (applied here)
  - `Testing-Patterns.md` — 100% coverage requirement, test structure
  - `Git-Workflow.md` — Worktree discipline, commit rules, PR process
  - `Naming-Conventions.md` — C# naming conventions
  - `Development-Workflow.md` — Feature implementation order

- **Thrive-specific** — `Docs/Thrive/`
  - `Architecture-Overview.md` — How all Thrive repos fit together
  - `Sermon-API.md` — Collections, endpoints, versioning strategy

## Key Principles

- **Single responsibility:** Controllers thin, Services rich, Repositories data-focused
- **Testability:** All business logic in Services; integration tests hit real MongoDB
- **Backward compatibility:** Versioning strategy protects legacy clients
- **TypeScript/Angular consumers:** Sermon data consumed by mobile app (`ThriveChurchOfficialApp_CrossPlatform`) and web clients
