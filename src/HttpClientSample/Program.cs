using HttpClientSample.Configuration;
using HttpClientSample.Options;
using HttpClientSample.Services;

var builder = WebApplication.CreateBuilder(args);

// Custom extension method that adds custom HttpClient configuration
builder.Services
    // .AddDefaultCorrelationId() // Add Correlation ID support to ASP.NET Core
    .AddControllers()
    .Services
    .AddPolicies(builder.Configuration)
    .AddHttpClient<IAnimeQuotesClient, AnimeQuotesClient, AnimeQuotesClientOptions>(
        builder.Configuration,
        nameof(ApplicationOptions.AnimeQuotesClient));

builder.AddHttpLogging();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// HTTP logging middleware
app.UseHttpLogging();

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
