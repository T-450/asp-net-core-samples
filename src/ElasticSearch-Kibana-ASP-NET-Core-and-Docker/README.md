# A Step by Step Guide to Logging with ElasticSearch, Kibana, ASP.NET Core 6 and Docker

[**Reference article**](https://www.humankode.com/asp-net-core/logging-with-elasticsearch-kibana-asp-net-core-and-docker)

0 - Launching Elasticsearch and Kibana in Docker

```bash
mkdir -p elastic-kibana/src/docker
cd elastic-kibana/src/docker

# Create a new file named docker-compose.yml and add this:
version: '3.1'

services:

  elasticsearch:
   container_name: elasticsearch
   image: docker.elastic.co/elasticsearch/elasticsearch:7.9.2
   ports:
    - 9200:9200
   volumes:
    - elasticsearch-data:/usr/share/elasticsearch/data
   environment:
    - xpack.monitoring.enabled=true
    - xpack.watcher.enabled=false
    - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    - discovery.type=single-node
   networks:
    - elastic

  kibana:
   container_name: kibana
   image: docker.elastic.co/kibana/kibana:7.9.2
   ports:
    - 5601:5601
   depends_on:
    - elasticsearch
   environment:
    - ELASTICSEARCH_URL=http://localhost:9200
   networks:
    - elastic
  
networks:
  elastic:
    driver: bridge

volumes:
  elasticsearch-data:
```

1 - Spin up the containers for kibana and elastic:

```bash
# Navigate to http://localhost:9200
docker-compose up -d
```

2 - Verify that Kibana is up and running on addr <http://localhost:5601>

3 - Adding Nuget Packages to the Project

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Sinks.Debug
dotnet add package Serilog.Sinks.Elasticsearch
dotnet add package Serilog.Exceptions
dotnet restore
```

4 - Adding Serilog log level verbosity in appsettings.Development.json

```bash
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Error",
        "System": "Warning"
      }
    }
  },
  "ElasticConfiguration": {
    "Uri": "http://localhost:9200"
  },
  "AllowedHosts": "*"
}
```

5 - Add the config function in program.cs

```cs
// Configure Serilog and Elastic
void ConfigureLogging(string enviromentName)
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile(
            $"appsettings.{enviromentName}.json",
            optional: true)
        .Build();

    var configureElasticSink = (IConfigurationRoot configurationRoot, string enviroment)
        => new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
        {
            AutoRegisterTemplate = true,
            IndexFormat =
                $"{Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-")}-{enviroment.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
        };

    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .Enrich.WithMachineName()
        .WriteTo.Debug()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(configureElasticSink(configuration, enviromentName))
        .Enrich.WithProperty("Environment", enviromentName)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}
```

6 - Configure the AppConfig

```cs
builder.WebHost
    .ConfigureAppConfiguration(configuration =>
    {
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configuration.AddJsonFile(
            $"appsettings.{enviroment}.json",
            optional: true);
    })
    .UseSerilog();
```

### **Start Logging Events to ElasticSearch!**

---

### **Kibana search query examples**

```bash
# Example 1
message: "HomeController Index"

# Example 2
level:"Information"

# Example 3
fields.ActionName:"Elastic.Kibana.Serilog.Controllers.HomeController.Index"

# Example 4
(message:"HomeController Index" AND level: "Information")
```
