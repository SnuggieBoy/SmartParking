namespace SmartParking.Application.DTOs.Owner;

/// <summary>
/// Owner dashboard summary
/// </summary>
public sealed record OwnerDashboardDto(
    // Parking Lots Summary
    int TotalParkingLots,
    int ActiveParkingLots,
    int PendingApprovalLots,
    int TotalSlots,
    int OccupiedSlots,
    
    // Bookings Summary
    int TotalBookings,
    int ActiveBookings,
    int CompletedBookingsToday,
    
    // Earnings Summary
    decimal TotalEarnings,
    decimal EarningsToday,
    decimal EarningsThisMonth,
    decimal PendingPayout
);

/// <summary>
/// Owner earnings details
/// </summary>
public sealed record OwnerEarningsDto(
    decimal TotalEarnings,
    decimal TotalCommissionPaid,
    decimal NetEarnings,
    decimal PendingPayout,
    decimal LastPayoutAmount,
    DateTime? LastPayoutDate,
    int TotalTransactions
);

/// <summary>
/// Owner earnings history item
/// </summary>
public sealed record OwnerEarningsHistoryDto(
    string Period, // "2026-01" for monthly, "2026-W04" for weekly
    decimal GrossEarnings,
    decimal Commission,
    decimal NetEarnings,
    int TransactionCount,
    DateTime StartDate,
    DateTime EndDate
);

/// <summary>
/// Owner payout record
/// </summary>
public sealed record OwnerPayoutDto(
    Guid PayoutId,
    decimal Amount,
    string Status, // Pending, Processing, Completed, Failed
    string? BankAccountNumber,
    string? BankName,
    string? TransferContent,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    DateTime? CompletedAt,
    string? Note
);

/// <summary>
/// Owner subscription status
/// </summary>
public sealed record OwnerSubscriptionDto(
    bool IsActive,
    string PlanType, // Monthly, Yearly
    decimal PlanPrice,
    DateTime StartDate,
    DateTime ExpiryDate,
    int DaysRemaining,
    bool IsExpiringSoon, // < 7 days
    bool CanRenew
);

/// <summary>
/// Parking lot statistics
/// </summary>
public sealed record ParkingLotStatisticsDto(
    Guid ParkingLotId,
    string ParkingLotName,
    // Capacity
    int TotalSlots,
    int CurrentOccupancy,
    decimal OccupancyRate,
    // Bookings
    int TotalBookings,
    int BookingsToday,
    int BookingsThisWeek,
    int BookingsThisMonth,
    // Revenue
    decimal TotalRevenue,
    decimal RevenueToday,
    decimal RevenueThisWeek,
    decimal RevenueThisMonth,
    // Average
    decimal AverageBookingDurationHours,
    decimal AverageBookingAmount
);

/// <summary>
/// Renew subscription request
/// </summary>
public sealed record RenewSubscriptionDto(
    string PlanType // Monthly, Yearly
);
