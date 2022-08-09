# HttpClientSample goals:

- [x] **Use the HttpClientFactory typed client**, I don't know why
  the ASP.NET team bothered to provide three ways to register a client,
  the typed client is the one to use. It provides type safety and removes
  the need for magic strings.
  - [x] Add configurable options.

- [ ] **Enable GZIP decompression of responses** for better performance. Interestingly,
  the HttpClient and ASP.NET Core does not support compression of GZIP requests, only
  responses.
  Doing some searching online some time ago suggests that this is an optimisation that is
  not very common at all, I thought this was pretty unbelievable at the time.

- [ ] The HttpClient should **time out after the server** does not respond after a set
  amount of time.

- [ ] The HttpClient should **retry requests** which fail due to transient errors.

- [ ] The HttpClient should stop performing new requests for a period of time when a
  consecutive number of requests fail
  using **the circuit breaker pattern**. Failing fast in this way helps to protect an API
  or database that may be under
  high load and means the client gets a failed response quickly rather than waiting for a
  time-out.

- [ ] The URL, time-out, retry and circuit breaker settings should be **configurable from
  the appsettings.json** file.

- [ ] The **HttpClient should send a User-Agent HTTP header** telling the server the name
  and version of the calling application.
  If the server is logging this information, this can be useful for debugging purposes.

- [ ] **The X-Correlation-ID HTTP header from the response should be passed on to the
  request** made using the HttpClient.
  This would make it easy to correlate a request across multiple applications.

- [ ] Add HttpClient Cache.

- [ ] Add HttpClient HeaderValidationHandler.

### Reference:

[You’re using HttpClient wrong and it’s destablizing your software](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/)

[Singleton HttpClient? Beware of this serious behaviour and how to fix it](http://byterot.blogspot.com/2016/07/singleton-httpclient-dns.html)

[Singleton HttpClient doesn’t respect DNS changes](https://github.com/dotnet/corefx/issues/11224)

[HTTPCLIENTFACTORY IN ASP.NET CORE 2.1 (PART 1)](https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore)

[Polly](https://github.com/App-vNext/Polly)

[Make HTTP requests using IHttpClientFactory in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0#consumption-patterns)

[Asp.Net Core High Performance Logs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/loggermessage?view=aspnetcore-2.1)

[CorrelationIdDelegatingHandler, UserAgentDelegatingHandler, Open Telemetry](https://rehansaeed.com/optimally-configuring-asp-net-core-httpclientfactory/)

[HttpClientSample](https://github.com/RehanSaeed/HttpClientSample)

[Use IHttpClientFactory to implement resilient HTTP requests]https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
