using System;
using System.Threading.Tasks;
using TechSalesManagement.Common;
using TechSalesManagement.Application.Common.Utils;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Implementations;

public class ShippingAddressService : IShippingAddressService
{
    private readonly IShippingAddressRepository _addressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ShippingAddressService(IShippingAddressRepository addressRepository, IUnitOfWork unitOfWork)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateAddressAsync(CreateAddressParams parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.Province) || 
            string.IsNullOrWhiteSpace(parameters.Ward) || 
            string.IsNullOrWhiteSpace(parameters.Detail))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }

        try
        {
            await _unitOfWork.BeginAsync();

            var address = new ShippingAddress(parameters.UserId, parameters.Province, parameters.Ward, parameters.Detail)
            {
                isDefault = false
            };

            // Logic tự động đặt làm mặc định nếu là địa chỉ đầu tiên
            var existingAddresses = await _addressRepository.GetAddressesByUserIdAsync(parameters.UserId);
            if (existingAddresses.Count == 0)
            {
                address.isDefault = true;
            }

            await _addressRepository.AddAsync(address);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAddressAsync(UpdateAddressParams parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.Province) || 
            string.IsNullOrWhiteSpace(parameters.Ward) || 
            string.IsNullOrWhiteSpace(parameters.Detail))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }

        try
        {
            await _unitOfWork.BeginAsync();

            var address = await _addressRepository.GetByIdAsync(parameters.AddressId);
            if (address == null)
            {
                throw new NotFoundException("Address not found.");
            }

            if (address.userId != parameters.UserId)
            {
                throw new ForbiddenException("Access denied to this address.");
            }

            address.province = parameters.Province;
            address.ward = parameters.Ward;
            address.detail = parameters.Detail;

            await _addressRepository.UpdateAsync(address);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task SetDefaultAddressAsync(SetDefaultAddressParams parameters)
    {
        try
        {
            await _unitOfWork.BeginAsync();

            var address = await _addressRepository.GetByIdAsync(parameters.AddressId);
            if (address == null)
            {
                throw new NotFoundException("Address not found.");
            }

            if (address.userId != parameters.UserId)
            {
                throw new ForbiddenException("Access denied to this address.");
            }

            if (address.isDefault)
            {
                // Đã là mặc định rồi thì không cần xử lý gì nữa
                await _unitOfWork.FinishAsync();
                return;
            }

            // Tìm địa chỉ mặc định cũ và hủy kích hoạt
            var oldDefault = await _addressRepository.GetDefaultAddressByUserIdAsync(parameters.UserId);
            if (oldDefault != null)
            {
                oldDefault.isDefault = false;
                await _addressRepository.UpdateAsync(oldDefault);
            }

            // Thiết lập địa chỉ mới làm mặc định
            address.isDefault = true;
            await _addressRepository.UpdateAsync(address);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
    public async Task<System.Collections.Generic.List<ShippingAddress>> GetAddressesByUserIdAsync(Guid userId)
    {
        return await _addressRepository.GetAddressesByUserIdAsync(userId);
    }
    public async Task DeleteAddressAsync(DeleteAddressParams parameters)
    {
        try
        {
            await _unitOfWork.BeginAsync();

            var address = await _addressRepository.GetByIdAsync(parameters.AddressId);
            if (address == null)
            {
                throw new NotFoundException("Address not found.");
            }

            if (address.userId != parameters.UserId)
            {
                throw new ForbiddenException("Access denied to this address.");
            }

            address.deletedAt = DateTimeOffset.UtcNow;
            await _addressRepository.UpdateAsync(address);

            // Nếu xóa địa chỉ mặc định, cố gắng đặt một địa chỉ khác làm mặc định
            if (address.isDefault)
            {
                var remaining = await _addressRepository.GetAddressesByUserIdAsync(parameters.UserId);
                if (remaining.Any())
                {
                    var nextDefault = remaining.First();
                    nextDefault.isDefault = true;
                    await _addressRepository.UpdateAsync(nextDefault);
                }
            }

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
