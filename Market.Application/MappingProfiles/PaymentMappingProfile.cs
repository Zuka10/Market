using AutoMapper;
using Market.Application.DTOs.Market;
using Market.Domain.Entities.Market;

namespace Market.Application.MappingProfiles;

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.IsCompleted))
            .ForMember(dest => dest.DisplayInfo, opt => opt.MapFrom(src => src.DisplayInfo));
    }
}