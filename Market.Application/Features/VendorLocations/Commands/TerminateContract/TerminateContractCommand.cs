using Market.Application.Common.Interfaces;

namespace Market.Application.Features.VendorLocations.Commands.TerminateContract;

public record TerminateContractCommand(
    long Id,
    DateTime? TerminationDate = null,
    string? Reason = null
) : ICommand<bool>;