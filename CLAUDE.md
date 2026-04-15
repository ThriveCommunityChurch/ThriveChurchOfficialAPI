# ThriveChurchOfficialAPI

C#/.NET 8 backend. MongoDB (SermonSeries DB). Single source of truth for all sermon/config data.

## Commands

```bash
dotnet restore
dotnet run --configuration Debug      # → https://localhost:5001/swagger
dotnet test --configuration Debug     # 100% branch on Services/ + Core/
```

MongoDB: `mongod --dbpath=C:\data\mongodb` (local dev, SermonSeries DB)

## Rules

- N-tier strict: Controllers → Services → Repositories → Core
- No AutoMapper — manual mapping only
- Return `SystemResponse<T>` from services
- Repositories inherit `RepositoryBase<T>`, implement `IRepository<T>`
- Soft deletes: `IsActive = false`, never physical delete

## Docs
- `Docs/Thrive/Sermon-API.md` — collections, endpoints, versioning
- `Docs/Shared/N-Tier-Architecture.md` — service/repository patterns
