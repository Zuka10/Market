using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Procurements.Commands.DeleteProcurement;

public record DeleteProcurementCommand(long ProcurementId) : ICommand<bool>;