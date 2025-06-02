using AutoMapper;
using Market.Application.DTOs.Market;
using Market.Domain.Entities.Market;

namespace Market.Application.MappingProfiles;

public class ProcurementDetailMappingProfile : Profile
{
    public ProcurementDetailMappingProfile()
    {
        CreateMap<ProcurementDetail, ProcurementDetailDto>()
            .ForMember(dest => dest.ProcurementReferenceNo, opt => opt.MapFrom(src => src.Procurement != null ? src.Procurement.ReferenceNo : string.Empty))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.CalculatedLineTotal, opt => opt.MapFrom(src => src.PurchasePrice * src.Quantity))
            .ForMember(dest => dest.PurchasePricePerUnit, opt => opt.MapFrom(src => src.PurchasePrice))
            .ForMember(dest => dest.PotentialProfitPerUnit, opt => opt.MapFrom(src =>
                src.Product != null ? (decimal?)(src.Product.Price - src.PurchasePrice) : null))
            .ForMember(dest => dest.PotentialTotalProfit, opt => opt.MapFrom(src =>
                src.Product != null ? (decimal?)((src.Product.Price - src.PurchasePrice) * src.Quantity) : null));
    }
}