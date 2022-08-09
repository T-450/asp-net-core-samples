// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.HttpLogging;

namespace HttpClientSample.Configuration;

/// <summary>
///     Configure Logging
/// </summary>
public static class Logging
{
    public static WebApplicationBuilder AddHttpLogging(this WebApplicationBuilder builder)
    {
        // Configure the default HTTP logging middleware
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-6.0#enabling-http-logging
        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All;
            logging.MediaTypeOptions.AddText("application/javascript");
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
        });
        return builder;
    }
}
