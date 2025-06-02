using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Locations.Commands.ActivateLocation;

public record ActivateLocationCommand(long LocationId) : ICommand<bool>;