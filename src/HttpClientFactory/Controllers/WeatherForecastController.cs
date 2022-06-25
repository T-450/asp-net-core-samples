// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using HttpClientFactory.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientFactory.Controllers;

public static class NamedHttpClients
{
    public const string GitHubClient = "GitHubClient";
}

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IGitHubHttpService _gitHubHttpClientService;

    // Update our controller to accept our typed client instead of an IHttpClientFactory
    public WeatherForecastController(IGitHubHttpService gitHubHttpClientService)
    {
        _gitHubHttpClientService = gitHubHttpClientService;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult> Get()
    {
        var res = await _gitHubHttpClientService.GetRootDataLength();

        return Ok(res);
    }
}
