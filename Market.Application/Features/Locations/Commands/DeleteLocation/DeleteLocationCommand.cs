using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Locations.Commands.DeleteLocation;

public record DeleteLocationCommand(long LocationId) : ICommand<bool>
{
}