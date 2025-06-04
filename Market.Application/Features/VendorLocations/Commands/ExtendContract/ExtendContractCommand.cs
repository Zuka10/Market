using Market.Application.Common.Interfaces;

namespace Market.Application.Features.VendorLocations.Commands.ExtendContract;

public record ExtendContractCommand(
    long Id,
    DateTime NewEndDate
) : ICommand<bool>;