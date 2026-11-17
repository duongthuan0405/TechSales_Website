using CommonModule.Presentation.Extensions;
using AuthModule.Domain.Constants;
using AuthModule.Application.Constants;
using AuthModule.Application.UseCases.GetCustomers;
using AuthModule.Application.UseCases.GetStaff;
using AuthModule.Application.UseCases.CreateUser;
using AuthModule.Application.UseCases.UpdateUser;
using AuthModule.Application.UseCases.LockUser;
using AuthModule.Application.UseCases.UnlockUser;
using AuthModule.Presentation.DTOs;
using AuthModule.Presentation.Authorization;
using CommonModule.BusinessExceptions;
using CommonModule.CustomAttributes;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthModule.Presentation.Controllers;
[ApiController]
[Authorize]
[Route("api/admin/users")]
public sealed class AdminUserController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IAuthorizationService _authorization;

    public AdminUserController(ISender sender, IAuthorizationService authorization)
    {
        _sender = sender;
        _authorization = authorization;
    }

    [HttpGet("customers")]
    [HasPermission(AuthPermission.CUSTOMERS_READ)]
    public async Task<IActionResult> GetCustomers([FromQuery] int pageNumber = AuthLimits.DefaultPageNumber, [FromQuery] int pageSize = AuthLimits.DefaultPageSize, CancellationToken ct = default)
    {
        GetCustomers_UCOutput result = await _sender.Send(new GetCustomers_UCInput(this.GetActorId(), pageNumber, pageSize), ct);
        ApiSuccessResponse<GetCustomers_UCOutput> response = new ApiSuccessResponse<GetCustomers_UCOutput>(result, "Customers retrieved successfully.");
        return Ok(response);
    }

    [HttpGet("staff")]
    [HasPermission(AuthPermission.STAFF_READ)]
    public async Task<IActionResult> GetStaff([FromQuery] int pageNumber = AuthLimits.DefaultPageNumber, [FromQuery] int pageSize = AuthLimits.DefaultPageSize, CancellationToken ct = default)
    {
        string[] roleNames = new string[]
        {
            UserRoleName.STAFF
        };
        if (User.IsInRole(UserRoleName.TECHNICAL_ADMIN))
        {
            roleNames = new string[]
            {
                UserRoleName.STAFF,
                UserRoleName.BUSINESS_ADMIN,
                UserRoleName.TECHNICAL_ADMIN
            };
        }

        GetStaff_UCOutput result = await _sender.Send(new GetStaff_UCInput(this.GetActorId(), roleNames, pageNumber, pageSize), ct);
        ApiSuccessResponse<GetStaff_UCOutput> response = new ApiSuccessResponse<GetStaff_UCOutput>(result, "Staff retrieved successfully.");
        return Ok(response);
    }

    [HttpPost]
    [HasPermission(AuthPermission.USERS_MANAGE)]
    public async Task<IActionResult> Create(CreateUserRequestDto request, CancellationToken ct)
    {
        await AuthorizeAccountAsync(new AccountManagementResource { RequestedRoles = request.Roles });
        CreateUser_UCOutput result = await _sender.Send(new CreateUser_UCInput(this.GetActorId(), request.Email, request.Password, request.Roles), ct);
        ApiSuccessResponse<CreateUser_UCOutput> response = new ApiSuccessResponse<CreateUser_UCOutput>(result, "Account created successfully.");
        return Created(string.Empty, response);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(AuthPermission.USERS_MANAGE)]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequestDto request, CancellationToken ct)
    {
        await AuthorizeAccountAsync(new AccountManagementResource { TargetUserId = id });
        UpdateUser_UCOutput result = await _sender.Send(new UpdateUser_UCInput(this.GetActorId(), id, request.Email), ct);
        ApiSuccessResponse<UpdateUser_UCOutput> response = new ApiSuccessResponse<UpdateUser_UCOutput>(result, "Account updated successfully.");
        return Ok(response);
    }

    [HttpPost("{id:guid}/lock")]
    [HasPermission(AuthPermission.CUSTOMERS_LOCK)]
    public async Task<IActionResult> Lock(Guid id, LockUserRequestDto request, CancellationToken ct)
    {
        await AuthorizeAccountAsync(new AccountManagementResource { TargetUserId = id });
        LockUser_UCOutput result = await _sender.Send(new LockUser_UCInput(this.GetActorId(), id, request.Until), ct);
        ApiSuccessResponse<LockUser_UCOutput> response = new ApiSuccessResponse<LockUser_UCOutput>(null, result.Message);
        return Ok(response);
    }

    [HttpPost("{id:guid}/unlock")]
    [HasPermission(AuthPermission.CUSTOMERS_LOCK)]
    public async Task<IActionResult> Unlock(Guid id, CancellationToken ct)
    {
        await AuthorizeAccountAsync(new AccountManagementResource { TargetUserId = id });
        UnlockUser_UCOutput result = await _sender.Send(new UnlockUser_UCInput(this.GetActorId(), id), ct);
        ApiSuccessResponse<UnlockUser_UCOutput> response = new ApiSuccessResponse<UnlockUser_UCOutput>(null, result.Message);
        return Ok(response);
    }

    private async Task AuthorizeAccountAsync(AccountManagementResource resource)
    {
        if (resource.TargetUserId.HasValue && resource.TargetUserId.Value == this.GetActorId())
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, "Use your own profile/password endpoints to manage your account.");
        }

        AuthorizationResult result = await _authorization.AuthorizeAsync(User, resource, AccountManagementRequirement.PolicyName);
        if (!result.Succeeded)
        {
            throw new BusinessException(BusinessExceptionCode.FORBIDDEN, "You cannot manage this account or assign the requested roles.");
        }
    }
}
