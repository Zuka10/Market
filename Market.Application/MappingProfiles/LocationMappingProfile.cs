using AutoMapper;
using Market.Application.DTOs.Market;
using Market.Domain.Entities.Market;

namespace Market.Application.MappingProfiles;

public class LocationMappingProfile : Profile
{
    public LocationMappingProfile()
    {
        CreateMap<Location, LocationDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.FullAddress, opt => opt.MapFrom(src => $"{src.Address}, {src.City}".Trim(',')))
            .ForMember(dest => dest.VendorCount, opt => opt.MapFrom(src => 0)) // Default to 0, will be set manually when needed
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => 0)) // Default to 0, will be set manually when needed
            .ForMember(dest => dest.VendorLocations, opt => opt.MapFrom(src => new List<VendorLocationDto>()))
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => new List<ProductDto>()));
    }
}