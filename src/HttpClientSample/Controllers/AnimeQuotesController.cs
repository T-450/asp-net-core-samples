using System.Net;
using HttpClientSample.Models;
using HttpClientSample.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientSample.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AnimeQuotesController : ControllerBase
{
    private readonly IAnimeQuotesClient _animeQuotesClient;
    private readonly ILogger<AnimeQuotesController> _logger;

    public AnimeQuotesController(IAnimeQuotesClient animeQuotesClient,
        ILogger<AnimeQuotesController> logger)
    {
        _animeQuotesClient = animeQuotesClient;
        _logger = logger;
    }

    // GET: api/AnimeQuotes/random
    [HttpGet("random")]
    public async Task<ActionResult<AnimeQuote?>> GetRandomQuote()
    {
        try
        {
            var randomQuote = await _animeQuotesClient.GetRandomQuote().ConfigureAwait(false);
            if (randomQuote is null)
            {
                return NotFound();
            }

            return Ok(randomQuote);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _logger.LogError(e.StackTrace);
            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
