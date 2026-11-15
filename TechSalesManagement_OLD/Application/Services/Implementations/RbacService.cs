using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Implementations;

public class RbacService : IRbacService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RbacService(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        return await _roleRepository.GetAllWithPermissionsAsync();
    }

    public async Task<List<Permission>> GetPermissionsAsync()
    {
        return await _permissionRepository.GetAllAsync();
    }

    public async Task UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds, Guid staffId)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(roleId);
        if (role == null) throw new NotFoundException("Role not found.");

        if (role.name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Permissions for Admin role cannot be modified.");

        try
        {
            await _unitOfWork.BeginAsync();

            await _roleRepository.UpdatePermissionsAsync(roleId, permissionIds);

            var auditLog = new AuditLog(staffId, "UPDATE_ROLE_PERMISSIONS", "RolePermissions", roleId.ToString())
            {
                newValues = System.Text.Json.JsonSerializer.Serialize(new { permissionIds = permissionIds })
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

    public async Task AssignUserRolesAsync(Guid userId, List<Guid> roleIds, Guid staffId)
    {
        try
        {
            await _unitOfWork.BeginAsync();

            await _roleRepository.AssignUserRolesAsync(userId, roleIds);

            var auditLog = new AuditLog(staffId, "ASSIGN_USER_ROLES", "UserRoles", userId.ToString())
            {
                newValues = System.Text.Json.JsonSerializer.Serialize(new { roleIds = roleIds })
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
