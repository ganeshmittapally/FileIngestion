output "acr_login_server" {
  value = azurerm_container_registry.acr.login_server
}

output "storage_account_name" {
  value = azurerm_storage_account.storage.name
}

output "blob_container" {
  value = azurerm_storage_container.blob_container.name
}

output "cosmos_account" {
  value = azurerm_cosmosdb_account.cosmos.name
}
