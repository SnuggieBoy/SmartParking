namespace SmartParking.Application.DTOs.Dashboard;

/// <summary>
/// Summary of users statistics
/// </summary>
public sealed record UsersSummaryDto(
    int TotalUsers,
    int TotalHosts,
    int TotalDrivers,
    int NewUsersToday
);

/// <summary>
/// Summary of parking lots statistics
/// </summary>
public sealed record ParkingLotsSummaryDto(
    int TotalActiveParkingLots,
    int TotalInactiveParkingLots,
    int PendingApprovalParkingLots
);

/// <summary>
/// Revenue and commission statistics for today
/// </summary>
public sealed record RevenueTodayDto(
    string Date,
    decimal TotalCommission,
    decimal TotalRevenue,
    int TotalCompletedBookings
);

/// <summary>
/// Summary of bookings statistics
/// </summary>
public sealed record BookingsSummaryDto(
    int ActiveBookings,
    int PendingBookings,
    int CompletedToday
);

/// <summary>
/// Recent activity item
/// </summary>
public sealed record RecentActivityDto(
    string Type,
    string Description,
    Guid? RelatedId,
    DateTime CreatedAt,
    string? UserName,
    string? ParkingLotName,
    decimal? Amount,
    string? Status
);

/// <summary>
/// Revenue chart data point
/// </summary>
public sealed record RevenueChartDto(string Name, decimal Revenue);

/// <summary>
/// Top parking lot for dashboard
/// </summary>
public sealed record TopParkingLotChartDto(Guid Id, string Name, string? Address, decimal Revenue, double Rating);

/// <summary>
/// User distribution for pie chart
/// </summary>
public sealed record UserDistributionDto(string Name, int Value);

/// <summary>
/// Recent review for dashboard
/// </summary>
public sealed record RecentReviewDto(Guid Id, string User, int Rating, string? Comment, DateTime Date, string? Avatar);
