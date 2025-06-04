using AutoMapper;
using Market.Application.DTOs.Auth;
using Market.Domain.Entities.Auth;

namespace Market.Application.MapperProfiles;

public class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.UserCount, opt => opt.MapFrom(src => 0)) // Default to 0, will be set manually when needed
            .ForMember(dest => dest.Users, opt => opt.MapFrom(src => new List<UserDto>())); // Default empty list
    }
}