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
  features {}
}

resource "azurerm_resource_group" "main" {
  name     = "ribbitreels-rg"
  location = "East US 2"
}

resource "azurerm_container_app_environment" "env" {
  name                = "ribbitreels-env"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
}

resource "azurerm_container_app" "api" {
  name                         = "ribbitreels-api"
  container_app_environment_id = azurerm_container_app_environment.env.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"

  template {
    container {
      name   = "ribbitreels-api"
      image  = "ghcr.io/${var.github_owner}/ribbitreels-api:latest"
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

      # Bind environment variables to secrets
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

    scale {
      min_replicas    = 1
      max_replicas    = 10
      rules           = []
      cooldown_period = 300
    }
  }

  ingress {
    external_enabled = true
    target_port      = 80
    transport        = "Auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  # App secrets
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
    name  = "jwt-expireminutes"
    value = var.jwt_expireminutes
  }
  secret {
    name  = "google-clientid"
    value = var.google_clientid
  }
  secret {
    name  = "google-clientsecret"
    value = var.google_clientsecret
  }
  secret {
    name  = "ghcr-token"
    value = var.github_token
  }

  registry {
    server               = "ghcr.io"
    username             = var.github_owner
    password_secret_name = "ghcr-token"
  }
}
