using SmartParking.Application.Common.Models;

namespace SmartParking.Application.DTOs.Owner;

/// <summary>
/// Subscription plan info for upgrading to Owner.
/// </summary>
public sealed record OwnerPlanDto(
    string PlanType,
    decimal MonthlyFee,
    decimal YearlyFee,
    string Description);

/// <summary>
/// Request DTO when a User wants to become an Owner.
/// </summary>
public sealed record CreateOwnerUpgradeRequestDto(
    string ParkingLotName,
    string ParkingLotAddress,
    decimal? Latitude,
    decimal? Longitude,
    string PlanType,
    Guid? PaymentTransactionId);

/// <summary>
/// Lightweight filter for admin listing of owner upgrade requests.
/// </summary>
public sealed record OwnerUpgradeRequestFilterDto(
    string? Status,
    int Page = 1,
    int PageSize = 20);

/// <summary>
/// Response DTO for an owner upgrade request.
/// </summary>
public sealed record OwnerUpgradeRequestResponseDto(
    Guid RequestId,
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    string ParkingLotName,
    string ParkingLotAddress,
    decimal? Latitude,
    decimal? Longitude,
    string PlanType,
    decimal FeeAmount,
    string Status,
    string? RejectReason,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    DateTime? RejectedAt,
    Guid? PaymentTransactionId);

/// <summary>
/// DTO for rejecting an owner upgrade request.
/// </summary>
public sealed record RejectOwnerUpgradeRequestDto(string Reason);

