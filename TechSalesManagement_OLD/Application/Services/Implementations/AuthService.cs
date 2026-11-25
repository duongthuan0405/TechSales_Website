using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TechSalesManagement.Common;
using TechSalesManagement.Application.Common.Configurations;
using TechSalesManagement.Application.Common.Utils;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.HelperServices;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly FrontendCO _frontendCO;
    private readonly ICacheService _cacheService;
    private readonly JwtCO _jwtCO;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IEmailService emailService,
        IOtpService otpService,
        IUserTokenRepository userTokenRepository,
        IRoleRepository roleRepository,
        IUserProfileRepository userProfileRepository,
        IOptions<FrontendCO> frontendOptions,
        ICacheService cacheService,
        IOptions<JwtCO> jwtOptions)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _emailService = emailService;
        _otpService = otpService;
        _userTokenRepository = userTokenRepository;
        _roleRepository = roleRepository;
        _userProfileRepository = userProfileRepository;
        _frontendCO = frontendOptions.Value;
        _cacheService = cacheService;
        _jwtCO = jwtOptions.Value;
    }

    public async Task<User> RegisterAsync(RegisterParams parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.Email))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }
        if (!ValidationUtils.IsValidEmail(parameters.Email))
        {
            throw new BadRequestException(MessageConstants.MSG2);
        }

        if (string.IsNullOrWhiteSpace(parameters.Password))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }
        if (!ValidationUtils.IsValidPasswordFormat(parameters.Password))
        {
            throw new BadRequestException(MessageConstants.MSG3);
        }

        if (string.IsNullOrWhiteSpace(parameters.ConfirmPassword))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }

        if (parameters.Password != parameters.ConfirmPassword)
        {
            throw new BadRequestException(MessageConstants.MSG4);
        }

        try
        {
            await _unitOfWork.BeginAsync();

            var existingUser = await _userRepository.GetByEmailAsync(parameters.Email);
            User userToReturn;
            
            if (existingUser != null)
            {
                if (existingUser.status != UserStatus.PENDING)
                {
                    throw new ConflictException(MessageConstants.MSG5);
                }

                // Cập nhật mật khẩu mới cho user đang chờ xác nhận
                existingUser.password = _passwordHasher.HashPassword(parameters.Password);
                await _userRepository.UpdateAsync(existingUser);
                
                userToReturn = existingUser;
            }
            else
            {
                // Tạo mới user nếu chưa tồn tại
                var newUser = new User
                {
                    email = parameters.Email,
                    password = _passwordHasher.HashPassword(parameters.Password),
                    status = UserStatus.PENDING
                };
                
                // Gán mặc định vai trò 'Customer' cho người dùng mới
                var customerRole = await _roleRepository.GetByNameAsync("Customer");
                if (customerRole != null)
                {
                    newUser.roles.Add(customerRole);
                }

                // Khởi tạo Profile trống
                var userProfile = new UserProfile
                {
                    fullName = string.Empty,
                    phone = string.Empty
                };

                await _userRepository.AddAsync(newUser);

                // Lưu trữ Profile thông qua repository riêng biệt
                userProfile.userId = newUser.id;
                await _userProfileRepository.AddAsync(userProfile);

                userToReturn = newUser;
            }

            // Tạo OTP mới
            var otpResult = _otpService.GenerateOtp();

            // Cập nhật bản ghi OTP cũ nếu có, nếu không thì tạo mới
            var existingToken = await _userTokenRepository.GetByUserIdAndTypeAsync(userToReturn.id, TokenType.EMAIL_VERIFICATION);
            if (existingToken != null)
            {
                existingToken.token = otpResult.otp;
                existingToken.expiredAt = otpResult.expiredAt;
                existingToken.usedAt = null;
                await _userTokenRepository.UpdateAsync(existingToken);
            }
            else
            {
                var userToken = new UserToken(userToReturn.id, otpResult.otp, TokenType.EMAIL_VERIFICATION, otpResult.expiredAt);
                await _userTokenRepository.AddAsync(userToken);
            }

            var verificationLink = $"{_frontendCO.url}/verify-email?email={userToReturn.email}&token={otpResult.otp}";
            await _emailService.SendVerificationEmailAsync(userToReturn.email, verificationLink);

            await _unitOfWork.FinishAsync();
            
            return userToReturn;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<User> LoginAsync(LoginParams parameters)
    {
        try {
            await _unitOfWork.BeginAsync();
            if (string.IsNullOrWhiteSpace(parameters.Email))
            {
                throw new BadRequestException(MessageConstants.MSG1);
            }
            if (!ValidationUtils.IsValidEmail(parameters.Email))
            {
                throw new BadRequestException(MessageConstants.MSG2);
            }

            if (string.IsNullOrWhiteSpace(parameters.Password))
            {
                throw new BadRequestException(MessageConstants.MSG1);
            }

            var user = await _userRepository.GetByEmailAsync(parameters.Email);

            if (user == null)
            {
                throw new UnauthorizedException(MessageConstants.MSG10);
            }

            // 1. Kiểm tra trạng thái khóa trước khi kiểm tra mật khẩu
            if (user.status == UserStatus.BLOCKED)
            {
                if (!user.lockedUntil.HasValue)
                {
                    // Khóa thủ công bởi Admin (không có thời hạn)
                    throw new ForbiddenException(MessageConstants.MSG9);
                }
                
                if (user.lockedUntil.Value > DateTimeOffset.UtcNow)
                {
                    // Vẫn đang trong thời gian bị khóa tự động
                    throw new ForbiddenException(MessageConstants.MSG9);
                }
                
                // Đã hết thời gian khóa tự động, tự động gỡ khóa
                user.status = UserStatus.ACTIVE;
                user.failedLoginAttempts = 0;
                user.lockedUntil = null;
            }

            if(user.lastFailedAt.HasValue && DateTimeOffset.UtcNow > user.lastFailedAt.Value.AddMinutes(30))
            {
                user.failedLoginAttempts = 0;
                user.lockedUntil = null;
            }

            // 2. Kiểm tra mật khẩu
            if (!_passwordHasher.VerifyPassword(parameters.Password, user.password))
            {
                user.failedLoginAttempts++;
                user.lastFailedAt = DateTimeOffset.UtcNow;
                
                if (user.failedLoginAttempts >= 5)
                {
                    user.status = UserStatus.BLOCKED;
                    user.lockedUntil = DateTimeOffset.UtcNow.AddMinutes(30);
                    await _userRepository.UpdateAsync(user);
                    throw new ForbiddenException(MessageConstants.MSG9);
                }

                
                await _userRepository.UpdateAsync(user);

                throw new UnauthorizedException(MessageConstants.MSG10);
            }

            // 3. Kiểm tra trạng thái PENDING khi đăng nhập thành công
            if (user.status == UserStatus.PENDING)
            {
                throw new UnauthorizedException(MessageConstants.MSG10);
            }

            // Reset hoàn toàn khi đăng nhập thành công để xóa dấu vết các lần lỗi cũ (nếu có)
            user.failedLoginAttempts = 0;
            user.lockedUntil = null;
            user.status = UserStatus.ACTIVE; 
            
            await _unitOfWork.FinishAsync();

            return user;
        }
        catch (Exception) {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task VerifyEmailAsync(VerifyEmailParams parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.Email))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }
        if (!ValidationUtils.IsValidEmail(parameters.Email))
        {
            throw new BadRequestException(MessageConstants.MSG2);
        }

        if (string.IsNullOrWhiteSpace(parameters.Token))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }

        try
        {
            await _unitOfWork.BeginAsync();

            var user = await _userRepository.GetByEmailAsync(parameters.Email);
            if (user == null)
            {
                throw new NotFoundException(MessageConstants.MSG12);
            }

            if (user.status == UserStatus.ACTIVE)
            {
                await _unitOfWork.FinishAsync();
                return;
            }

            var userToken = await _userTokenRepository.GetByUserIdAndTypeAsync(user.id, TokenType.EMAIL_VERIFICATION);
            
            if (userToken == null || userToken.token != parameters.Token || userToken.usedAt != null || DateTimeOffset.UtcNow > userToken.expiredAt)
            {
                throw new BadRequestException(MessageConstants.MSG8);
            }

            // Đánh dấu token đã sử dụng
            userToken.usedAt = DateTimeOffset.UtcNow;
            await _userTokenRepository.UpdateAsync(userToken);

            // Kích hoạt người dùng
            user.status = UserStatus.ACTIVE;
            await _userRepository.UpdateAsync(user);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordParams parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.Email))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }
        if (!ValidationUtils.IsValidEmail(parameters.Email))
        {
            throw new BadRequestException(MessageConstants.MSG2);
        }

        var user = await _userRepository.GetByEmailAsync(parameters.Email);
        if (user == null)
        {
            throw new NotFoundException(MessageConstants.MSG12);
        }

        var otpResult = _otpService.GenerateOtp();

        try
        {
            await _unitOfWork.BeginAsync();

            // Check and manage existing reset tokens
            var existingToken = await _userTokenRepository.GetByUserIdAndTypeAsync(user.id, TokenType.RESET_PASSWORD);
            if (existingToken != null)
            {
                existingToken.token = otpResult.otp;
                existingToken.expiredAt = otpResult.expiredAt;
                existingToken.usedAt = null;
                await _userTokenRepository.UpdateAsync(existingToken);
            }
            else
            {
                var userToken = new UserToken(user.id, otpResult.otp, TokenType.RESET_PASSWORD, otpResult.expiredAt);
                await _userTokenRepository.AddAsync(userToken);
            }

            var resetLink = $"{_frontendCO.url}/reset-password?email={user.email}&token={otpResult.otp}";
            await _emailService.SendPasswordResetEmailAsync(user.email, resetLink);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordParams parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.Email))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }
        if (!ValidationUtils.IsValidEmail(parameters.Email))
        {
            throw new BadRequestException(MessageConstants.MSG2);
        }

        if (string.IsNullOrWhiteSpace(parameters.Token))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }

        if (string.IsNullOrWhiteSpace(parameters.NewPassword))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }
        if (!ValidationUtils.IsValidPasswordFormat(parameters.NewPassword))
        {
            throw new BadRequestException(MessageConstants.MSG3);
        }

        if (string.IsNullOrWhiteSpace(parameters.ConfirmPassword))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }

        if (parameters.NewPassword != parameters.ConfirmPassword)
        {
            throw new BadRequestException(MessageConstants.MSG4);
        }

        try
        {
            await _unitOfWork.BeginAsync();

            var user = await _userRepository.GetByEmailAsync(parameters.Email);
            if (user == null)
            {
                throw new NotFoundException(MessageConstants.MSG12);
            }

            var userToken = await _userTokenRepository.GetByUserIdAndTypeAsync(user.id, TokenType.RESET_PASSWORD);
            
            if (userToken == null || userToken.token != parameters.Token || userToken.usedAt != null || DateTimeOffset.UtcNow > userToken.expiredAt)
            {
                throw new BadRequestException(MessageConstants.MSG8);
            }

            user.password = _passwordHasher.HashPassword(parameters.NewPassword);
            user.failedLoginAttempts = 0;
            user.lockedUntil = null;
            await _userRepository.UpdateAsync(user);

            userToken.usedAt = DateTimeOffset.UtcNow;
            await _userTokenRepository.UpdateAsync(userToken);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task ChangePasswordAsync(ChangePasswordParams parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.CurrentPassword))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }

        if (string.IsNullOrWhiteSpace(parameters.NewPassword))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }
        if (!ValidationUtils.IsValidPasswordFormat(parameters.NewPassword))
        {
            throw new BadRequestException(MessageConstants.MSG3);
        }

        if (string.IsNullOrWhiteSpace(parameters.ConfirmPassword))
        {
            throw new BadRequestException(MessageConstants.MSG1);
        }

        if (parameters.NewPassword != parameters.ConfirmPassword)
        {
            throw new BadRequestException(MessageConstants.MSG4);
        }

        try
        {
            await _unitOfWork.BeginAsync();

            var user = await _userRepository.GetByIdAsync(parameters.UserId);
            if (user == null) throw new NotFoundException(MessageConstants.MSG117);

            if (!_passwordHasher.VerifyPassword(parameters.CurrentPassword, user.password))
            {
                throw new BadRequestException(MessageConstants.MSG18);
            }

            if (parameters.NewPassword == parameters.CurrentPassword)
            {
                throw new BadRequestException(MessageConstants.MSG19);
            }

            user.password = _passwordHasher.HashPassword(parameters.NewPassword);
            await _userRepository.UpdateAsync(user);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task LogoutAsync(string token)
    {
        if (string.IsNullOrEmpty(token)) return;

        var cacheKey = $"blacklist:token:{token}";
        // Blacklist the token in Redis for the exact configured JWT lifetime
        await _cacheService.SetAsync(cacheKey, true, TimeSpan.FromMinutes(_jwtCO.durationInMinutes));
    }
}
