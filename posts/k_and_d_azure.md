# Kubernetes and Docker on Azure

## Setup Docker repository

You should start with picking one of your subscriptions:
```
az account list
az account set --subscription [subscription_id]
```

Lets create resource group and container which will host our docker images
```
az group create --name [resource_group] --location eastus

az acr create --resource-group [resource_group] --name [container_name] --sku Basic

az ad sp create-for-rbac --scopes /subscriptions/196e0d0e-0b80-4da5-bbcf-b0ec44cd0eb9/resourceGroups/[resource_group]/providers/Microsoft.ContainerRegistry/registries/[container_name] --role Owner --password [container_name]
```

Next we should be able to login to our container registry and show it's url
```
az acr login --name [container_name]
az acr list --resource-group [resource_group] --query "[].{acrLoginServer:loginServer}" --output table
```

## Setup Kubernetes cluster

Remember about registering service and picking up right subscription:
```
az provider register -n Microsoft.ContainerService
az account list
az account set --subscription 196e0d0e-0b80-4da5-bbcf-b0ec44cd0eb9
```

Next you have to create cluster where you can define number of nodes. [app_id] should be copier form result of "az ad sp" command.
```
az aks create --resource-group [resource_group] --name [cluster_name] --node-count 1 --generate-ssh-keys --service-principal [app_id] --client-secret [container_name]
```

Finally you should get your credential which will be stored in .kube\config file. Please remember that credential will be appended to this file.
```
az aks get-credentials --resource-group [resource_group] --name [cluster_name]
```

To get list of active nodes you can type:
```
kubectl get nodes
```
