// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
namespace HttpClientSample.Options;

/// <summary>
///     HttpClient options.
/// </summary>
public class HttpClientOptions
{
    public Uri BaseAddress { get; set; }

    public TimeSpan Timeout { get; set; }
}
