using Serilog;
using Serilog.Events;
using SerilogLoggingSample.Configuration;

// https://github.com/serilog/serilog-aspnetcore#two-stage-initialization
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var config = GetConfiguration();

try
{
    Log.Information("Starting web host");

    var builder = WebApplication
        .CreateBuilder(args)
        .CreateLogger(config);

// Uncomment to add Azure Application Insights
// builder.Services.AddApplicationInsightsTelemetry(config);
    builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();
    // correlate logs that belong to the same request
    // app.UseMiddleware<RequestLogContextMiddleware>();

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Note that the Serilog middleware is added after the health and metrics middleware.
    // This is to avoid generating logs every time your health check endpoints are hit by AWS load balancers.
    app.UseSerilogRequestLogging();

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

static IConfigurationRoot GetConfiguration()
{
    var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile(
            $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
            true,
            true)
        .AddEnvironmentVariables()
        .Build();

    return config;
}
