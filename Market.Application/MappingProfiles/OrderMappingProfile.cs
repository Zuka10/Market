using AutoMapper;
using Market.Application.DTOs.Market;
using Market.Domain.Entities.Market;

namespace Market.Application.MappingProfiles;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : string.Empty))
            .ForMember(dest => dest.DiscountCode, opt => opt.MapFrom(src => src.Discount != null ? src.Discount.DiscountCode : string.Empty))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.TotalPaid, opt => opt.MapFrom(src => 0)) // Default to 0, will be set manually when needed
            .ForMember(dest => dest.AmountDue, opt => opt.MapFrom(src => src.Total)) // Initially all amount is due
            .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => false)) // Default to false, will be set manually when needed
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderDetails != null ? src.OrderDetails.Count : 0))
            .ForMember(dest => dest.LineItemCount, opt => opt.MapFrom(src => src.OrderDetails != null ? (int)src.OrderDetails.Sum(od => od.Quantity) : 0))
            .ForMember(dest => dest.TotalProfit, opt => opt.MapFrom(src => 0)) // Default to 0, will be calculated manually when needed
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => "Pending")) // Default status
            .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails ?? new List<OrderDetail>()))
            .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => new List<PaymentDto>())); // Default empty list
    }
}