using AutoMapper;
using Market.Application.DTOs.Market;
using Market.Domain.Entities.Market;

namespace Market.Application.MappingProfiles;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => 0)) // Default to 0, will be set manually when needed
            .ForMember(dest => dest.AvailableProductCount, opt => opt.MapFrom(src => 0)) // Default to 0, will be set manually when needed
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => new List<ProductDto>())); // Default empty list
    }
}