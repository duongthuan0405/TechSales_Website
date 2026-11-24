using AuthModule.Application.Messages;
using CommonModule.Presentation.Extensions;
using AuthModule.Domain.Constants;
using AuthModule.Application.UseCases.GetCustomers;
using AuthModule.Application.UseCases.GetStaff;
using AuthModule.Application.UseCases.GetUsers;
using AuthModule.Application.UseCases.CreateUser;
using AuthModule.Application.UseCases.UpdateUser;
using AuthModule.Application.UseCases.LockUser;
using AuthModule.Application.UseCases.UnlockUser;
using AuthModule.Presentation.DTOs;
using AuthModule.Presentation.Authorization;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using AuthModule.Application.UseCases.DeactivateUser;

namespace AuthModule.Presentation.Controllers;
[ApiController]
[Authorize]
[Route("api/admin/users")]
public sealed class AdminUserController : ControllerBase
{
    private readonly ISender _sender;

    public AdminUserController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Roles = RoleAccess.AccountWriters)]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetUsers_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = AuthLimits.DefaultPageNumber,
        [FromQuery] int pageSize = AuthLimits.DefaultPageSize, [FromQuery] string? role = null,
        [FromQuery] string? keyword = null, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetUsers_UCInput(this.GetActorId(), pageNumber, pageSize, role, keyword), ct);
        return Ok(new ApiSuccessResponse<GetUsers_UCOutput>(result, ResponseMessage.SuccessUsers));
    }

    [HttpGet("customers")]
    [Authorize(Roles = RoleAccess.CustomerManagement)]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetCustomers_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers([FromQuery] int pageNumber = AuthLimits.DefaultPageNumber, [FromQuery] int pageSize = AuthLimits.DefaultPageSize, [FromQuery] string? keyword = null, CancellationToken ct = default)
    {
        GetCustomers_UCOutput result = await _sender.Send(new GetCustomers_UCInput(this.GetActorId(), pageNumber, pageSize, keyword), ct);
        ApiSuccessResponse<GetCustomers_UCOutput> response = new ApiSuccessResponse<GetCustomers_UCOutput>(result, ResponseMessage.SuccessCustomers);
        return Ok(response);
    }

    [HttpGet("staff")]
    [Authorize(Roles = RoleAccess.Administrators)]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetStaff_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStaff([FromQuery] int pageNumber = AuthLimits.DefaultPageNumber, [FromQuery] int pageSize = AuthLimits.DefaultPageSize, [FromQuery] string? role = null, [FromQuery] string? keyword = null, CancellationToken ct = default)
    {
        GetStaff_UCOutput result = await _sender.Send(new GetStaff_UCInput(this.GetActorId(), pageNumber, pageSize, role, keyword), ct);
        ApiSuccessResponse<GetStaff_UCOutput> response = new ApiSuccessResponse<GetStaff_UCOutput>(result, ResponseMessage.SuccessStaff);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = RoleAccess.AccountWriters)]
    [ProducesResponseType(typeof(ApiSuccessResponse<CreateUser_UCOutput>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateUserRequestDto request, CancellationToken ct)
    {

        CreateUser_UCOutput result = await _sender.Send(new CreateUser_UCInput(this.GetActorId(), request.Email, request.Password, request.Roles), ct);
        ApiSuccessResponse<CreateUser_UCOutput> response = new ApiSuccessResponse<CreateUser_UCOutput>(result, ResponseMessage.SuccessAccountCreated);
        return Created(string.Empty, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleAccess.AccountWriters)]
    [ProducesResponseType(typeof(ApiSuccessResponse<UpdateUser_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequestDto request, CancellationToken ct)
    {

        UpdateUser_UCOutput result = await _sender.Send(new UpdateUser_UCInput(this.GetActorId(), id, request.Email), ct);
        ApiSuccessResponse<UpdateUser_UCOutput> response = new ApiSuccessResponse<UpdateUser_UCOutput>(result, ResponseMessage.SuccessAccountUpdated);
        return Ok(response);
    }

    [HttpPost("{id:guid}/lock")]
    [Authorize(Roles = RoleAccess.AccountStatusWriters)]
    [ProducesResponseType(typeof(ApiSuccessResponse<LockUser_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Lock(Guid id, LockUserRequestDto request, CancellationToken ct)
    {

        LockUser_UCOutput result = await _sender.Send(new LockUser_UCInput(this.GetActorId(), id, request.Until), ct);
        ApiSuccessResponse<LockUser_UCOutput> response = new ApiSuccessResponse<LockUser_UCOutput>(null, result.Message);
        return Ok(response);
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = RoleAccess.Administrators)]
    [ProducesResponseType(typeof(ApiSuccessResponse<DeactivateUser_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        DeactivateUser_UCOutput result = await _sender.Send(new DeactivateUser_UCInput(this.GetActorId(), id), ct);
        return Ok(new ApiSuccessResponse<DeactivateUser_UCOutput>(null, result.Message));
    }

    [HttpPost("{id:guid}/unlock")]
    [Authorize(Roles = RoleAccess.AccountStatusWriters)]
    [ProducesResponseType(typeof(ApiSuccessResponse<UnlockUser_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Unlock(Guid id, CancellationToken ct)
    {

        UnlockUser_UCOutput result = await _sender.Send(new UnlockUser_UCInput(this.GetActorId(), id), ct);
        ApiSuccessResponse<UnlockUser_UCOutput> response = new ApiSuccessResponse<UnlockUser_UCOutput>(null, result.Message);
        return Ok(response);
    }

}
