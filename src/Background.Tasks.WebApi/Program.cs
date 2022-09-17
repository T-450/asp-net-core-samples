using Background.Tasks.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddScoped<SampleService>();
builder.Services.AddSingleton<PeriodicHostedService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<PeriodicHostedService>());

// builder.Services.AddSingleton<MonitorLoop>();
// builder.Services.AddHostedService<QueuedHostedService>();
// builder.Services.AddSingleton<IBackgroundTaskQueue>(ctx =>
// {
//     var queueCapacity = builder.Configuration.GetValue<int>("QueueCapacity");
//     return new BackgroundTaskQueue(queueCapacity);
//
// });
// await using var provider = new ServiceCollection()
//     .AddScoped<MonitorLoop>()
//     .BuildServiceProvider();
//
// using (var scope = provider.CreateScope())
// {
//     var monitorLoop = scope.ServiceProvider.GetRequiredService<MonitorLoop>();
//     monitorLoop?.StartMonitorLoop();
// }

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

// Return the current state of the background service
app.MapGet("/background",
    (PeriodicHostedService service) => new PeriodicHostedServiceState(service.IsEnabled));
// A patch route set the desired state of our background service
app.MapMethods("/background", new[] {"PATCH"}, (
    PeriodicHostedServiceState state,
    PeriodicHostedService service) =>
{
    service.IsEnabled = state.IsEnabled;
});
app.Run();