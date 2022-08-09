// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
namespace HttpClientSample.Options;

/// <summary>
///     Application Options from appsettings.json
/// </summary>
public class ApplicationOptions
{
    public PolicyOptions Policies { get; set; }

    public AnimeQuotesClientOptions AnimeQuotesClient { get; set; }
}
