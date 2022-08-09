// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
namespace HttpClientSample.Models;

/// <summary>
///     AnimeQuote Model
/// </summary>
/// <param name="anime"></param>
/// <param name="character"></param>
/// <param name="quote"></param>
public record AnimeQuote(string anime, string character, string quote);
