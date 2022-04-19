using AutoMapper;
using WebApiFundamentals.Entities;
using WebApiFundamentals.Models;

namespace WebApiFundamentals.Profiles;

public class CityProfile : Profile
{
    public CityProfile()
    {
        CreateMap<City, CityWithoutPointsOfInterestDto>();
        CreateMap<City, CityDto>();
    }
}
