// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
using Serilog;
using Serilog.Formatting.Compact;
using ILogger = Serilog.ILogger;

namespace SerilogLoggingSample.Configuration;

public static class Serilog
{
    public static WebApplicationBuilder CreateLogger(this WebApplicationBuilder builder, IConfiguration? configuration)
    {
        // var seqServerUrl = configuration["Serilog:SeqServerUrl"];
        // var logstashUrl = configuration["Serilog:LogstashgUrl"];
        ILogger logger = new LoggerConfiguration()
            .WriteTo.File(new RenderedCompactJsonFormatter(), "logs/log.txt", rollingInterval: RollingInterval.Day)
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        builder.Host.UseSerilog(logger);
        return builder;
    }
}
