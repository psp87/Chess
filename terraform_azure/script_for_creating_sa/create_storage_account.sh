#! /bin/bash
# YOU SHOULD BE LOGGED IN YOUR AZURE ACCOUTN via "az login"

RESOURCE_GROUP_NAME=$1-tstate-rg
RESOURCE_GROUP_LOCATION=$2
STORAGE_ACCOUNT_NAME=$1tfstate$RANDOM
CONTAINER_NAME=tfstate

# Create resource group
az group create --resource-group $RESOURCE_GROUP_NAME --location $RESOURCE_GROUP_LOCATION

# Create storage account
az storage account create --name $STORAGE_ACCOUNT_NAME --resource-group $RESOURCE_GROUP_NAME --sku Standard_LRS --encryption-services blob

# Get storage account key
ACCOUNT_KEY=$(az storage account keys list --resource-group $RESOURCE_GROUP_NAME --account-name $STORAGE_ACCOUNT_NAME --query [0].value -o tsv)

# Create blob container
az storage container create --name $CONTAINER_NAME --account-name $STORAGE_ACCOUNT_NAME --account-key $ACCOUNT_KEY
echo "storage_account_name: $STORAGE_ACCOUNT_NAME"
echo "container_name: $CONTAINER_NAME"
echo "access_key: $ACCOUNT_KEY"