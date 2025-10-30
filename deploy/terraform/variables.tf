variable "resource_group_name" {
  type        = string
  description = "Name of the resource group"
}

variable "location" {
  type        = string
  default     = "eastus"
}

variable "acr_name" {
  type        = string
  description = "ACR name (lowercase)"
}

variable "storage_account_name" {
  type        = string
  description = "Storage account name"
}

variable "blob_container_name" {
  type        = string
  default     = "file-ingestion"
}

variable "cosmos_account_name" {
  type        = string
  description = "Cosmos account name"
}

variable "cosmos_database_name" {
  type        = string
  default     = "FileIngestionDb"
}
