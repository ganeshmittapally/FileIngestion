# FileIngestion

FileIngestion is a .NET 8 microservice scaffold that demonstrates a hexagonal (ports & adapters) architecture for file uploads and metadata management.

Summary
- API: ASP.NET Core minimal API with an API-key middleware for simple auth.
- Architecture: Hexagonal — ports (interfaces) in Application/Shared, adapters in Infrastructure.
- Storage: Azure Blob Storage (file contents) + Azure Cosmos DB (metadata) in production; an in-memory metadata adapter is used by default for local dev.
- Observability: OpenTelemetry (console exporter for local development and OTLP/Datadog placeholders for production).

Project layout
- `src/FileIngestion.Api` — API and startup wiring
- `src/FileIngestion.Application` — service interfaces and use-case orchestration (`IFileService`, `FileService`)
- `src/FileIngestion.Domain` — domain models and rich business logic (empty scaffold)
- `src/FileIngestion.Infrastructure` — adapters (Blob, Cosmos or in-memory fallback)
- `src/FileIngestion.Shared` — DTOs and shared contracts

OpenAPI & Postman
- The OpenAPI spec is available at `specification/openapi.yaml` in the repo.
- A Postman collection is available at `specification/postman_collection.json` (import it to test quickly).

API endpoints
The service exposes the following endpoints (API-key header: `X-API-KEY: <your-key>`):

- POST /files — Uploads a file (multipart/form-data)
	- Request: multipart form with `file` field
	- Response: { url: string, metadataId?: string }

- GET /files — Lists metadata (paging query params: `page`, `pageSize`)
	- Response: [ FileMetadata ]

- GET /files/{id} — Gets metadata by id
	- Response: FileMetadata

- DELETE /files/{id} — Deletes metadata and associated file (best-effort)
	- Response: 204 No Content

Try it (examples)
Replace `<API_URL>` with the running API address (default: `http://localhost:5000`), and `<API_KEY>` with the configured API key.

1) Upload (curl)

```powershell
curl -X POST "http://localhost:5000/files" -H "X-API-KEY: <API_KEY>" -F "file=@C:\path\to\yourfile.txt" -v
```

PowerShell (Invoke-RestMethod) equivalent:

```powershell
$file = Get-Item "C:\path\to\yourfile.txt"
$form = @{ file = Get-Item $file.FullName }
Invoke-RestMethod -Uri "http://localhost:5000/files" -Method Post -Headers @{ 'X-API-KEY' = '<API_KEY>' } -Form $form
```

2) List metadata

```powershell
curl "http://localhost:5000/files?page=1&pageSize=20" -H "X-API-KEY: <API_KEY>"
```

3) Get metadata by id

```powershell
curl "http://localhost:5000/files/<METADATA_ID>" -H "X-API-KEY: <API_KEY>"
```

4) Delete metadata (and associated file)

```powershell
curl -X DELETE "http://localhost:5000/files/<METADATA_ID>" -H "X-API-KEY: <API_KEY>"
```

Notes about metadata adapter
- Default (local): an in-memory metadata adapter is used so the project builds and runs without Azure dependencies.
- Production (Cosmos): a Cosmos-backed adapter implementation exists but is compiled only when you opt in by building with the MSBuild property `UseCosmos=true`.

Enable the Cosmos adapter (build-time)
1. Pick a compatible `Azure.Cosmos` package version available to your environment and set it as `CosmosPackageVersion`.
2. Build with:

```powershell
dotnet build -p:UseCosmos=true -p:CosmosPackageVersion=4.0.0-preview3
```

If the package version you choose isn't available on your NuGet feed, update `CosmosPackageVersion` to a supported version or use the default in-memory adapter.

Running locally
1) Build

```powershell
dotnet build "src\FileIngestion.Api\FileIngestion.Api.csproj"
```

2) Run

```powershell
dotnet run --project "src\FileIngestion.Api\FileIngestion.Api.csproj"
```

3) Run tests

```powershell
dotnet test "tests\FileIngestion.Application.Tests\FileIngestion.Application.Tests.csproj"
```

More
- See `docs/design.md` for detailed architecture and deployment plan.
- See `docs/code.md` for coding guidelines and standards.
- OpenAPI: `specification/openapi.yaml`
- Postman collection: `specification/postman_collection.json`

Status & next steps
- Scaffold complete. Next recommended work items:
	- Integrate a stable `Azure.Cosmos` package and enable the Cosmos adapter in CI for integration tests.
	- Add integration tests (Azurite or testcontainers) for Blob and Cosmos adapters.
	- Configure GitHub branch protection and required CI checks.

