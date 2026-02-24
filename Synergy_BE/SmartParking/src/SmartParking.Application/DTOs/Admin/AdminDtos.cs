namespace SmartParking.Application.DTOs.Admin;

/// <summary>
/// Transaction detail for admin view
/// </summary>
public sealed record TransactionDto(
    Guid PaymentId,
    Guid BookingId,
    Guid UserId,
    string UserName,
    string UserEmail,
    string? ParkingLotName,
    Guid? ParkingLotOwnerId,
    string? OwnerName,
    decimal Amount,
    string PaymentMethod,
    string PaymentStatus,
    string? PaymentType,
    string TransactionRef,
    string? BankCode,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    // Refund info
    decimal? RefundAmount,
    string? RefundReason,
    DateTime? RefundedAt,
    Guid? RefundedBy
);

/// <summary>
/// Filter for transaction listing.
/// Admin mặc định chỉ xem PaymentType = Subscription (phí nâng cấp + phí bãi gửi tháng/năm).
/// </summary>
public sealed record TransactionFilterDto(
    string? Status,
    string? PaymentMethod,
    string? PaymentType, // null = Subscription only (admin), "All" = tất cả
    Guid? UserId,
    Guid? ParkingLotId,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20
);

/// <summary>
/// Request to process refund
/// </summary>
public sealed record ProcessRefundDto(
    decimal RefundAmount,
    string Reason
);

/// <summary>
/// Owner payout record for admin management
/// </summary>
public sealed record AdminPayoutDto(
    Guid PayoutId,
    Guid OwnerId,
    string OwnerName,
    string OwnerEmail,
    Guid? BankAccountId,
    string? BankAccountNumber,
    string? BankName,
    decimal Amount,
    string Status, // Pending, Processing, Completed, Failed
    string? TransferContent,
    string? Note,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    DateTime? CompletedAt,
    Guid? ProcessedBy
);

/// <summary>
/// Create payout request
/// </summary>
public sealed record CreatePayoutDto(
    Guid OwnerId,
    Guid BankAccountId,
    decimal Amount,
    string? Note
);

/// <summary>
/// Revenue report
/// </summary>
public sealed record RevenueReportDto(
    DateTime FromDate,
    DateTime ToDate,
    string Period, // Daily, Weekly, Monthly
    decimal TotalRevenue,
    decimal TotalCommission,
    decimal NetOwnerRevenue,
    int TotalTransactions,
    int SuccessfulTransactions,
    int FailedTransactions,
    int RefundedTransactions,
    decimal RefundedAmount,
    // Breakdown by day/week/month
    IEnumerable<RevenueBreakdownDto> Breakdown
);

/// <summary>
/// Revenue breakdown item
/// </summary>
public sealed record RevenueBreakdownDto(
    string Period, // Date or Week or Month
    decimal Revenue,
    decimal Commission,
    decimal NetRevenue,
    int TransactionCount
);

/// <summary>
/// Bookings report
/// </summary>
public sealed record BookingsReportDto(
    DateTime FromDate,
    DateTime ToDate,
    int TotalBookings,
    int PendingBookings,
    int ConfirmedBookings,
    int InProgressBookings,
    int CompletedBookings,
    int CancelledBookings,
    decimal TotalRevenue,
    decimal AverageBookingAmount,
    double AverageBookingDurationHours,
    // Top parking lots
    IEnumerable<TopParkingLotDto> TopParkingLots
);

/// <summary>
/// Top parking lot stats
/// </summary>
public sealed record TopParkingLotDto(
    Guid ParkingLotId,
    string Name,
    string OwnerName,
    int BookingCount,
    decimal Revenue
);

/// <summary>
/// System settings
/// </summary>
public sealed record SystemSettingsDto(
    decimal CommissionRatePercent,
    decimal MonthlySubscriptionFee,
    decimal YearlySubscriptionFee,
    int MonthlySubscriptionDays,
    int YearlySubscriptionDays
);

/// <summary>
/// Update system settings
/// </summary>
public sealed record UpdateSystemSettingsDto(
    decimal? CommissionRatePercent,
    decimal? MonthlySubscriptionFee,
    decimal? YearlySubscriptionFee
);
