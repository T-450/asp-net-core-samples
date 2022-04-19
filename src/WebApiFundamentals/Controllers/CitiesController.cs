using AutoMapper;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiFundamentals.Extensions;
using WebApiFundamentals.Models;

namespace WebApiFundamentals.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cities")]
public class CitiesController : ControllerBase
{
    private const int MaxCitiesPageSize = 20;
    private readonly ICityInfoRepository _cityInfoRepository;
    private readonly IMapper _mapper;

    public CitiesController(ICityInfoRepository cityInfoRepository,
        IMapper mapper)
    {
        _cityInfoRepository = cityInfoRepository ??
                              throw new ArgumentNullException(nameof(cityInfoRepository));
        _mapper = mapper ??
                  throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities(
        string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
    {
        if (pageSize > MaxCitiesPageSize)
        {
            pageSize = MaxCitiesPageSize;
        }

        var (cityEntities, paginationMetadata) = await _cityInfoRepository
            .GetCitiesAsync(name, searchQuery, pageNumber, pageSize).ConfigureAwait(false);

        Response.Headers.Add("X-Pagination", paginationMetadata.ToJson());

        return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities));
    }

    /// <summary>
    ///     Get a city by id
    /// </summary>
    /// <param name="id">The id of the city to get</param>
    /// <param name="includePointsOfInterest">Whether or not to include the points of interest</param>
    /// <returns>An IActionResult</returns>
    /// <response code="200">Returns the requested city</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCity(int id, bool includePointsOfInterest = false)
    {
        var city = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest).ConfigureAwait(false);
        if (city == null)
        {
            return NotFound();
        }

        return includePointsOfInterest
            ? Ok(_mapper.Map<CityDto>(city))
            : Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(city));
    }
}
