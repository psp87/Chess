terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.9.0"
    }
  }

  backend "azurerm" {}
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = true
    }
  }
}

##############################
# module for naming resources
##############################

module "naming" {
  source  = "Azure/naming/azurerm"
  version = "0.1.1"
  prefix  = ["${var.project_name}"]
}

##################################
# resources group + client config
##################################

data "azurerm_client_config" "config" {}

resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = var.resource_group_location

  tags = {
    environment = var.env
    managedBy   = "terraform"
    gitrepo     = var.repository_url
  }
}

###################
# storage account
###################

resource "azurerm_storage_account" "sa" {
  name                     = module.naming.storage_account.name_unique
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = {
    environment = var.env
    managedBy   = "terraform"
    gitrepo     = var.repository_url
  }
}

resource "azurerm_storage_share" "sashare" {
  name                 = module.naming.storage_share.name_unique
  storage_account_name = azurerm_storage_account.sa.name
  quota                = 50
}

#############################
# mssql database and server
#############################

resource "azurerm_mssql_server" "sqldb" {
  name                         = module.naming.mssql_server.name_unique
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password

  tags = {
    environment = var.env
    managedBy   = "terraform"
    gitrepo     = var.repository_url
  }
}

resource "azurerm_mssql_database" "db" {
  name        = var.sql_database_name
  server_id   = azurerm_mssql_server.sqldb.id
  collation   = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb = var.max_size_gb
  sku_name    = var.sql_size

  tags = {
    environment = var.env
    managedBy   = "terraform"
    gitrepo     = var.repository_url
  }
}

resource "azurerm_mssql_firewall_rule" "allow_all_azure_ips" {
  name             = "AllowAllWindowsAzureIps"
  server_id        = azurerm_mssql_server.sqldb.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_database_extended_auditing_policy" "db_policy" {
  database_id                             = azurerm_mssql_database.db.id
  storage_endpoint                        = azurerm_storage_account.sa.primary_blob_endpoint
  storage_account_access_key              = azurerm_storage_account.sa.primary_access_key
  storage_account_access_key_is_secondary = false
  retention_in_days                       = 6
}

########################
# key vault secret
########################

#resource "azurerm_key_vault" "kv" {
#  name                = module.naming.key_vault.name_unique
#  location            = azurerm_resource_group.rg.location
#  resource_group_name = azurerm_resource_group.rg.name
#  tenant_id           = data.azurerm_client_config.config.tenant_id
#  sku_name            = var.kv_sku_name
#
#  access_policy {
#    tenant_id = data.azurerm_client_config.config.tenant_id
#    object_id = data.azurerm_client_config.config.object_id
#
#    key_permissions = [
#      "Create",
#      "Get",
#    ]
#
#    secret_permissions = [
#      "Set",
#      "Get",
#      "Delete",
#      "Purge",
#      "Recover"
#    ]
#  }
#
#  tags = {
#    environment = var.env
#    managedBy   = "terraform"
#    gitrepo     = var.repository_url
#  }
#}
#
#resource "azurerm_key_vault_secret" "kv_sql_pass_secret" {
# name         = module.naming.key_vault_secret.name_unique
#  value        = var.sql_admin_password
#  key_vault_id = azurerm_key_vault.kv.id
#}
#
#resource "azurerm_key_vault_secret" "kv_web_app_name" {
#  name         = module.naming.key_vault_secret.name_unique
#  value        = azurerm_linux_web_app.webapp.name
#  key_vault_id = azurerm_key_vault.kv.id
#}

########################
# linux web app
########################

resource "azurerm_service_plan" "app_plan" {
  name                = module.naming.app_service_plan.name_unique
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  os_type             = "Linux"
  sku_name            = var.web_app_sku_name

  tags = {
    environment = var.env
    managedBy   = "terraform"
    gitrepo     = var.repository_url
  }
}

resource "azurerm_linux_web_app" "webapp" {
  name                = var.web_app_name
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  service_plan_id     = azurerm_service_plan.app_plan.id

  site_config {
    always_on        = false
    app_command_line = "dotnet Chess.Web.dll"

    application_stack {
      dotnet_version = var.dotnet_version
    }
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT" = "Development"
  }

  connection_string {
    name  = "ChessDb"
    type  = "SQLServer"
    value = "Server=tcp:${azurerm_mssql_server.sqldb.fully_qualified_domain_name},1433;Database=${azurerm_mssql_database.db.name};User ID=${azurerm_mssql_server.sqldb.administrator_login};Password='${azurerm_mssql_server.sqldb.administrator_login_password}';Trusted_Connection=False;Encrypt=True;"
  }

  storage_account {
    name         = var.sa_name # wpcontent
    type         = var.sa_type # AzureFiles
    account_name = azurerm_storage_account.sa.name
    access_key   = azurerm_storage_account.sa.primary_access_key
    share_name   = azurerm_storage_share.sashare.name
    mount_path   = var.sa_mounth_path # /home/site/wwwroot/wp-content
  }

  tags = {
    environment = var.env
    managedBy   = "terraform"
    gitrepo     = var.repository_url
  }
}

####################################
# source control for linux web app
####################################

#resource "azurerm_app_service_source_control" "scm" {
#  app_id   = azurerm_linux_web_app.webapp.id
#  repo_url = var.repository_url
#  branch   = var.branch_pointer
#}
