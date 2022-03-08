# Azure Storage Demo with AzureTableStorage and CosmosDb Using DotNet Core 5.0

#### Create New Web App ContactsTableCosmosWebApp

- dotnet new web --name ContactsTableCosmosWebApp -f net5.0

#### DotNet commands

- DotNet Packages for the Project
  - Razor RuntimeCompilation
    - dotnet add package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation --version 5.0.14
  - Azure Storage
    - dotnet add package WindowsAzure.Storage -v 9.3.3 (or latest)
  - Azure CosmosDB API
  - Using .Net 3.0
    - dotnet add package Microsoft.Azure.Cosmos -v 3.25.0
  - For Swagger
    - dotnet add package Swashbuckle.AspNetCore -v 6.2.3
  - For KeyVault
    - dotnet add package Microsoft.Azure.KeyVault -Version 3.0.5 (or latest)
    - dotnet add package Microsoft.Azure.Services.AppAuthentication -Version 1.6.2 (or latest)
    - dotnet add package Microsoft.Extensions.Configuration.AzureKeyVault -v 3.1.22
  - For Redis Cache
    - dotnet add package Microsoft.Extensions.Caching.Redis -Version 2.2.0
  - For Azure Application Insights
    - dotnet add package Microsoft.ApplicationInsights.AspNetCore -Version 2.20.0 or (latest)
    - dotnet add package Microsoft.Extensions.Logging.ApplicationInsights -Version 2.20.0 (for logging ILogger user defined logs in ApplicationInsights)

#### Setup libman

- dotnet tool list -g (will list all the dotnet tools installed globally)
- dotnet tool uninstall --gloabl microsoft.web.librarymanager.cli
- dotnet tool install --gloabl microsoft.web.librarymanager.cli
- dotnet tool uninstall --gloabl dotnet-ef
- dotnet tool install --gloabl dotnet-ef
- dotnet tool update --global microsoft.web.librarymanager.cli
- dotnet tool update --global dotnet-ef
- libman init -p jsdelivr
- libman install bootstrap@5.0.0-beta3 -d wwwroot/lib/bootstrap
- libman install bootstrap-icons@1.4.1 -d wwwroot/lib/bootstrap-icons --files "filename"
- libman restore

```json
{
  "version": "1.0",
  "defaultProvider": "jsdelivr",
  "libraries": [
    {
      "library": "bootstrap@5.1.3",
      "destination": "wwwroot/lib/bootstrap"
    },
    {
      "library": "bootstrap-icons@1.8.1",
      "destination": "wwwroot/lib/bootstrap-icons",
      "files": [
        "font/*",
        "font/fonts/*",
        "icons/chevron-down.svg",
        "icons/cart2.svg",
        "icons/cart3.svg"
      ]
    }
  ]
}
```

#### Note

"StorageAccountInformation--StorageAccountName": "Enter the StorageAccountName"
"StorageAccountInformation--StorageAccountAccessKey": "Enter the Key"
"CosmosConnectionString--CosmosEndpoint"="Enter the URI"
"CosmosConnectionString--CosmosKey"="Enter the Key"
"ApplicationInsights--InstrumentationKey"="Enter the Key"
"ConnectionStrings--RedisConnection"="Enter the ConnectionString"
# ContactsTableCosmosWebApp
