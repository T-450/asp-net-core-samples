// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
using Serilog.Context;

namespace SerilogLoggingSample.Middlewares;

/// <summary>
///     Ensure that the correlation ID is pushed into every log event
/// </summary>
public class RequestLogContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLogContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", context.GetCorrelationId()))
        {
            return _next.Invoke(context);
        }
    }
}

public static class HttpExtensions
{
    /// <summary>
    ///     In order to correlate logs that belong to the same request, even across multiple applications,
    ///     add a CorrelationId property to your logs.
    ///     Obs: If the application is public facing, you should not rely on the provided correlation ID header.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static string GetCorrelationId(this HttpContext httpContext)
    {
        httpContext.Request.Headers.TryGetValue("Cko-Correlation-Id", out var correlationId);
        return correlationId.FirstOrDefault() ?? httpContext.TraceIdentifier;
    }
}
