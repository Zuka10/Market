using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Locations.Commands.ActivateLocation;

public record ActivateLocationCommand(long LocationId) : ICommand<bool>;