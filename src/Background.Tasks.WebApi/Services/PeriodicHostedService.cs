// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
namespace Background.Tasks.WebApi.Services;

/// <summary>
///     Background service that runs a timer for the periodic invocations.
///     Needs to implement the IHostedService interface in order to registered as a hosted service.
///     To facilitate handling a hosted service use the BackgroundService base class that handles
///     most of the hosted service house keeping.
/// </summary>
public class PeriodicHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _factory;
    private readonly ILogger<PeriodicHostedService> _logger;

    private readonly TimeSpan _period = TimeSpan.FromSeconds(5);
    private int _executionCount;

    public PeriodicHostedService(ILogger<PeriodicHostedService> logger, IServiceScopeFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public bool IsEnabled { get; set; }

    /// <summary>
    ///     The ExecuteAsync method that’s called to run the background service.
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing Periodic Task");
        // Prefer to use PeriodicTimer as it does not block resources
        // see more => https://www.ilkayilknur.com/a-new-modern-timer-api-in-dotnet-6-periodictimer
        using (var timer = new PeriodicTimer(_period))
        {
            // The loop shall run while no cancellation of the background service is requested
            while (!cancellationToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    if (IsEnabled)
                    {
                        // No scoped is created for a hosted service by default so we need to create one using the IServiceScopeFactory
                        // and then get the actual service from the scope.
                        await using var asyncScope = _factory.CreateAsyncScope();
                        // Invoke the sample service to execute the business logic
                        var sampleService = asyncScope.ServiceProvider.GetRequiredService<SampleService>();
                        await sampleService.DoSomethingAsync();

                        _executionCount++;
                        _logger.LogInformation(
                            string.Format("Executed PeriodicHostedService - Count: {count}", _executionCount));
                    }
                    else
                    {
                        _logger.LogInformation("Skipped PeriodicHostedService");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(
                        string.Format("Failed to execute PeriodicHostedService with exception message {msg}.", e.Message));
                    throw;
                }
            }
        }
    }
}