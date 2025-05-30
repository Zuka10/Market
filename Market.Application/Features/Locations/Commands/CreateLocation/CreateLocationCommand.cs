using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Locations.Commands.CreateLocation;

public record CreateLocationCommand(
    string Name,
    string? Description,
    string Address,
    string City,
    string Country,
    string? PostalCode
) : ICommand<LocationDto>;