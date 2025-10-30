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

## Workflow
The workflow `publish-to-acr.yml` will trigger on pushes to `main` and build the Docker image using the Dockerfile at `deploy/docker/Dockerfile` and push to the configured ACR.

## Terraform
See `deploy/terraform` for a skeleton. Run `terraform init` and `terraform apply` after filling variable values.
