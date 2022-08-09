// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
namespace HttpClientSample.Options;

/// <summary>
///     Options for configuring policies for the HttpClient.
///     The options are set in the appsettings.json.
/// </summary>
public class PolicyOptions
{
    public CircuitBreakerPolicyOptions HttpCircuitBreaker { get; set; }
    public RetryPolicyOptions HttpRetry { get; set; }
}

/// <summary>
///     Options for configuring HttpClient retries.
/// </summary>
public class RetryPolicyOptions
{
    public int Count { get; set; } = 3;
    public int BackoffPower { get; set; } = 2;
}

/// <summary>
///     Options for configuring CircuitBreaker.
/// </summary>
public class CircuitBreakerPolicyOptions
{
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);
    public int ExceptionsAllowedBeforeBreaking { get; set; } = 12;
}
