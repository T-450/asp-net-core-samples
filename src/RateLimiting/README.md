### What is Rate Limiting?
The process of restricting the number of requests for a resource within a specific time window.

### Why use Rate Limiting?

- **Commercial purposes to generate revenue:** if a user wants more requests they are forced to upgrade the plan;
- **Protection against malicious bot attacks:** A hacker can use bots to make a repeated calls to an API endpoint -
  making the service unavailable, also called DOS (Denial of Service).
- **Regulate traffic according to infrastructure availability** :  Very used by cloud-base APIs that uses the "pay as
  you go" IaaS (Infrastructure as a Service).

### Implementation

Asp.Net Core 6 does not support Rate Limiting out of the box, instead it provides middleware extensibility options for
this purpose.

1 - Create a decorator to decorate the endpoint to throttle;

```cs
    // This attribute only applies to methods.
    [AttributeUsage(AttributeTargets.Method)]
    public class LimitRequests
      :Attribute
	   {
      // Indicate the max requests allowed within a specific time window
      public int TimeWindow { get; set; }
      public int MaxRequests { get; set; }
   }
```

2 - Apply the decorator to the endpoint we want to throttle;
3 - Configure the endpoint with the MaxRequests and TimeWindow. In the example below, we configured so that we only
receive 2 requests for a window of 5 seconds;

```cs
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
	// Now the third request within the window of 5 seconds won't return a successful response
    [HttpGet]
    [ProducesResponseType(typeof(ActionResult<string>), StatusCodes.Status200OK)]
    [LimitRequests(MaxRequests = 2, TimeWindow = 5)]
    public ActionResult<string> GetAll()
    {
        return Ok("Request received");
    }
}
```

4 - Create the class RateLimitingMiddleware.cs as a custom middleware that contains the logic for rate limiting;

```cs
public class RateLimitingMiddleware
{
    private RequestDelegate  _next { get; }
    public Dictionary<string, ClientStatistics> _cache { get; } = new ();

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var rateLimitingDecorator  = endpoint?.Metadata.GetMetadata<LimitRequests>();
        // Check if the requested endpoint contains the LimitRequests decorator.
        // If no decorator is found the request passes to the next middleware
        if (rateLimitingDecorator  is null)
        {
            await _next(context).ConfigureAwait(false);
        }
        // If the decorator is found, generate a unique key based on a combination
        // of the endpoint path and the IP Address of the client - or wahatver you like.
        var key = GenerateClientKey(context);
        var clientStatistics = await GetClientStatisticsByKey(key).ConfigureAwait(false);

        if (clientStatistics != null &&
            DateTime.UtcNow < clientStatistics.LastSuccessfulResponseTime.AddSeconds(rateLimitingDecorator !.TimeWindow) &&
            clientStatistics.NumberOfRequestsCompletedSuccessfully == rateLimitingDecorator .MaxRequests)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
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
                clientStat.NumberOfRequestsCompletedSuccessfully = 1;
            else
                clientStat.NumberOfRequestsCompletedSuccessfully++;

            await Task.FromResult(_cache[key]).ConfigureAwait(false);
        }
        else
        {
            var clientStatistics = new ClientStatistics
            {
                LastSuccessfulResponseTime = DateTime.UtcNow,
                NumberOfRequestsCompletedSuccessfully = 1
            };

            await Task.FromResult(_cache.TryAdd(key, clientStatistics)).ConfigureAwait(false);
        }
    }

    // We can limit requests within a specified window of time based on the IP address, user id, or a client key.
    private static string GenerateClientKey(HttpContext context)
        => $"{context.Request.Path}_{context.Connection.RemoteIpAddress}";

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
```

5 - Finally, configure the middleware on the Startup.cs/Program.cs file:

```cs
app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<RateLimitingMiddleware>();
app.MapControllers();

app.Run();
```

Now you can make requests to your endpoint and see if it works.

You can see a working example [here](https://github.com/edward-teixeira/asp-net-core-samples/tree/master/src/RateLimiting);

Links:
- [The NEW Rate Limiter of .NET 7 is AWESOME](http://obsidian.md)
- [DOS - Denial of Service](https://en.wikipedia.org/wiki/Denial-of-service_attack)
- [What is IaaS?](https://azure.microsoft.com/en-us/overview/what-is-iaas)
- [Rate Limiting in ASP.NET Core Web API](https://code-maze.com/aspnetcore-web-api-rate-limiting/)
- [How to Design a Scalable Rate Limiting Algorithm with Kong API](https://konghq.com/blog/how-to-design-a-scalable-rate-limiting-algorithm)
- [Source](https://github.com/edward-teixeira/asp-net-core-samples/tree/master/src/RateLimiting);
