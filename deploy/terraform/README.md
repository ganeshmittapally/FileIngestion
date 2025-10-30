Terraform skeleton for Azure resources needed by FileIngestion.

This folder contains example Terraform files to create:
- Resource Group
- Azure Container Registry (ACR)
- Storage Account + Blob Container
- Cosmos DB Account and Database
- (Optional) AKS or Azure Container Apps resources

Before applying:
1. Install Terraform and Azure CLI
2. Authenticate with Azure CLI: `az login` and `az account set --subscription <id>`
3. Create a service principal for CI with `az ad sp create-for-rbac --name "fileingestion-ci" --role Contributor --scopes /subscriptions/<subId>/resourceGroups/<rg>` and capture the JSON output as `AZURE_CREDENTIALS` secret in GitHub

Example workflow will expect the following repository secrets:
- `AZURE_CREDENTIALS` (service principal JSON)
- `ACR_NAME` (target ACR name)
- `DATADOG_API_KEY` (for monitoring)

Apply with:

```powershell
cd deploy/terraform
terraform init
terraform plan -out tfplan
terraform apply tfplan
```
