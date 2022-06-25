// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
namespace HttpClientFactory.Services;

public interface IGitHubHttpService
{
    Task<int> GetRootDataLength();
}

public class GitHubHttpService : IGitHubHttpService
{
    public GitHubHttpService(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    public HttpClient HttpClient { get; }

    public async Task<int> GetRootDataLength()
    {
        var data = await HttpClient.GetStringAsync("/");

        return data.Length;
    }
}
