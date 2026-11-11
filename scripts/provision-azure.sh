#!/usr/bin/env bash
set -euo pipefail

SUBSCRIPTION_NAME="${AZURE_SUBSCRIPTION:-2026-2TDSPJ-JOAO-H-BATISTA-LEAL-RM564361}"
RESOURCE_GROUP="${RESOURCE_GROUP:-rg-travessia-gs2026}"
LOCATION="${LOCATION:-brazilsouth}"
SQL_LOCATION="${SQL_LOCATION:-brazilsouth}"
APP_SERVICE_PLAN="${APP_SERVICE_PLAN:-plan-travessia-free}"
WEBAPP_NAME="${WEBAPP_NAME:-webapp-travessia-rm564361}"
SQL_SERVER_NAME="${SQL_SERVER_NAME:-sql-travessia-rm564361}"
SQL_DB_NAME="${SQL_DB_NAME:-sqldb-travessia}"
SQL_ADMIN_USER="${SQL_ADMIN_USER:-sqladmin_travessia}"
KEYVAULT_NAME="${KEYVAULT_NAME:-kv-travessia-564361}"

: "${SQL_ADMIN_PASSWORD:?Environment variable SQL_ADMIN_PASSWORD is required}"
: "${SQL_CONNECTION_STRING:?Environment variable SQL_CONNECTION_STRING is required}"
: "${JWT_SECRET_KEY:?Environment variable JWT_SECRET_KEY is required}"

az account set --subscription "$SUBSCRIPTION_NAME"

az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --output none

az appservice plan create \
  --name "$APP_SERVICE_PLAN" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --sku F1 \
  --is-linux \
  --output none

az webapp create \
  --name "$WEBAPP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --plan "$APP_SERVICE_PLAN" \
  --runtime "DOTNETCORE:10.0" \
  --output none

WEBAPP_PRINCIPAL_ID=$(az webapp identity assign \
  --name "$WEBAPP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query principalId \
  --output tsv)

az sql server create \
  --name "$SQL_SERVER_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$SQL_LOCATION" \
  --admin-user "$SQL_ADMIN_USER" \
  --admin-password "$SQL_ADMIN_PASSWORD" \
  --output none

az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "AllowAzureServices" \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0 \
  --output none

az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "AllowProvisioningClient" \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 255.255.255.255 \
  --output none

az sql db create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "$SQL_DB_NAME" \
  --edition Basic \
  --capacity 5 \
  --max-size 2GB \
  --output none

if command -v sqlcmd >/dev/null 2>&1; then
  sqlcmd \
    -S "tcp:${SQL_SERVER_NAME}.database.windows.net,1433" \
    -d "$SQL_DB_NAME" \
    -U "$SQL_ADMIN_USER" \
    -P "$SQL_ADMIN_PASSWORD" \
    -i "./scripts/script-bd.sql"
fi

az keyvault create \
  --name "$KEYVAULT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --sku standard \
  --enable-rbac-authorization true \
  --output none

KEYVAULT_ID=$(az keyvault show \
  --name "$KEYVAULT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query id \
  --output tsv)

KEYVAULT_URI=$(az keyvault show \
  --name "$KEYVAULT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query properties.vaultUri \
  --output tsv)

CURRENT_USER_OBJECT_ID=$(az ad signed-in-user show --query id --output tsv)

az role assignment create \
  --role "Key Vault Secrets Officer" \
  --assignee-object-id "$CURRENT_USER_OBJECT_ID" \
  --assignee-principal-type User \
  --scope "$KEYVAULT_ID" \
  --output none

az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee-object-id "$WEBAPP_PRINCIPAL_ID" \
  --assignee-principal-type ServicePrincipal \
  --scope "$KEYVAULT_ID" \
  --output none

az keyvault secret set \
  --vault-name "$KEYVAULT_NAME" \
  --name "ConnectionStrings--DefaultConnection" \
  --value "$SQL_CONNECTION_STRING" \
  --output none

az keyvault secret set \
  --vault-name "$KEYVAULT_NAME" \
  --name "JwtSettings--SecretKey" \
  --value "$JWT_SECRET_KEY" \
  --output none

az webapp config appsettings set \
  --name "$WEBAPP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --settings \
    KEYVAULT_URI="$KEYVAULT_URI" \
    ASPNETCORE_ENVIRONMENT="Production" \
  --output none
