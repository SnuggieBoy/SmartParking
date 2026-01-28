using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Owner;

namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service for owner dashboard, earnings, and subscription management
/// </summary>
public interface IOwnerDashboardService
{
    /// <summary>
    /// Get owner dashboard summary
    /// </summary>
    Task<OwnerDashboardDto> GetDashboardAsync(Guid ownerId, CancellationToken ct = default);

    /// <summary>
    /// Get owner earnings summary
    /// </summary>
    Task<OwnerEarningsDto> GetEarningsAsync(Guid ownerId, CancellationToken ct = default);

    /// <summary>
    /// Get owner earnings history (monthly breakdown)
    /// </summary>
    Task<IEnumerable<OwnerEarningsHistoryDto>> GetEarningsHistoryAsync(
        Guid ownerId,
        int months = 12,
        CancellationToken ct = default);

    /// <summary>
    /// Get owner payout history
    /// </summary>
    Task<PagedResult<OwnerPayoutDto>> GetPayoutsAsync(
        Guid ownerId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Get owner subscription status
    /// </summary>
    Task<OwnerSubscriptionDto?> GetSubscriptionAsync(Guid ownerId, CancellationToken ct = default);

    /// <summary>
    /// Renew owner subscription
    /// </summary>
    Task<OwnerSubscriptionDto> RenewSubscriptionAsync(
        Guid ownerId,
        RenewSubscriptionDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Get parking lot statistics
    /// </summary>
    Task<ParkingLotStatisticsDto> GetParkingLotStatisticsAsync(
        Guid parkingLotId,
        Guid ownerId,
        CancellationToken ct = default);
}
