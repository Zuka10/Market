using AutoMapper;
using Market.Application.DTOs.Market;
using Market.Domain.Entities.Market;

namespace Market.Application.MappingProfiles;

public class VendorMappingProfile : Profile
{
    public VendorMappingProfile()
    {
        CreateMap<Vendor, VendorDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.CommissionRatePercentage, opt => opt.MapFrom(src => src.CommissionRate * 100))
            .ForMember(dest => dest.LocationCount, opt => opt.MapFrom(src => src.VendorLocations != null ? src.VendorLocations.Count : 0))
            .ForMember(dest => dest.VendorLocations, opt => opt.MapFrom(src => src.VendorLocations ?? new List<VendorLocation>()));
    }
}