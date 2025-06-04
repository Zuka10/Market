using Market.Application.Common.Interfaces;

namespace Market.Application.Features.VendorLocations.Commands.DeleteVendorLocation;

public record DeleteVendorLocationCommand(long Id) : ICommand<bool>;