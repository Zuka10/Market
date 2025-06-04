using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Market;

namespace Market.Application.Features.Discounts.Commands.CreateDiscount;

public class CreateDiscountHandler(IUnitOfWork unitOfWork, IMapper mapper) : ICommandHandler<CreateDiscountCommand, DiscountDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<DiscountDto>> Handle(CreateDiscountCommand request, CancellationToken cancellationToken)
    {
        var discount = new Discount
        {
            DiscountCode = request.DiscountCode.Trim().ToUpperInvariant(),
            Description = request.Description?.Trim(),
            Percentage = request.Percentage,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            LocationId = request.LocationId,
            VendorId = request.VendorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdDiscount = await _unitOfWork.Discounts.AddAsync(discount);
        var discountDto = _mapper.Map<DiscountDto>(createdDiscount);

        return BaseResponse<DiscountDto>.Success(discountDto, "Discount created successfully.");
    }
}