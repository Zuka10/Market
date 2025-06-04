using AutoMapper;
using Market.Application.DTOs.Market;
using Market.Domain.Entities.Market;

namespace Market.Application.MappingProfiles;

public class ProcurementMappingProfile : Profile
{
    public ProcurementMappingProfile()
    {
        CreateMap<Procurement, ProcurementDto>()
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.Name : string.Empty))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : string.Empty))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.ProcurementDetails != null ? src.ProcurementDetails.Count : 0))
            .ForMember(dest => dest.LineItemCount, opt => opt.MapFrom(src => src.ProcurementDetails != null ? src.ProcurementDetails.Sum(pd => pd.Quantity) : 0))
            .ForMember(dest => dest.CalculatedTotal, opt => opt.MapFrom(src => src.ProcurementDetails != null ? src.ProcurementDetails.Sum(pd => pd.LineTotal) : 0))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.ReferenceNo)
                    ? $"{src.ReferenceNo} - {src.ProcurementDate:yyyy-MM-dd}"
                    : $"Procurement #{src.Id} - {src.ProcurementDate:yyyy-MM-dd}"))
            .ForMember(dest => dest.ProcurementDetails, opt => opt.MapFrom(src => src.ProcurementDetails ?? new List<ProcurementDetail>()));
    }
}