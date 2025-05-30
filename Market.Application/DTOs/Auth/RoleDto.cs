namespace Market.Application.DTOs.Auth;

public class RoleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public List<UserDto> Users { get; set; } = [];
}