namespace SmartParking.Application.DTOs.User;

/// <summary>
/// User response DTO for admin
/// </summary>
public sealed record UserResponseDto(
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    string RoleName,
    bool? IsActive,
    bool EmailConfirmed,
    DateTime? CreatedAt,
    int TotalBookings,
    int TotalParkingLots
);

/// <summary>
/// DTO for updating user (admin)
/// </summary>
public sealed record UpdateUserDto(
    string? FullName,
    string? Phone,
    bool? IsActive,
    int? RoleId
);

/// <summary>
/// DTO for filtering users (admin)
/// </summary>
public sealed record UserFilterDto(
    string? SearchTerm,
    string? RoleName,
    bool? IsActive,
    bool? EmailConfirmed,
    int Page = 1,
    int PageSize = 20
);
