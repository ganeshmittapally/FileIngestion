# Deployment & CI/CD

This doc explains how to provision Azure resources and configure CI/CD to build and publish Docker images to ACR.

## Create Azure Service Principal for CI
Run locally (Azure CLI):

```powershell
az login
az account set --subscription <subscription-id>
az ad sp create-for-rbac --name "fileingestion-ci" --role Contributor --scopes /subscriptions/<subscription-id>/resourceGroups/<resource-group-name> --sdk-auth
```

Copy the JSON output and add it as a GitHub repository secret named `AZURE_CREDENTIALS`.

## Create ACR
Either use the Terraform module in `deploy/terraform` or create ACR manually. Set the repository secret `ACR_NAME` to the ACR name (without `.azurecr.io`).

## GitHub Secrets
Add the following secrets in the GitHub repository (Settings → Secrets → Actions):
- `AZURE_CREDENTIALS` — the service principal JSON from `az ad sp create-for-rbac --sdk-auth`
- `ACR_NAME` — the ACR name
- `DATADOG_API_KEY` — Datadog API key for monitoring (optional)

## Observability placeholders
The application includes placeholders for observability. Configure these values in `src/FileIngestion.Api/appsettings.json` or via environment variables in your deployment platform.

- `Observability:Datadog:ApiKey` — Datadog API key (set as `DATADOG_API_KEY` secret in GitHub if used in workflows)
- `Observability:Datadog:OtLPEndpoint` — OTLP endpoint for exporting traces/metrics (e.g., `https://otlp.example.com:4317`)

If these are not provided the app will fall back to the console exporter for local development.

## Workflow
The workflow `publish-to-acr.yml` will trigger on pushes to `main` and build the Docker image using the Dockerfile at `deploy/docker/Dockerfile` and push to the configured ACR.

## Terraform
See `deploy/terraform` for a skeleton. Run `terraform init` and `terraform apply` after filling variable values.
