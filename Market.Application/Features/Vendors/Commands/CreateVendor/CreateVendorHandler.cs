using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Market;

namespace Market.Application.Features.Vendors.Commands.CreateVendor;

public class CreateVendorHandler(IUnitOfWork unitOfWork, IMapper mapper) : ICommandHandler<CreateVendorCommand, VendorDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<VendorDto>> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        // Check if vendor with same email already exists
        var existingVendor = await _unitOfWork.Vendors.GetByEmailAsync(request.Email.Trim());
        if (existingVendor is not null)
        {
            return BaseResponse<VendorDto>.Failure(["Vendor with this email already exists."]);
        }

        var vendor = new Vendor
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.Phone?.Trim()!,
            Address = request.Address?.Trim()!,
            ContactPersonName = request.ContactPerson?.Trim()!,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
        };

        var createdVendor = await _unitOfWork.Vendors.AddAsync(vendor);
        var vendorDto = _mapper.Map<VendorDto>(createdVendor);

        return BaseResponse<VendorDto>.Success(vendorDto, "Vendor created successfully.");
    }
}