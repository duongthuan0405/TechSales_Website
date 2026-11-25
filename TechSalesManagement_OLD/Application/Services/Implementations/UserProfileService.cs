using System;
using System.Threading.Tasks;
using TechSalesManagement.Common;
using TechSalesManagement.Application.Common.Utils;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Application.HelperServices;

namespace TechSalesManagement.Application.Services.Implementations;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;
    private readonly ICacheService _cacheService;

    public UserProfileService(
        IUserProfileRepository userProfileRepository, 
        IUnitOfWork unitOfWork,
        IImageService imageService,
        ICacheService cacheService)
    {
        _userProfileRepository = userProfileRepository;
        _unitOfWork = unitOfWork;
        _imageService = imageService;
        _cacheService = cacheService;
    }

    public async Task UpdateProfileAsync(UpdateProfileParams parameters)
    {
        try
        {
            await _unitOfWork.BeginAsync();

            var profile = await _userProfileRepository.GetByUserIdAsync(parameters.UserId);
            
            if (profile == null)
            {
                // Vì khi Đăng ký đã tự động khởi tạo Profile rỗng, 
                // trường hợp không tìm thấy Profile là dữ liệu không đồng nhất.
                throw new NotFoundException(MessageConstants.MSG117); 
            }

            var fullName = parameters.FullName;
            var phone = parameters.Phone;

            // Xử lý FullName (Bắt buộc)
            if (string.IsNullOrWhiteSpace(fullName))
            {
                if (string.IsNullOrWhiteSpace(profile.fullName)) 
                    throw new BadRequestException(MessageConstants.MSG1);
                else {
                    fullName = profile.fullName; 
                }
            }
            
            if (string.IsNullOrWhiteSpace(phone))
            {
                if (string.IsNullOrWhiteSpace(profile.phone)) 
                    throw new BadRequestException(MessageConstants.MSG16);
                else {
                    phone = profile.phone; 
                }
            }

            if (!ValidationUtils.IsValidPhoneNumber(phone))
            {
                throw new BadRequestException(MessageConstants.MSG16);
            }

            string? avatarUrl = parameters.AvatarUrl;
            if (parameters.AvatarFile != null)
            {
                avatarUrl = await _imageService.UploadImageAsync(parameters.AvatarFile);
            }

            profile.fullName = fullName;
            profile.phone = phone;            
            if (avatarUrl != null) profile.avatarUrl = avatarUrl;
            if (parameters.DateOfBirth != null) profile.dateOfBirth = parameters.DateOfBirth;

            await _userProfileRepository.UpdateAsync(profile);

            await _unitOfWork.FinishAsync();

            // Invalidate the GetMe cache for this user
            await _cacheService.RemoveAsync($"users:me:{parameters.UserId}");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
