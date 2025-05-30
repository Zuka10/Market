using AutoMapper;
using Market.Application.DTOs.Auth;
using Market.Application.Features.Auth.Commands.RegisterUser;
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

        CreateMap<UpdateUserCommand, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLower()))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username.Trim()))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName.Trim()))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName.Trim()))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.PhoneNumber) ? null : src.PhoneNumber.Trim()))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Role, opt => opt.Ignore());
    }
}