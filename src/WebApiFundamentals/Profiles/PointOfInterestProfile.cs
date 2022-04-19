using AutoMapper;
using WebApiFundamentals.Entities;
using WebApiFundamentals.Models;

namespace WebApiFundamentals.Profiles;

public class PointOfInterestProfile : Profile
{
    public PointOfInterestProfile()
    {
        CreateMap<PointOfInterest, PointOfInterestDto>();
        CreateMap<PointOfInterestForCreationDto, PointOfInterest>();
        CreateMap<PointOfInterestForUpdateDto, PointOfInterest>();
        CreateMap<PointOfInterest, PointOfInterestForUpdateDto>();
    }
}
