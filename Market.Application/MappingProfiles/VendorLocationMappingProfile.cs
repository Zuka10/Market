using AutoMapper;
using Market.Application.DTOs.Market;
using Market.Domain.Entities.Market;

namespace Market.Application.MappingProfiles;

public class VendorLocationMappingProfile : Profile
{
    public VendorLocationMappingProfile()
    {
        CreateMap<VendorLocation, VendorLocationDto>()
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.Name : string.Empty))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : string.Empty))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.IsCurrentlyActive, opt => opt.MapFrom(src =>
                src.IsActive &&
                src.StartDate <= DateTime.UtcNow &&
                (!src.EndDate.HasValue || src.EndDate > DateTime.UtcNow)))
            .ForMember(dest => dest.DaysActive, opt => opt.MapFrom(src =>
                src.EndDate.HasValue
                    ? (int)(src.EndDate.Value - src.StartDate).TotalDays
                    : (int)(DateTime.UtcNow - src.StartDate).TotalDays))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => GetDisplayName(src.Vendor, src.Location, src.StallNumber)));
    }

    private static string GetDisplayName(Vendor? vendor, Location? location, string? stallNumber)
    {
        var vendorName = vendor?.Name ?? "Unknown";

        if (!string.IsNullOrEmpty(stallNumber))
        {
            return $"{vendorName} - Stall {stallNumber}";
        }

        var locationName = location?.Name ?? "Unknown Location";
        return $"{vendorName} - {locationName}";
    }
}