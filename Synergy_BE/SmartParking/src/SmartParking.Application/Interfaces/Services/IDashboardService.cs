using SmartParking.Application.DTOs.Dashboard;

namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service for admin dashboard statistics and activities
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get users summary (total users, hosts, drivers, new users today)
    /// </summary>
    Task<UsersSummaryDto> GetUsersSummaryAsync(CancellationToken ct = default);

    /// <summary>
    /// Get parking lots summary (active/inactive counts)
    /// </summary>
    Task<ParkingLotsSummaryDto> GetParkingLotsSummaryAsync(CancellationToken ct = default);

    /// <summary>
    /// Get revenue and commission statistics for today
    /// </summary>
    Task<RevenueTodayDto> GetRevenueTodayAsync(CancellationToken ct = default);

    /// <summary>
    /// Get bookings summary (active, pending, completed today)
    /// </summary>
    Task<BookingsSummaryDto> GetBookingsSummaryAsync(CancellationToken ct = default);

    /// <summary>
    /// Get recent activities (bookings, payments, user registrations, etc.)
    /// </summary>
    Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int limit = 10, CancellationToken ct = default);
}
