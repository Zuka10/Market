using AutoMapper;
using Market.Application.DTOs.Market;
using Market.Domain.Entities.Market;

namespace Market.Application.MappingProfiles;

public class OrderDetailMappingProfile : Profile
{
    public OrderDetailMappingProfile()
    {
        CreateMap<OrderDetail, OrderDetailDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.CalculatedLineTotal, opt => opt.MapFrom(src => src.UnitPrice * src.Quantity))
            .ForMember(dest => dest.CostPrice, opt => opt.MapFrom(src => src.Product != null ? (decimal?)src.Product.Price : null)) // Assuming cost price from product
            .ForMember(dest => dest.Profit, opt => opt.MapFrom(src =>
                src.Product != null ? (decimal?)(src.LineTotal - (src.Product.Price * src.Quantity)) : null))
            .ForMember(dest => dest.ProfitMargin, opt => opt.MapFrom(src =>
                src.LineTotal > 0 && src.Product != null
                    ? (decimal?)((src.LineTotal - (src.Product.Price * src.Quantity)) / src.LineTotal * 100)
                    : null))
            .ForMember(dest => dest.ProfitPerUnit, opt => opt.MapFrom(src =>
                src.Product != null ? (decimal?)(src.UnitPrice - src.Product.Price) : null));
    }
}