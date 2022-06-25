// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
using System.Diagnostics;

namespace HttpClientFactory.Handlers;

/// <summary>
///     A StopWatch will be started before calling and awaiting the base handler’s
///     SendAsync method which will return a HttpResponseMessage.
/// </summary>
public class TimingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public TimingHandler(ILogger<TimingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Request started");
        var response = await base.SendAsync(request, cancellationToken);
        _logger.LogInformation("Request took: {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

        return response;
    }
}
