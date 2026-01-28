using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.User;

namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service for user management (admin operations and user profile)
/// </summary>
public interface IUserService
{
    #region Admin Operations

    /// <summary>
    /// Get all users with pagination and filters (admin only)
    /// </summary>
    Task<PagedResult<UserResponseDto>> GetAllAsync(UserFilterDto filter, CancellationToken ct = default);

    /// <summary>
    /// Get user by ID (admin view)
    /// </summary>
    Task<UserResponseDto> GetByIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Update user (admin)
    /// </summary>
    Task<UserResponseDto> UpdateAsync(Guid userId, UpdateUserDto request, CancellationToken ct = default);

    /// <summary>
    /// Toggle user active status (admin)
    /// </summary>
    Task<UserResponseDto> ToggleActiveAsync(Guid userId, CancellationToken ct = default);

    #endregion

    #region User Profile Operations

    /// <summary>
    /// Get current user's profile
    /// </summary>
    Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Update current user's profile
    /// </summary>
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto request, CancellationToken ct = default);

    #endregion
}
