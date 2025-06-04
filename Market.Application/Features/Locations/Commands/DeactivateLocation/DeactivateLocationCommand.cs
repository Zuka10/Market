using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Locations.Commands.DeactivateLocation;

public record DeactivateLocationCommand(long LocationId) : ICommand<bool>;