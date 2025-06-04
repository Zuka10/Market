using AutoMapper;
using Market.Application.DTOs.Market;
using Market.Domain.Entities.Market;

namespace Market.Application.MappingProfiles;

public class DiscountMappingProfile : Profile
{
    public DiscountMappingProfile()
    {
        CreateMap<Discount, DiscountDto>()
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : string.Empty))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.Name : string.Empty))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.IsCurrentlyValid, opt => opt.MapFrom(src => src.IsActive &&
                (!src.StartDate.HasValue || src.StartDate <= DateTime.UtcNow) &&
                (!src.EndDate.HasValue || src.EndDate >= DateTime.UtcNow)))
            .ForMember(dest => dest.DiscountRateDecimal, opt => opt.MapFrom(src => src.Percentage / 100))
            .ForMember(dest => dest.ValidityPeriod, opt => opt.MapFrom(src => GetValidityPeriod(src.StartDate, src.EndDate)));
    }

    private static string GetValidityPeriod(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue)
        {
            return $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
        }

        if (startDate.HasValue)
        {
            return $"From {startDate:yyyy-MM-dd}";
        }

        return endDate.HasValue ? $"Until {endDate:yyyy-MM-dd}" : "No time limit";
    }
}