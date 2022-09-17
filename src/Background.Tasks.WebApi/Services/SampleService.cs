// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
namespace Background.Tasks.WebApi.Services;

/// <summary>
///     A service that represents our business logic that should be invoked by the periodic background tasks.
/// </summary>
public sealed class SampleService
{
    private readonly ILogger<SampleService> _logger;

    public SampleService(ILogger<SampleService> logger)
    {
        _logger = logger;
    }

    public async Task DoSomethingAsync()
    {
        await Task.Delay(100);
        _logger.LogInformation(
            "Sample Service did something.");
    }
}
