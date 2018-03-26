# Kubernetes and Docker on VSTS

To setup Kubernetes cluster and Docker repository on Azure please see : [kubernetes and docker on Azure](k_and_d_azure.md). In this post we are going to look into process of deployment sample .net core app to kubernetes cluster. First we are going to create sample web api which is going to backed with redis database. Next we are going to prepare docker image with our application using docker compose and at least we are going to create automatic build and release in vsts.

## Sample api

Our sample endpoint is going to consist of one controller which is responsible of creating and fetching customer:

```csharp
    [Produces("application/json")]
    [Route("api/customer")]
    public class CustomerController : Controller
    {
        private readonly IDatabaseGateway _database;

        public CustomerController(IDatabaseGateway database)
        {
            _database = database;
        }

        [ValidateModel]
        [HttpPost]
        public IActionResult Create([FromBody] Customer customer)
        {
            var isRegistered = _database.Add(customer);

            if (isRegistered)
            {
                return Ok();
            }

            return StatusCode(409); // already exists
        }

        [Route("{customerName}")]
        [HttpGet]
        public IActionResult Get(string customerName)
        {
            var customer = _database.Get<Customer>(customerName);

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }
    }
```

Here is code of RedisGateway:

```csharp
    public class RedisGateway : IDatabaseGateway
    {
        private readonly RedisManagerPool _manager;

        public RedisGateway(DatabaseSettings databaseSettings)
        {
            _manager = new RedisManagerPool(databaseSettings.Url);
        }

        public bool Add<T>(string key, T item)
        {
            using (var client = _manager.GetClient())
            {
                var isSaved = client.Set(key, item);

                client.Save();

                return isSaved;
            }
        }

        public T Get<T>(string key)
        {
            using (var client = _manager.GetClient())
            {
                var item = client.Get<T>(key);

                return item;
            }
        }
    }
```

Next step is to create Dockerfile and docker-compose files: which can be created by Visual Studio:

In following Dockerfile we are pulling aspnetcore and building/publishing our app which is going to be exposes on port 80.

```
FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0.0 AS build
WORKDIR /src
COPY sample-api.sln ./
COPY src/Sample.Api/Sample.Api.csproj src/Sample.Api/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/src/Sample.Api
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Sample.Api.dll"
```

In docker-compose.yml we define two services: sample api - which is our application and redis which will be also be deployed. To use redis from our app you have to connect to is using : ```redis://db:6379```.

```
version: '3'
services:
  ecpapi:
    image: sampleapi
    build:
      context: .
      dockerfile: src/Sample.Api/Dockerfile
    ports:
      - "80"
  db:
    image: "redis"
```

Next we have to define steps to build our solution in VSTS.
1. Build project
2. Build service - docker compose step that points to our docker-compose.yml file.
3. Publish service - docker compose step that pushes our image produced be previous step to docker repository created in : [kubernetes and docker on Azure](k_and_d_azure.md).
4. Publish build artifacts that points to our kubernetes configuration which should have two files:

deployment.yaml:

```
apiVersion: apps/v1beta1
kind: Deployment
metadata:
  name: sampleapi
spec:
  replicas: 1
  template:
    metadata:
      labels:
        app: sampleapi
    spec:
      containers:
      - name: ecpapi
        image: [url to container repository on azure]/sampleapi
        ports:
        - containerPort: 80
---
apiVersion: apps/v1beta1
kind: Deployment
metadata:
  name: sampleapi-db
spec:
  replicas: 1
  template:
    metadata:
      labels:
        app: sampleapi-db
    spec:
      containers:
      - name: sampleapi-db
        image: redis
        ports:
        - containerPort: 6379
          name: redis
```

service.yaml

```
apiVersion: v1
kind: Service
metadata:
  name: sampleapi
spec:
  type: LoadBalancer
  ports:
  - port: 80
  selector:
    app: sampleapi
---
apiVersion: v1
kind: Service
metadata:
  name: sampleapi-db
spec:
  ports:
  - port: 6379
  selector:
    app: sampleapi-db
```

Last step is to define release build which consists of following steps:
1. Deploy to Kubernetes - apply command that uses ```deployment.yaml``` file.
2. Deploy to Kubernetes - apply command that uses ```service.yaml``` file.
3. Deploy to Kubernetes - set command with following arguments: ```image deployment/sampleapi sampleapi=[url to azure registry]/sampleapi:$(Build.BuildId)```

After your first successful build you can check your service url by typing in console:
```
kubectl get service sampleapi --watch
```
