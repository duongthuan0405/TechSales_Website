using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Common;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;
using TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;
using TechSalesManagement.Presentation_WebAPI.Extensions;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/shipping-address")]
public class ShippingAddressController : ControllerBase
{
    private readonly IShippingAddressService _addressService;

    public ShippingAddressController(IShippingAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiSuccessResponse<System.Collections.Generic.List<AddressResponseDto>>>> GetAddressesAsync()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var addresses = await _addressService.GetAddressesByUserIdAsync(userId.Value);

        var responseDtos = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(addresses, a => new AddressResponseDto
        {
            id = a.id,
            province = a.province,
            ward = a.ward,
            detail = a.detail,
            isDefault = a.isDefault
        }));

        return Ok(new ApiSuccessResponse<System.Collections.Generic.List<AddressResponseDto>>(responseDtos));
    }

    [HttpPost]
    public async Task<ActionResult<ApiSuccessResponse<object>>> CreateAddressAsync([FromBody] CreateAddressRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new CreateAddressParams
        {
            UserId = userId.Value,
            Province = request.province,
            Ward = request.ward,
            Detail = request.detail
        };

        await _addressService.CreateAddressAsync(parameters);

        return Created(string.Empty, new ApiSuccessResponse<object>(null, MessageConstants.MSG23));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> UpdateAddressAsync(Guid id, [FromBody] UpdateAddressRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new UpdateAddressParams
        {
            AddressId = id,
            UserId = userId.Value,
            Province = request.province,
            Ward = request.ward,
            Detail = request.detail
        };

        await _addressService.UpdateAddressAsync(parameters);

        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG22));
    }

    [HttpPatch("{id:guid}/default")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> SetDefaultAddressAsync(Guid id)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new SetDefaultAddressParams
        {
            AddressId = id,
            UserId = userId.Value
        };

        await _addressService.SetDefaultAddressAsync(parameters);

        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG21));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> DeleteAddressAsync(Guid id)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new DeleteAddressParams
        {
            AddressId = id,
            UserId = userId.Value
        };

        await _addressService.DeleteAddressAsync(parameters);

        return Ok(new ApiSuccessResponse<object>(null, "Address deleted successfully."));
    }
}
