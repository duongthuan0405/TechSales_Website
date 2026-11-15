using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.HelperServices;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Common;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Implementations;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserProfileRepository _userProfileRepository;

    public AdminUserService(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IRoleRepository roleRepository,
        IUserProfileRepository userProfileRepository)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _roleRepository = roleRepository;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<(List<User> items, int totalCount)> GetCustomersAsync(int pageNumber, int pageSize)
    {
        return await _userRepository.GetPagedUsersByRoleAsync("Customer", pageNumber, pageSize);
    }

    public async Task<(List<User> items, int totalCount)> GetStaffAsync(int pageNumber, int pageSize, Guid requesterId)
    {
        var requester = await _userRepository.GetByIdAsync(requesterId);
        if (requester == null) throw new UnauthorizedException("Requester not found.");

        var roles = new List<string> { "Staff" };
        
        // Technical Admin can see everyone
        if (requester.roles.Any(r => r.name == "Technical Admin"))
        {
            roles.Add("Business Admin");
            roles.Add("Technical Admin");
        }
        // Business Admin can only see Staff (already in the list)
        
        return await _userRepository.GetPagedUsersByRolesAsync(roles.ToArray(), pageNumber, pageSize);
    }

    public async Task<User> CreateStaffAsync(User user, string password, Guid requesterId)
    {
        var requester = await _userRepository.GetByIdAsync(requesterId);
        if (requester == null) throw new UnauthorizedException("Requester not found.");

        bool isBusinessAdmin = requester.roles.Any(r => r.name == "Business Admin");
        bool isTechAdmin = requester.roles.Any(r => r.name == "Technical Admin");

        // Restriction: Business Admin can only create Staff and Business Admin
        if (isBusinessAdmin && !isTechAdmin)
        {
            if (user.roles.Any(r => r.name == "Technical Admin"))
            {
                throw new ForbiddenException("Business Admin does not have permission to create Technical Admin accounts.");
            }
            
            // Allow only Staff or Business Admin
            if (!user.roles.All(r => r.name == "Staff" || r.name == "Business Admin"))
            {
                throw new ForbiddenException("Business Admin can only create Staff or Business Admin accounts.");
            }
        }

        try
        {
            await _unitOfWork.BeginAsync();

            var existing = await _userRepository.GetByEmailAsync(user.email);
            if (existing != null) throw new ConflictException("User with this email already exists.");

            // Finalize user object
            user.id = Guid.NewGuid();
            user.password = _passwordHasher.HashPassword(password);
            user.status = UserStatus.ACTIVE;
            user.createdAt = DateTimeOffset.UtcNow;

            // Resolve roles from DB to ensure they exist and are correctly attached
            var dbRoles = new List<Role>();
            foreach (var role in user.roles)
            {
                var dbRole = await _roleRepository.GetByNameAsync(role.name);
                if (dbRole != null) dbRoles.Add(dbRole);
            }
            user.roles = dbRoles;

            await _userRepository.AddAsync(user);

            // Create empty profile
            var profile = new UserProfile
            {
                userId = user.id,
                fullName = string.Empty,
                phone = string.Empty
            };
            await _userProfileRepository.AddAsync(profile);

            var auditLog = new AuditLog(requesterId, "CREATE_USER", "Users", user.id.ToString())
            {
                newValues = System.Text.Json.JsonSerializer.Serialize(new { email = user.email, roles = user.roles.Select(r => r.name) })
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
            return user;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<User> UpdateStaffAsync(Guid id, User user)
    {
        var existing = await _userRepository.GetByIdAsync(id);
        if (existing == null) throw new NotFoundException("Staff member not found.");

        existing.email = user.email;
        existing.updatedAt = DateTimeOffset.UtcNow;
        
        await _userRepository.UpdateAsync(existing);
        await _unitOfWork.FinishAsync();
        return existing;
    }

    public async Task LockCustomerAsync(Guid userId, DateTimeOffset? until, Guid staffId)
    {
        if (userId == staffId) throw new BadRequestException("You cannot block your own account.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User not found.");

        var requester = await _userRepository.GetByIdAsync(staffId);
        if (requester == null) throw new UnauthorizedException("Requester not found.");

        // Hierarchy check
        if (requester.roles.Any(r => r.name == "Business Admin"))
        {
            if (user.roles.Any(r => r.name == "Technical Admin" || r.name == "Business Admin"))
            {
                throw new ForbiddenException("You do not have permission to lock this administrative account.");
            }
        }

        try
        {
            await _unitOfWork.BeginAsync();

            user.LockAccount(until);
            await _userRepository.UpdateStatusAsync(userId, UserStatus.BLOCKED, until);

            var auditLog = new AuditLog(staffId, "LOCK_USER", "Users", userId.ToString())
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(new { status = UserStatus.ACTIVE.ToString() }),
                newValues = System.Text.Json.JsonSerializer.Serialize(new { status = UserStatus.BLOCKED.ToString(), lockedUntil = until }),
                affectedColumns = "status,lockedUntil"
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task UnlockCustomerAsync(Guid userId, Guid staffId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User not found.");

        var requester = await _userRepository.GetByIdAsync(staffId);
        if (requester == null) throw new UnauthorizedException("Requester not found.");

        // Hierarchy check
        if (requester.roles.Any(r => r.name == "Business Admin"))
        {
            if (user.roles.Any(r => r.name == "Technical Admin" || r.name == "Business Admin"))
            {
                throw new ForbiddenException("You do not have permission to unlock this administrative account.");
            }
        }

        try
        {
            await _unitOfWork.BeginAsync();

            user.UnlockAccount();
            await _userRepository.UpdateStatusAsync(userId, UserStatus.ACTIVE, null);

            var auditLog = new AuditLog(staffId, "UNLOCK_USER", "Users", userId.ToString())
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(new { status = UserStatus.BLOCKED.ToString() }),
                newValues = System.Text.Json.JsonSerializer.Serialize(new { status = UserStatus.ACTIVE.ToString() }),
                affectedColumns = "status,lockedUntil"
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
