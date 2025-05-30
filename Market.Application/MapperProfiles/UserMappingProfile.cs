using AutoMapper;
using Market.Application.DTOs.Auth;
using Market.Domain.Entities.Auth;

namespace Market.Application.MapperProfiles;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User Entity to DTO Mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));
    }
}