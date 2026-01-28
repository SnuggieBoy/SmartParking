using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.User;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers;

/// <summary>
/// User profile endpoints for authenticated users.
/// </summary>
[Authorize]
[Route("api/users")]
public sealed class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile(CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var profile = await _userService.GetProfileAsync(userId, ct);
        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile, "Profile retrieved successfully"));
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile(
        [FromBody] UpdateUserProfileDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var profile = await _userService.UpdateProfileAsync(userId, request, ct);
        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile, "Profile updated successfully"));
    }
}
