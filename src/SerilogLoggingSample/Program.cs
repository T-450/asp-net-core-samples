using Serilog;
using Serilog.Formatting.Compact;
using ILogger = Serilog.ILogger;

var configuration = GetConfiguration();

Log.Logger = CreateSerilogLogger(configuration);

Log.Information("Starting up!");

try
{
    Log.Information("Configuring web host...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging
        .ClearProviders()
        .AddSerilog(Log.Logger);

// Add services to the container.
// Uncomment to add Azure Application Insights
// builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

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

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly!");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

static IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
        .AddEnvironmentVariables();

    return builder.Build();
    ;
}

static ILogger CreateSerilogLogger(IConfiguration? configuration)
{
    // var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    // var logstashUrl = configuration["Serilog:LogstashgUrl"];
    return new LoggerConfiguration()
        .WriteTo.File(new RenderedCompactJsonFormatter(), "logs/logInfo.txt", rollingInterval: RollingInterval.Day)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}
