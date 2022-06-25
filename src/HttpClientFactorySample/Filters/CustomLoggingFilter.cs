// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
using System.Net;
using Microsoft.Extensions.Http;

namespace HttpClientFactory.Filters;

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

        return builder =>
        {
            // Run other configuration first, we want to decorate.
            next(builder);

            var outerLogger =
                _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler");

            builder.AdditionalHandlers.Insert(0, new CustomLoggingScopeHttpMessageHandler(outerLogger));
        };
    }
}

public class CustomLoggingScopeHttpMessageHandler : DelegatingHandler
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

        private static class EventIds
        {
            public static readonly EventId PipelineStart = new(100, "RequestPipelineStart");
            public static readonly EventId PipelineEnd = new(101, "RequestPipelineEnd");
        }
    }
}
