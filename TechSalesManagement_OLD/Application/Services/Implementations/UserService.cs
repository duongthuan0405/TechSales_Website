using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository, 
        IUserProfileRepository userProfileRepository,
        ICacheService cacheService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(GetUserByIdParams parameters)
    {
        var cacheKey = $"users:me:{parameters.UserId}";
        var cached = await _cacheService.GetAsync<User>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("--> Redis Cache Hit for GetMe (UserId: {UserId})", parameters.UserId);
            return cached;
        }

        _logger.LogInformation("--> Redis Cache Miss for GetMe (UserId: {UserId}), loading from DB", parameters.UserId);
        var user = await _userRepository.GetByIdAsync(parameters.UserId);
        if (user != null)
        {
            // Nạp động Profile rời rạc phục vụ cho tổng hợp thông tin cá nhân
            user.profile = await _userProfileRepository.GetByUserIdAsync(parameters.UserId);
            
            // Lưu cache sử dụng thời lượng mặc định
            await _cacheService.SetAsync(cacheKey, user);
        }
        return user;
    }
}
