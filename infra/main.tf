terraform {
  required_version = ">= 1.8.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "ribbitreels-rg"
    storage_account_name = "ribbitreelstfstatev2"
    container_name       = "tfstate"
    key                  = "infra.terraform.tfstate"
  }
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
  client_id       = var.client_id
  client_secret   = var.client_secret
  tenant_id       = var.tenant_id
  subscription_id = var.subscription_id
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "ribbitreels-rg"
  location = var.location
}

# Container App Environment
resource "azurerm_container_app_environment" "env" {
  name                = "ribbitreels-env"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
}

# Azure SQL Server
resource "azurerm_mssql_server" "main" {
  name                         = "ribbitrelessql"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_user
  administrator_login_password = var.sql_admin_password
}

# Azure SQL Database
resource "azurerm_mssql_database" "main" {
  name        = "RibbitReelsDb"
  server_id   = azurerm_mssql_server.main.id
  sku_name    = "GP_Gen5_2"
  max_size_gb = 5
}

# Firewall rule for Container App
resource "azurerm_mssql_firewall_rule" "allow_container_app" {
  name             = "allow-container-app"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "255.255.255.255"
}

output "sql_server_fqdn" {
  value = azurerm_mssql_server.main.fully_qualified_domain_name
}

# Container App
resource "azurerm_container_app" "api" {
  name                         = "ribbitreels-api"
  container_app_environment_id = azurerm_container_app_environment.env.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"

  template {
    container {
      name   = "ribbitreels-api"
      image  = "ghcr.io/godfreyowidi/ribbitreels-api:latest"
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "WEBSITES_PORT"
        value = "80"
      }

      env {
        name  = "ASPNETCORE_URLS"
        value = "http://+:80"
      }

      env {
        name  = "ConnectionStrings__DefaultConnection"
        value = "Server=${azurerm_mssql_server.main.fully_qualified_domain_name};Database=${azurerm_mssql_database.main.name};User Id=${var.sql_admin_user};Password=${var.sql_admin_password};TrustServerCertificate=True"
      }

      # App secrets from containerapp secrets
      env {
        name        = "Jwt__Key"
        secret_name = "jwt-key"
      }
      env {
        name        = "Jwt__Issuer"
        secret_name = "jwt-issuer"
      }
      env {
        name        = "Jwt__Audience"
        secret_name = "jwt-audience"
      }
      env {
        name        = "Jwt__ExpireMinutes"
        secret_name = "jwt-expireminutes"
      }
      env {
        name        = "GoogleAuth__ClientId"
        secret_name = "google-clientid"
      }
      env {
        name        = "GoogleAuth__ClientSecret"
        secret_name = "google-clientsecret"
      }
    }
  }

  ingress {
    external_enabled = true
    target_port      = 80

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  # Secrets
  secret {
    name  = "jwt-key"
    value = var.jwt_key
  }

  secret {
    name  = "jwt-issuer"
    value = var.jwt_issuer
  }

  secret {
    name  = "jwt-audience"
    value = var.jwt_audience
  }

  secret {
    name  = "google-clientid"
    value = var.google_clientid
  }

  secret {
    name  = "google-clientsecret"
    value = var.google_clientsecret
  }

  # GHCR Registry auth
  secret {
    name  = "ghcr-token"
    value = var.ghcr_token
  }

  registry {
    server               = "ghcr.io"
    username             = var.ghcr_owner
    password_secret_name = "ghcr-token"
  }
}
