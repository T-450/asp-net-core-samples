namespace Background.Tasks.WebApi.Services;

/// <summary>
///     A MonitorLoop service handles enqueuing tasks for the
///     hosted service whenever the W key is selected on an input device.
/// </summary>
public class MonitorLoop
{
    private readonly CancellationToken _cancellationToken;
    private readonly ILogger _logger;
    private readonly IBackgroundTaskQueue _taskQueue;

    public MonitorLoop(IBackgroundTaskQueue taskQueue,
        ILogger<MonitorLoop> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _cancellationToken = applicationLifetime.ApplicationStopping;
    }

    public Task StartMonitorLoop()
    {
        _logger.LogInformation("MonitorAsync Loop is starting.");

        // Run a console user input loop in a background thread
        return Task.Run(async () => await MonitorAsync());
    }

    private async ValueTask MonitorAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            var keyStroke = Console.ReadKey();

            if (keyStroke.Key == ConsoleKey.W)
            {
                // Enqueue a background work item
                await _taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItem).ConfigureAwait(false);
            }
        }
    }

    private async ValueTask BuildWorkItem(CancellationToken token)
    {
        // Simulate three 5-second tasks to complete
        // for each enqueued work item

        var delayLoop = 0;
        var guid = Guid.NewGuid().ToString();

        _logger.LogInformation("Queued Background Task {Guid} is starting.", guid);

        while (!token.IsCancellationRequested && delayLoop < 3)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if the Delay is cancelled
            }

            delayLoop++;

            _logger.LogInformation("Queued Background Task {Guid} is running. "
                                   + "{DelayLoop}/3", guid, delayLoop);
        }

        if (delayLoop == 3)
        {
            _logger.LogInformation("Queued Background Task {Guid} is complete.", guid);
        }
        else
        {
            _logger.LogInformation("Queued Background Task {Guid} was cancelled.", guid);
        }
    }
}
