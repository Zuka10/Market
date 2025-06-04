using AutoMapper;
using Market.Application.DTOs.Market;
using Market.Domain.Entities.Market;

namespace Market.Application.MappingProfiles;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : string.Empty))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => string.Empty)) // Default empty, will be set manually when needed
            .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.InStock > 0))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => $"{src.Name} - {src.Price:C}"))
            .ForMember(dest => dest.TotalValue, opt => opt.MapFrom(src => src.Price * src.InStock))
            .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src => GetStockStatus(src.InStock)));
    }

    private static string GetStockStatus(int inStock)
    {
        if (inStock <= 0)
        {
            return "Out of Stock";
        }

        return inStock <= 10 ? "Low Stock" : "In Stock";
    }
}