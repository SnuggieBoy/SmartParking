using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.User;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for user management.
/// SECURITY: Only Admin can access these endpoints.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/users")]
public sealed class AdminUsersController : BaseApiController
{
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get all users (admin view - paginated, filterable)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserResponseDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? roleName,
        [FromQuery] bool? isActive,
        [FromQuery] bool? emailConfirmed,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new UserFilterDto(search, roleName, isActive, emailConfirmed, page, pageSize);
        var result = await _userService.GetAllAsync(filter, ct);
        return Ok(ApiResponse<PagedResult<UserResponseDto>>.SuccessResponse(result, "Users retrieved successfully"));
    }

    /// <summary>
    /// Get user by ID (admin view)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var user = await _userService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User retrieved successfully"));
    }

    /// <summary>
    /// Update user (admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> Update(
        Guid id,
        [FromBody] UpdateUserDto request,
        CancellationToken ct = default)
    {
        var user = await _userService.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User updated successfully"));
    }

    /// <summary>
    /// Toggle user active status (admin)
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> ToggleActive(Guid id, CancellationToken ct = default)
    {
        var user = await _userService.ToggleActiveAsync(id, ct);
        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User status updated successfully"));
    }
}
