using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Locations.Commands.UpdateLocation;

public record UpdateLocationCommand(
    long LocationId,
    string Name,
    string? Description,
    string? PostalCode,
    string Address,
    string City,
    string Country,
    bool IsActive
) : ICommand<bool>;