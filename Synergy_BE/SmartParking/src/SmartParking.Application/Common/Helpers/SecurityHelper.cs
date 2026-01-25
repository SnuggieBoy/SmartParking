using SmartParking.Application.Common.Exceptions;
using SmartParking.Domain.Constants;

namespace SmartParking.Application.Common.Helpers;

/// <summary>
/// Security helper for validating ownership and permissions at service layer.
/// CRITICAL: Controller-level authorization is NOT sufficient. Service layer MUST validate ownership.
/// </summary>
public static class SecurityHelper
{
    /// <summary>
    /// Validates that the current user owns the resource OR is an Admin.
    /// Throws ForbiddenException if validation fails.
    /// </summary>
    /// <param name="resourceOwnerId">The UserId that owns the resource</param>
    /// <param name="currentUserId">The UserId of the current authenticated user</param>
    /// <param name="isAdmin">Whether the current user has Admin role</param>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <exception cref="ForbiddenException">Thrown when user doesn't own resource and is not Admin</exception>
    public static void ValidateOwnership(
        Guid resourceOwnerId,
        Guid currentUserId,
        bool isAdmin,
        string? errorMessage = null)
    {
        if (!isAdmin && resourceOwnerId != currentUserId)
        {
            throw new ForbiddenException(errorMessage ?? Messages.Common.Forbidden);
        }
    }

    /// <summary>
    /// Validates that the current user IS the specified user OR is an Admin.
    /// Useful for endpoints like "view my profile" where User + Admin should access.
    /// </summary>
    /// <param name="targetUserId">The UserId being accessed</param>
    /// <param name="currentUserId">The UserId of the current authenticated user</param>
    /// <param name="isAdmin">Whether the current user has Admin role</param>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <exception cref="ForbiddenException">Thrown when user is not the target user and is not Admin</exception>
    public static void ValidateUserAccess(
        Guid targetUserId,
        Guid currentUserId,
        bool isAdmin,
        string? errorMessage = null)
    {
        ValidateOwnership(targetUserId, currentUserId, isAdmin, errorMessage);
    }

    /// <summary>
    /// Checks if user owns resource OR is Admin (returns bool instead of throwing).
    /// Useful when you need conditional logic based on ownership.
    /// </summary>
    public static bool CanAccess(Guid resourceOwnerId, Guid currentUserId, bool isAdmin)
    {
        return isAdmin || resourceOwnerId == currentUserId;
    }
}
