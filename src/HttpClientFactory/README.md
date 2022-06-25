# HttpClientFactory Sample

## A sample ASP.NET Core project showing how to configure the HttpClientFactory

More
info [here](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
.

### Overview

In the words of the ASP.NET Team it is "an opinionated factory for creating HttpCient instances";

### What problem does it solve?

HttpClientFactory is designed to help start solving these problems and provides a new mechanism to create
HttpClient instances that **are properly managed** for us behind the scenes. It will “do the right thing” for us and we
can focus on other things! While the above problems are mentioned in reference to HttpClient, in fact the source of
the issues actually occurs on the HttpClientHandler, which is used by HttpClient.

1.The first issue with the (now) old **HttpClient** is when you create too many HttpClients within your code which can
in
turn create two problems…

2.The bigger problem you can have if you create a lot of them is that you can run into socket exhaustion where you have
basically used up too many sockets too fast. There is a limit on how many sockets you can have open at one time.
When you dispose of the HttpClient, the connection it had open remains open for up to 240 seconds in a TIME_WAIT state (
in case any packets from the remote server still come through).

3.Using a single HttpClient in this way will keep connections open and not respect the DNS Time To Live (TTL) setting.
Now the connections will never get DNS updates so the server you are talking to will never have its address updated.
This is entirely possible in some situations where you are balancing over many hosts that may go away over time or
perhaps rolling out new services using blue/green deployments. If the server is gone, the IP your connection is using
may no longer respond to requests that you make through the single HttpClient.

### Using HttpClientFactory (.Net Core 5+)

1.Open Program.cs and register the service;

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Behind the scenes this will register a few required services,
// one of which will be an implementation of IHttpClientFactory.
builder.Services.AddHttpClient();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

2.Update the default WeatherForecastController:

```csharp
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult> Get()
    {
        var httpClient = _httpClientFactory.CreateClient();
        /// Within our Get action we are then using the HttpClientFactory to create a client.
        /// Behind the scenes the HttpClientFactory will create a new HttpClient for us.
        ///
        var res = await httpClient.GetStringAsync("http://www.google.com");
        return Ok(res);
    }
}
```

```csharp

```

### Defining named and typed clients
-

- Often though you’ll want to make multiple requests to the same service, from multiple places in your code.
- HttpClientFactory makes this slightly easier by providing the concept of named clients.
- With named clients, you can create a registration which includes some specific configuration that will be applied when
  creating the HttpClient.

1. Open Program.cs and register in the Services as following:

```csharp
/*
* The first string parameter is the name used for this client registration. The Action<HttpClient> delegate allows us
* to configure our HttpClient when it is constructed for us. This is pretty handy as we can predefine a base address
and some known request headers for example. When we ask for a named client, a new one is created for us and it’ll have
this configuration applied each time.
*/
builder.Services.AddHttpClient("GitHubClient", client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactoryTesting");
});
```

2. Ask for a client by name when calling CreateClient as follows:

```csharp
/*
In this example, we now have an instance of a HttpClient which has the base address set,
so our GetStringAsync method can pass in the relative URI to follow the base address.
*/
    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var client = _httpClientFactory.CreateClient("GitHubClient");
        var result = await client.GetStringAsync("/");

        return Ok(result);
    }
}
// Obs: I’m not a huge fan of the magic strings here so if I were using named clients I’d likely have a static class
// containing string constants for the names of the clients.
```

3.When registering (or requesting) a client we can then use the static class values, instead of the magic string:

```csharp
services.AddHttpClient(**NamedHttpClients.GitHubClient**, client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactoryTesting");
});
```

### Typed Clients

- Typed clients allow us to define custom classes which expect a HttpClient to be injected in via the constructor.
- can be wired up within the DI system using extension methods on the IHttpClientBuilder or using the generic
  AddHttpClient method which accepts the custom type.
- Once we have our custom class, we can either expose the HttpClient directly encapsulate the HTTP calls inside specific
  methods which better define the use of our external service.

1 - Defining our custom typed client class

```csharp
/*
This class needs to accept a HttpClient as a parameter on its constructor.
For now, we’ve set a public property with the instance of the HttpClient.
*/
public class GitHubHttpService
{
    public GitHubHttpService(HttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    private readonly HttpClient _httpClient;

    public HttpClient HttpClient => _httpClient;
}
```

2 - This will be registered with a transient scope in DI.
3 - As our custom typed class accepts a HttpClient this will be wired up within the factory to create an instance with
the appropriately.
4 - Update the Controler:

```csharp
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly GitHubHttpService _gitHubHttpServiceFactory;

    // Update our controller to accept our typed client instead of an IHttpClientFactory
    public WeatherForecastController(GitHubHttpService gitHubHttpServiceFactory)
    {
        _gitHubHttpServiceFactory = gitHubHttpServiceFactory;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult> Get()
    {
        var res = await _gitHubHttpServiceFactory.HttpClient.GetStringAsync("/");

        return Ok(res);
    }
}
```

5 - Since our custom typed client exposes its HttpClient as a property we can use that to make HTTP calls directly.

### ENCAPSULATING THE HTTPCLIENT

Let's use see a case where we want to **encapsulate the HttpClient entirely**.

- This approach is most likely used when we want to define methods which handle specific calls to our endpoint.
- At this point, we could also encapsulate the validation of the response and deserialisation within each method
  so that it is handled in a single place.

```csharp
/*
 Instead of dependants of this class accessing the HttpClient directly, we have provided a GetRootDataLength method
 which performs the HTTP call and returns the length of the response. A trivial example but you get the idea!
*/
public class GitHubHttpService
{
    public GitHubHttpService(HttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    private readonly HttpClient _httpClient;

    public HttpClient HttpClient => _httpClient;

    public async Task<int> GetRootDataLength()
    {
        var data = await _httpClient.GetStringAsync("/");

        return data.Length;
    }
}
```

6 - In Program.cs, register the new DI container for the Service:

```csharp
// The AddHttpClient has a signature which accepts two generic arguments and wires up DI appropriately.
builder.Services.AddHttpClient<IGitHubHttpService, GitHubHttpService>(client=>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactoryTesting");
});
```

6 - On your Controller:

```csharp
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IGitHubHttpService _gitHubHttpClientService;

    // Update our controller to accept our typed client instead of an IHttpClientFactory
    public WeatherForecastController(IGitHubHttpService gitHubHttpClientService)
    {
        _gitHubHttpClientService = gitHubHttpClientService;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult> Get()
    {
        var res = await _gitHubHttpClientService.GetRootDataLength();

        return Ok(res);
    }
}
```

## DELEGATINGHANDLERS

- HttpClientFactory simply makes the consumption of these building blocks easier, through a more composable and clear
  API.
- You can define a chain of handlers as a pipeline, which will all have the chance to process an outgoing HTTP request
  before it is sent.
- These handlers may choose to modify headers programmatically, inspect the body of the request or perhaps log some
  information about the request.
- The HttpRequestMessage flows through each handler in turn under it reaches the final inner handler.
- This handler is what will actually dispatch the HTTP request across the wire.
- The inner handler will also be the first to receive the response.
- Each handler can inspect, modify or use the response as necessary. Perhaps for certain request paths you want to apply
  caching of the returned data for example.
- Much like ASP.NET Core middleware, it also possible for a handler to short-circuit the flow and return a response
  immediately.
-

<img alt="In the diagram above you can see this pipeline visualised." src="https://www.stevejgordon.co.uk/wp-content/uploads/2018/04/ihttpclientfactory-delegatinghandler-outgoing-middleware-pipeline.png">

- One example where this might be useful is to enforce certain rules you may have in place. For example, you could
  create a
  handler which checks if an API key header is present on outgoing requests. If this is missing, then it doesn’t pass
  the
  request along to the next handler (avoiding an actual HTTP call) and instead generates a failure response which it
  returns
  to the caller.

- With IHttpClientFactory we can more quickly apply one or more handlers by defining them when registering our named or
  typed clients. Now, anytime we get an instance of that named or typed client from the HttpClientFactory, it will be
  configured with the required handlers. The easier way to show this is with some code.

### CREATING A HANDLER

1 - To create a handler we can simply create a class which inherits from the DelegatingHandler abstract class. We can
the override the SendAsync method to add our own functionality.

```csharp
/*
In our example this will be our outer request. A StopWatch will be started before calling and awaiting the base handler’s
SendAsync method which will return a HttpResponseMessage.
*/
public class TimingHandler : DelegatingHandler
{
    private readonly ILogger<TimingHandler> _logger;

    public TimingHandler(ILogger<TimingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Starting request");

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation($"Finished request in {sw.ElapsedMilliseconds}ms");

        return response;
    }
}
```

### REGISTERING HANDLERS

- Now that we have created the handlers we wish to use, the final step is to register them with the dependency injection
  container and define a client.

```csharp
// The Order Matter Here
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<TimingHandler>();
    services.AddTransient<ValidateHeaderHandler>();

    services.AddHttpClient("github", c =>
    {
        c.BaseAddress = new Uri("https://api.github.com/");
        c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
    })
    .AddHttpMessageHandler<TimingHandler>() // This handler is on the outside and executes first on the way out and last on the way in.
    .AddHttpMessageHandler<ValidateHeaderHandler>(); // This handler is on the inside, closest to the request.
}
```

### APPLYING A POLICY with Microsoft.Extensions.Http.Polly

The Microsoft.Extensions.Http.Polly package includes an extension method called AddPolicyHandler on the
IHttpClientBuilder that we can use to add a handler which will wrap all requests made using an instance of that client
in a Polly policy. The IHttpClientBuilder is returned when we define a named or typed client.

We can then use the extensions in our ConfigureServices method:

```csharp
// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using HttpClientFactory.Handlers;
using HttpClientFactory.Services;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddTransient<TimingHandler>();
builder.Services.AddTransient<ValidateHeaderHandler>();

// Behind the scenes this will register a few required services,
// one of which will be an implementation of IHttpClientFactory.
// The AddHttpClient has a signature which accepts two generic arguments and wires up DI appropriately.
builder.Services.AddHttpClient<IGitHubHttpService, GitHubHttpService>(client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactoryTesting");
}).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10)))
  .AddHttpMessageHandler<TimingHandler>() // This handler is on the outside and executes first on the way out and last on the way in.
  .AddHttpMessageHandler<ValidateHeaderHandler>();
```

### REUSING POLICIES

- When using Polly, where possible, it is a good practice to define policies once and share them in cases where the same
  policy should be applied.

```csharp
/*
For this example, we’ll declare the timeout policy from the last example once and share it with two named clients…
*/
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

services.AddHttpClient("github")
  .AddPolicyHandler(timeoutPolicy);

services.AddHttpClient("google")
  .AddPolicyHandler(timeoutPolicy);
```

### TRANSIENT FAULT HANDLING

- When dealing with HTTP requests, the most common scenarios we want to handle are transient faults. As this is a common
  requirement, the Microsoft.Extensions.Http.Polly package includes a specific extension that we can use to quickly
  setup policies that handle transient faults.

```csharp
// For example, to add a basic retry when a transient fault occurs for requests from a named client we can register the retry policy as follows:
services.AddHttpClient("github")
  .AddTransientHttpErrorPolicy(p => p.RetryAsync(3));
// In this case, all requests made through the client will retry when certain failure conditions are met.
```

- The AddTransientHttpErrorPolicy method takes a Func<PolicyBuilder<HttpResponseMessage>,
  IAsyncPolicy<HttpResponseMessage>>.

```csharp
/*
    We can then call HttpPolicyExtensions.HandleTranisentHttpError() to get a PolicyBuilder that is configured with the transient fault conditions.
*/
var retryPolicy = HttpPolicyExtensions
  .HandleTransientHttpError()
  .RetryAsync(3);

var noOp = Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();

services.AddHttpClient("github")
  .AddPolicyHandler(request => request.Method == HttpMethod.Get ? retryPolicy : noOp);
```

### USING A POLICYREGISTRY

- Policies can be applied from a policy registry.
- To support policy reuse, Polly provides the concept of a PolicyRegistry which is essentially a container for policies.
  These can be defined at application startup by adding policies into the registry.
- The extensions available on the IHttpClientBuilder also support adding Polly based handlers to a client using a
  registry:

```csharp
/* First, we must register a PolicyRegistry with DI. The Microsoft.Extensions.Http.Polly package includes some
extension methods to make this simple. In the above example, I call the AddPolicyRegistry method which is an
extension on the IServiceCollection. This will create a new PolicyRegistry and add register it in DI as the
implementation for IPolicyRegistry<string> and IReadOnlyPolicyRegistry<string>.
The method returns the policy so that we have access to add policies to it. */

var registry = services.AddPolicyRegistry();

var timeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
var longTimeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));

registry.Add("regular", timeout);
registry.Add("long", longTimeout);

services.AddHttpClient("github")
	.AddPolicyHandlerFromRegistry("regular");
```

- In this example, we’ve added two timeout policies and given them names. Now when registering a client we can call the
  AddPolicyHandlerFromRegistry method available on the IHttpClientBuilder.

### EXPLORING THE DEFAULT REQUEST AND RESPONSE LOGGING AND HOW TO REPLACE THE LOGGING IMPLEMENTATION

- The code in the LoggingHttpMessageHandlerBuilderFilter implementation of the Configure method is responsible for
  creating the two loggers and passing them to logging handlers, which themselves are implementations of the
  DelegatingHandler abstract base class.

```csharp
public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
{
    if (next == null)
    {
        throw new ArgumentNullException(nameof(next));
    }

    return (builder) =>
    {
        // Run other configuration first, we want to decorate.
        next(builder);

        // We want all of our logging message to show up as-if they are coming from HttpClient,
        // but also to include the name of the client for more fine-grained control.
        var outerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler");
        var innerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.ClientHandler");

        // The 'scope' handler goes first so it can surround everything.
        builder.AdditionalHandlers.Insert(0, new LoggingScopeHttpMessageHandler(outerLogger));

        // We want this handler to be last so we can log details about the request after
        // service discovery and security happen.
        builder.AdditionalHandlers.Add(new LoggingHttpMessageHandler(innerLogger));
    };
}
```

### CONFIGURING THE LOGGING OUTPUT

- In the appsettings.json file, you can control and filter the logging which is recorded:

```csharp
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "System.Net.Http.HttpClient": "Information"
    }
  }
}
```

- Let’s imagine we are only interested in logging the requests via a typed client named MyClient.
  Also, perhaps we only want the raw timing of the HTTP request itself. In this example we can enable logging just for
  the
  ClientHandler of our MyClient:

```csharp
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "System.Net.Http.HttpClient.MyClient.ClientHandler": "Information"
    }
  }
}
```

### CUSTOMISING THE LOG MESSAGES

1 - First, we’ll need to create a new implementation of IHttpMessageHandlerBuilderFilter:

```csharp
public class CustomLoggingFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly ILoggerFactory _loggerFactory;

    public CustomLoggingFilter(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        if (next == null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        return (builder) =>
        {
            // Run other configuration first, we want to decorate.
            next(builder);

            var outerLogger =
                _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler");

            builder.AdditionalHandlers.Insert(0, new CustomLoggingScopeHttpMessageHandler(outerLogger));
        };
    }
}
```

- The CustomLoggingScopeHttpMessageHandler class is as follows:

```csharp
ublic class CustomLoggingScopeHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public CustomLoggingScopeHttpMessageHandler(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        using (Log.BeginRequestPipelineScope(_logger, request))
        {
            Log.RequestPipelineStart(_logger, request);
            var response = await base.SendAsync(request, cancellationToken);
            Log.RequestPipelineEnd(_logger, response);

            return response;
        }
    }

    private static class Log
    {
        private static class EventIds
        {
            public static readonly EventId PipelineStart = new EventId(100, "RequestPipelineStart");
            public static readonly EventId PipelineEnd = new EventId(101, "RequestPipelineEnd");
        }

        private static readonly Func<ILogger, HttpMethod, Uri, string, IDisposable> _beginRequestPipelineScope =
            LoggerMessage.DefineScope<HttpMethod, Uri, string>(
                "HTTP {HttpMethod} {Uri} {CorrelationId}");

        private static readonly Action<ILogger, HttpMethod, Uri, string, Exception> _requestPipelineStart =
            LoggerMessage.Define<HttpMethod, Uri, string>(
                LogLevel.Information,
                EventIds.PipelineStart,
                "Start processing HTTP request {HttpMethod} {Uri} [Correlation: {CorrelationId}]");

        private static readonly Action<ILogger, HttpStatusCode, Exception> _requestPipelineEnd =
            LoggerMessage.Define<HttpStatusCode>(
                LogLevel.Information,
                EventIds.PipelineEnd,
                "End processing HTTP request - {StatusCode}");

        public static IDisposable BeginRequestPipelineScope(ILogger logger, HttpRequestMessage request)
        {
            var correlationId = GetCorrelationIdFromRequest(request);
            return _beginRequestPipelineScope(logger, request.Method, request.RequestUri, correlationId);
        }

        public static void RequestPipelineStart(ILogger logger, HttpRequestMessage request)
        {
            var correlationId = GetCorrelationIdFromRequest(request);
            _requestPipelineStart(logger, request.Method, request.RequestUri, correlationId, null);
        }

        public static void RequestPipelineEnd(ILogger logger, HttpResponseMessage response)
        {
            _requestPipelineEnd(logger, response.StatusCode, null);
        }

        private static string GetCorrelationIdFromRequest(HttpRequestMessage request)
        {
            var correlationId = "Not set";

            if (request.Headers.TryGetValues("X-Correlation-ID", out var values))
            {
                correlationId = values.First();
            }

            return correlationId;
        }
    }
}
```

- The final step now that we have our filter implementation is to register it in DI, replacing the existing default
  filter applied by the HttpClientFactory library.

```csharp
services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CustomLoggingFilter>());
```

### Links:

[You’re using HttpClient wrong and it’s destablizing your software](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/)

[Singleton HttpClient? Beware of this serious behaviour and how to fix it](http://byterot.blogspot.com/2016/07/singleton-httpclient-dns.html)

[Singleton HttpClient doesn’t respect DNS changes](https://github.com/dotnet/corefx/issues/11224)

[HTTPCLIENTFACTORY IN ASP.NET CORE 2.1 (PART 1)](https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore)

[Polly](https://github.com/App-vNext/Polly)

[Asp.Net Core High Performance Logs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/loggermessage?view=aspnetcore-2.1)

[CorrelationIdDelegatingHandler, UserAgentDelegatingHandler, Open Telemetry, ] (https://rehansaeed.com/optimally-configuring-asp-net-core-httpclientfactory/)

[HttpClientSample](https://github.com/RehanSaeed/HttpClientSample)
