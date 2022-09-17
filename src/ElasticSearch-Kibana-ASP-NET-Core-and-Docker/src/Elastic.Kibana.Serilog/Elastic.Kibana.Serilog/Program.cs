using System.Reflection;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

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

var builder = WebApplication.CreateBuilder(args);
var enviroment = builder.Environment.EnvironmentName;

//configure logging first
ConfigureLogging(enviroment);
builder.WebHost
    .ConfigureAppConfiguration(configuration =>
    {
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configuration.AddJsonFile(
            $"appsettings.{enviroment}.json",
            optional: true);
    })
    .UseSerilog();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();