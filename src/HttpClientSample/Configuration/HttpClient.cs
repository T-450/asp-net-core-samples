// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
using System.Net;
using HttpClientSample.Options;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace HttpClientSample.Configuration;

/// <summary>
///     Helper type to access policy names.
/// </summary>
internal static class PolicyName
{
    public const string HttpCircuitBreaker = nameof(HttpCircuitBreaker);
    public const string HttpRetry = nameof(HttpRetry);
}

/// <summary>
///     This type just enables Brotli, GZIP and Deflate compression.
/// </summary>
internal class DefaultHttpClientHandler : HttpClientHandler
{
    public DefaultHttpClientHandler()
    {
        AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.Deflate | DecompressionMethods.GZip;
    }
}

/// <summary>
///     Extensions method for HttpClient configuration.
/// </summary>
public static class HttpClient
{
    private const string PoliciesConfigurationSectionName = "Policies";

    /// <summary>
    ///     Adds a retry and circuit breaker policy and configure them using the settings from PolicyOptions.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configurationSectionName"></param>
    /// <returns></returns>
    public static IServiceCollection AddPolicies(this IServiceCollection services, IConfiguration configuration,
        string configurationSectionName = PoliciesConfigurationSectionName)
    {
        services.Configure<PolicyOptions>(configuration);
        var policyOptions = configuration.GetSection(configurationSectionName).Get<PolicyOptions>();
        // https://www.nuget.org/packages/Microsoft.Extensions.Http.Polly/
        var policyRegistry = services.AddPolicyRegistry();
        policyRegistry.Add(
            PolicyName.HttpRetry,
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    policyOptions.HttpRetry.Count,
                    retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(policyOptions.HttpRetry.BackoffPower, retryAttempt))));
        policyRegistry.Add(
            PolicyName.HttpCircuitBreaker,
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    policyOptions.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                    policyOptions.HttpCircuitBreaker.DurationOfBreak));

        return services;
    }

    /// <summary>
    ///     The AddHttpClient method starts by binding the TClientOptions type to a configuration section in appsettings.json.
    ///     TClientOptions is a derived type of HttpClientOptions which just contains a base address and time-out value.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configurationSectionName"></param>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <typeparam name="TClientOptions"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddHttpClient<TClient, TImplementation, TClientOptions>(
        this IServiceCollection services, IConfiguration configuration, string configurationSectionName)
        where TClient : class
        where TImplementation : class, TClient
        where TClientOptions : HttpClientOptions, new()
    {
        return services
            .Configure<TClientOptions>(configuration.GetSection(configurationSectionName))
            // .AddSingleton<CorrelationIdDelegatingHandler>()
            // .AddSingleton<UserAgentDelegatingHandler>()
            .AddHttpClient<TClient, TImplementation>()
            .ConfigureHttpClient(
                (serviceProvider, httpClient) =>
                {
                    var httpClientOptions = serviceProvider
                        .GetRequiredService<IOptions<TClientOptions>>()
                        .Value;
                    httpClient.BaseAddress = httpClientOptions.BaseAddress;
                    httpClient.Timeout = httpClientOptions.Timeout;
                })
            .ConfigurePrimaryHttpMessageHandler(serviceProvider => new DefaultHttpClientHandler())
            .AddPolicyHandlerFromRegistry(PolicyName.HttpRetry)
            .AddPolicyHandlerFromRegistry(PolicyName.HttpCircuitBreaker)
            // .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
            // .AddHttpMessageHandler<UserAgentDelegatingHandler>()
            .Services;
    }
}
