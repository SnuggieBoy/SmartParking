using Microsoft.AspNetCore.Mvc;
using SmartParking.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartParking.API.Controllers;

/// <summary>
/// Base controller with centralized authentication/authorization helpers.
/// All API controllers MUST inherit from this for consistent claim extraction.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Extracts UserId from JWT claims (Sub or NameIdentifier claim).
    /// SECURITY: Never trust userId from request body - always extract from validated JWT.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">If claim is missing or invalid GUID</exception>
    protected Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user identity in token");
        }
        
        return userId;
    }

    /// <summary>
    /// Checks if current user has Admin role.
    /// Admin role bypasses ownership checks in service layer.
    /// </summary>
    protected bool IsAdmin() => User.IsInRole(AuthConstants.Roles.Admin);

    /// <summary>
    /// Checks if current user has Owner role.
    /// </summary>
    protected bool IsOwner() => User.IsInRole(AuthConstants.Roles.Owner);

    /// <summary>
    /// Gets user's email from JWT claims.
    /// </summary>
    protected string? GetUserEmail() => 
        User.FindFirstValue(JwtRegisteredClaimNames.Email) 
        ?? User.FindFirstValue(ClaimTypes.Email);
}
