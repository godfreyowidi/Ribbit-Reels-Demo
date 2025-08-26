variable "location" {
  description = "Azure region where resources will be deployed"
  type        = string
  default     = "East US" # or change to your preferred region
}

variable "client_id" {
  description = "Azure Service Principal Client ID"
  type        = string
}

variable "client_secret" {
  description = "Azure Service Principal Client Secret"
  type        = string
  sensitive   = true
}

variable "subscription_id" {
  description = "Azure Subscription ID"
  type        = string
}

variable "tenant_id" {
  description = "Azure Tenant ID"
  type        = string
}

variable "sql_admin_user" {
  description = "SQL Server administrator username"
  type        = string
}

variable "sql_admin_password" {
  description = "SQL Server administrator password"
  type        = string
  sensitive   = true
}

variable "jwt_key" {
  description = "JWT signing key"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "JWT issuer"
  type        = string
}

variable "jwt_audience" {
  description = "JWT audience"
  type        = string
}

variable "jwt_expireminutes" {
  description = "JWT token expiration in minutes"
  type        = string
}

variable "google_clientid" {
  description = "Google OAuth Client ID"
  type        = string
}

variable "google_clientsecret" {
  description = "Google OAuth Client Secret"
  type        = string
  sensitive   = true
}

variable "github_owner" {
  description = "GitHub repository owner (used for GHCR)"
  type        = string
}

variable "github_token" {
  description = "GitHub Personal Access Token (used for GHCR)"
  type        = string
  sensitive   = true
}
