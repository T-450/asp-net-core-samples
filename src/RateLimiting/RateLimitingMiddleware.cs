// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
using System.Net;

namespace RateLimiting;

public class RateLimitingMiddleware
{
    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private RequestDelegate _next { get; }
    public Dictionary<string, ClientStatistics> _cache { get; } = new();

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var rateLimitingDecorator = endpoint?.Metadata.GetMetadata<LimitRequests>();
        // Check if the requested endpoint contains the LimitRequests decorator.
        // If no decorator is found the request passes to the next middleware
        if (rateLimitingDecorator is null)
        {
            await _next(context).ConfigureAwait(false);
        }

        // If the decorator is found, generate a unique key based on a combination
        // of the endpoint path and the IP Address of the client - or wahatver you like.
        var key = GenerateClientKey(context);
        var clientStatistics = await GetClientStatisticsByKey(key).ConfigureAwait(false);

        if (clientStatistics != null &&
            DateTime.UtcNow <
            clientStatistics.LastSuccessfulResponseTime.AddSeconds(rateLimitingDecorator !.TimeWindow) &&
            clientStatistics.NumberOfRequestsCompletedSuccessfully == rateLimitingDecorator.MaxRequests)
        {
            context.Response.StatusCode = (int) HttpStatusCode.TooManyRequests;
            return;
        }

        await UpdateClientStatisticsStorage(key, rateLimitingDecorator !.MaxRequests).ConfigureAwait(false);
        await _next(context).ConfigureAwait(false);
    }

    // Ideally for a load-balanced API, we would store the client statistics data in a distributed cache like Redis and Memcached.
    private async Task UpdateClientStatisticsStorage(string key, object maxRequests)
    {
        var clientStat = _cache.ContainsKey(key) ? _cache[key] : null;

        if (clientStat != null)
        {
            clientStat.LastSuccessfulResponseTime = DateTime.UtcNow;

            if (clientStat.NumberOfRequestsCompletedSuccessfully == (int) maxRequests)
            {
                clientStat.NumberOfRequestsCompletedSuccessfully = 1;
            }
            else
            {
                clientStat.NumberOfRequestsCompletedSuccessfully++;
            }

            await Task.FromResult(_cache[key]).ConfigureAwait(false);
        }
        else
        {
            var clientStatistics = new ClientStatistics
            {
                LastSuccessfulResponseTime = DateTime.UtcNow, NumberOfRequestsCompletedSuccessfully = 1
            };

            await Task.FromResult(_cache.TryAdd(key, clientStatistics)).ConfigureAwait(false);
        }
    }

    // We can limit requests within a specified window of time based on the IP address, user id, or a client key.
    private static string GenerateClientKey(HttpContext context)
    {
        return $"{context.Request.Path}_{context.Connection.RemoteIpAddress}";
    }

    private async Task<ClientStatistics?> GetClientStatisticsByKey(string key)
    {
        _cache.TryGetValue(key, out var result);
        return await Task.FromResult(result).ConfigureAwait(false);
    }

// Represents the number of times the specific client was served a response and the time of the last successful response.
    public record ClientStatistics
    {
        public DateTime LastSuccessfulResponseTime { get; set; }
        public int NumberOfRequestsCompletedSuccessfully { get; set; }
    }
}
