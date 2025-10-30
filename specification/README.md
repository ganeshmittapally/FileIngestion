# API Specification

This folder contains the OpenAPI specification and a Postman collection for the FileIngestion API.

Files:
- `openapi.yaml` — OpenAPI 3.1 specification for the API (hosts and security placeholders included).
- `postman_collection.json` — Postman collection that you can import into Postman to exercise the API.

Placeholders to update before use:
- `{{host}}` in both files — replace with `localhost:5001` for local run or your deployed API host.
- `X-API-KEY` header — set `{{api_key}}` to the API key your gateway or service expects.

Observability placeholders (configured in `src/FileIngestion.Api/appsettings.json`):
- `Observability:Datadog:ApiKey` — set to your Datadog API key (or leave empty for local console exporter)
- `Observability:Datadog:OtLPEndpoint` — set to your OTLP collector endpoint if using OTLP (e.g., Datadog's OTLP endpoint or an internal collector)

Usage:
1. Run the API locally:

```powershell
cd src\FileIngestion.Api
dotnet run
```

2. Import `specification/postman_collection.json` into Postman and update the `host` and `api_key` variables.

3. Use the `Upload File` request to upload a file and create metadata.
