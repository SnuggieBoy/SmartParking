namespace SmartParking.API.Authorization.Policies;

/// <summary>
/// Centralized authorization policy names for consistent application-wide usage.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy requiring User role (Driver) - can manage own bookings, vehicles, payments
    /// </summary>
    public const string UserOnly = "UserOnly";

    /// <summary>
    /// Policy requiring Owner role - can manage own parking lots and view their bookings
    /// </summary>
    public const string OwnerOnly = "OwnerOnly";

    /// <summary>
    /// Policy requiring Admin role - full system access
    /// </summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>
    /// Policy requiring either Owner or Admin role
    /// </summary>
    public const string OwnerOrAdmin = "OwnerOrAdmin";

    /// <summary>
    /// Policy requiring either User or Admin role
    /// </summary>
    public const string UserOrAdmin = "UserOrAdmin";

    /// <summary>
    /// Policy requiring User, Owner or Admin role
    /// </summary>
    public const string UserOrOwnerOrAdmin = "UserOrOwnerOrAdmin";
}
