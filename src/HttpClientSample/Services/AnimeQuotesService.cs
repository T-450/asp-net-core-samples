// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
using HttpClientSample.Models;

// This is how the typed client HttpClient looks like.
namespace HttpClientSample.Services;

/// <summary>
///     HttpClient that calls a restful API serving quality anime quotes.
///     https://github.com/rocktimsaikia/anime-chan
/// </summary>
public interface IAnimeQuotesClient
{
    Task<AnimeQuote?> GetRandomQuote();
}

/// <inheritdoc />
public sealed class AnimeQuotesClient : IAnimeQuotesClient
{
    private readonly HttpClient _httpClient;

    public AnimeQuotesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AnimeQuote?> GetRandomQuote()
    {
        var res = await _httpClient.GetAsync("api/random").ConfigureAwait(false);
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<AnimeQuote>().ConfigureAwait(false) : null;
    }
}
