using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.DTOs.Admin;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Services;

public sealed class AdminService : IAdminService
{
    private readonly SmartParkingDBContext _context;
    private readonly CommissionSettings _commissionSettings;
    private readonly OwnerSubscriptionSettings _subscriptionSettings;
    private readonly IWalletService _walletService;

    public AdminService(
        SmartParkingDBContext context,
        IOptions<CommissionSettings> commissionSettings,
        IOptions<OwnerSubscriptionSettings> subscriptionSettings,
        IWalletService walletService)
    {
        _context = context;
        _commissionSettings = commissionSettings.Value;
        _subscriptionSettings = subscriptionSettings.Value;
        _walletService = walletService;
    }

    #region Transactions

    public async Task<PagedResult<AdminActivityDto>> GetActivitiesAsync(
        ActivityFilterDto filter,
        CancellationToken ct = default)
    {
        var activities = new List<AdminActivityDto>();

        // 1. PaymentTransactions
        var paymentQuery = _context.PaymentTransactions
            .Include(p => p.Booking)
                .ThenInclude(b => b!.ParkingLot)
                    .ThenInclude(pl => pl!.Owner)
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
            paymentQuery = paymentQuery.Where(p => p.PaymentStatus == filter.Status);
        if (filter.UserId.HasValue)
            paymentQuery = paymentQuery.Where(p => p.UserId == filter.UserId.Value);
        if (filter.ParkingLotId.HasValue)
            paymentQuery = paymentQuery.Where(p => p.Booking != null && p.Booking.ParkingLotId == filter.ParkingLotId.Value);
        if (filter.FromDate.HasValue)
            paymentQuery = paymentQuery.Where(p => p.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            paymentQuery = paymentQuery.Where(p => p.CreatedAt <= filter.ToDate.Value);
        if (!string.IsNullOrWhiteSpace(filter.ActivityType))
            paymentQuery = paymentQuery.Where(p => p.PaymentType == filter.ActivityType);

        var payments = await paymentQuery.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
        foreach (var p in payments)
        {
            var desc = p.PaymentType switch
            {
                "Booking" => $"User thanh toán {p.Amount:N0} đ cho booking tại {p.Booking?.ParkingLot?.Name ?? "N/A"}",
                "Extension" => $"User gia hạn {p.Amount:N0} đ cho booking tại {p.Booking?.ParkingLot?.Name ?? "N/A"}",
                "Subscription" => $"Phí nâng cấp owner / phí bãi tháng-năm: {p.Amount:N0} đ",
                _ => $"Thanh toán {p.Amount:N0} đ"
            };
            activities.Add(new AdminActivityDto(
                Id: p.PaymentId,
                Source: "Payment",
                ActivityType: p.PaymentType ?? "Unknown",
                UserId: p.UserId,
                UserName: p.User?.FullName ?? "Unknown",
                UserEmail: p.User?.Email,
                Amount: p.Amount,
                Description: desc,
                CreatedAt: p.CreatedAt,
                Status: p.PaymentStatus,
                PaymentMethod: p.PaymentMethod ?? "Unknown",
                TransactionRef: p.VnpTxnRef ?? p.SePayOrderId ?? "N/A",
                ParkingLotName: p.Booking?.ParkingLot?.Name,
                OwnerName: p.Booking?.ParkingLot?.Owner?.FullName,
                BookingId: p.BookingId
            ));
        }

        // 2. WalletTransactions
        var walletQuery = _context.WalletTransactions
            .Include(w => w.User)
            .AsQueryable();

        if (filter.UserId.HasValue)
            walletQuery = walletQuery.Where(w => w.UserId == filter.UserId.Value);
        if (filter.FromDate.HasValue)
            walletQuery = walletQuery.Where(w => w.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            walletQuery = walletQuery.Where(w => w.CreatedAt <= filter.ToDate.Value);
        if (!string.IsNullOrWhiteSpace(filter.ActivityType))
            walletQuery = walletQuery.Where(w => w.Type == filter.ActivityType);

        var wallets = await walletQuery.OrderByDescending(w => w.CreatedAt).ToListAsync(ct);

        foreach (var w in wallets)
        {
            string desc;
            string? parkingLotName = null;
            string? ownerName = null;
            Guid? bookingId = w.BookingId;

            if (w.BookingId.HasValue)
            {
                var booking = await _context.Bookings
                    .Include(b => b!.ParkingLot)
                        .ThenInclude(pl => pl!.Owner)
                    .FirstOrDefaultAsync(b => b.BookingId == w.BookingId.Value, ct);
                parkingLotName = booking?.ParkingLot?.Name;
                ownerName = booking?.ParkingLot?.Owner?.FullName;
            }

            desc = w.Type switch
            {
                "TopUp" => $"User nạp tiền ví: +{w.Amount:N0} đ",
                "BookingPayment" => $"User trả booking bằng ví: -{Math.Abs(w.Amount):N0} đ tại {parkingLotName ?? "N/A"}",
                "ExtensionPayment" => $"User gia hạn bằng ví: -{Math.Abs(w.Amount):N0} đ tại {parkingLotName ?? "N/A"}",
                "BookingIncome" => $"Owner nhận tiền từ booking: +{w.Amount:N0} đ tại {parkingLotName ?? "N/A"}",
                "Refund" => $"User nhận hoàn tiền: +{w.Amount:N0} đ",
                "EarlyCheckoutRefund" => $"User nhận hoàn 70% thời gian chưa dùng: +{w.Amount:N0} đ",
                _ => w.Description ?? $"{w.Type}: {w.Amount:N0} đ"
            };

            activities.Add(new AdminActivityDto(
                Id: w.WalletTransactionId,
                Source: "Wallet",
                ActivityType: w.Type,
                UserId: w.UserId,
                UserName: w.User?.FullName ?? "Unknown",
                UserEmail: w.User?.Email,
                Amount: w.Amount,
                Description: desc,
                CreatedAt: w.CreatedAt,
                Status: "Success",
                PaymentMethod: "Wallet",
                TransactionRef: "W" + w.WalletTransactionId.ToString("N")[..7].ToUpperInvariant(),
                ParkingLotName: parkingLotName,
                OwnerName: ownerName,
                BookingId: bookingId
            ));
        }

        // Merge, sort, paginate
        var sorted = activities.OrderByDescending(a => a.CreatedAt).ToList();
        var totalCount = sorted.Count;
        var items = sorted
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return new PagedResult<AdminActivityDto>(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<PagedResult<TransactionDto>> GetTransactionsAsync(
        TransactionFilterDto filter,
        CancellationToken ct = default)
    {
        var query = _context.PaymentTransactions
            .Include(p => p.Booking)
                .ThenInclude(b => b!.ParkingLot)
                    .ThenInclude(pl => pl!.Owner)
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(p => p.PaymentStatus == filter.Status);
        }

        if (!string.IsNullOrWhiteSpace(filter.PaymentMethod))
        {
            query = query.Where(p => p.PaymentMethod == filter.PaymentMethod);
        }

        if (!string.IsNullOrWhiteSpace(filter.PaymentType) && filter.PaymentType != "All")
        {
            query = query.Where(p => p.PaymentType == filter.PaymentType);
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(p => p.UserId == filter.UserId.Value);
        }

        if (filter.ParkingLotId.HasValue)
        {
            query = query.Where(p => p.Booking != null && p.Booking.ParkingLotId == filter.ParkingLotId.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= filter.ToDate.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(p => new TransactionDto(
            PaymentId: p.PaymentId,
            BookingId: p.BookingId ?? Guid.Empty,
            UserId: p.UserId,
            UserName: p.User?.FullName ?? "Unknown",
            UserEmail: p.User?.Email ?? "",
            ParkingLotName: p.Booking?.ParkingLot?.Name,
            ParkingLotOwnerId: p.Booking?.ParkingLot?.OwnerId,
            OwnerName: p.Booking?.ParkingLot?.Owner?.FullName,
            Amount: p.Amount,
            PaymentMethod: p.PaymentMethod ?? "Unknown",
            PaymentStatus: p.PaymentStatus ?? "Unknown",
            PaymentType: p.PaymentType,
            TransactionRef: p.VnpTxnRef ?? p.SePayOrderId ?? "N/A",
            BankCode: p.VnpBankCode ?? p.SePayBankCode,
            CreatedAt: p.CreatedAt,
            UpdatedAt: p.UpdatedAt,
            RefundAmount: p.RefundAmount,
            RefundReason: p.RefundReason,
            RefundedAt: p.RefundedAt,
            RefundedBy: p.RefundedBy
        )).ToList();

        return new PagedResult<TransactionDto>(dtos, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<TransactionDto> GetTransactionByIdAsync(Guid paymentId, CancellationToken ct = default)
    {
        var p = await _context.PaymentTransactions
            .Include(p => p.Booking)
                .ThenInclude(b => b!.ParkingLot)
                    .ThenInclude(pl => pl!.Owner)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId && !p.IsDeleted, ct);

        if (p == null)
        {
            throw new NotFoundException("Transaction not found");
        }

        return new TransactionDto(
            PaymentId: p.PaymentId,
            BookingId: p.BookingId ?? Guid.Empty,
            UserId: p.UserId,
            UserName: p.User?.FullName ?? "Unknown",
            UserEmail: p.User?.Email ?? "",
            ParkingLotName: p.Booking?.ParkingLot?.Name,
            ParkingLotOwnerId: p.Booking?.ParkingLot?.OwnerId,
            OwnerName: p.Booking?.ParkingLot?.Owner?.FullName,
            Amount: p.Amount,
            PaymentMethod: p.PaymentMethod ?? "Unknown",
            PaymentStatus: p.PaymentStatus ?? "Unknown",
            PaymentType: p.PaymentType,
            TransactionRef: p.VnpTxnRef ?? p.SePayOrderId ?? "N/A",
            BankCode: p.VnpBankCode ?? p.SePayBankCode,
            CreatedAt: p.CreatedAt,
            UpdatedAt: p.UpdatedAt,
            RefundAmount: p.RefundAmount,
            RefundReason: p.RefundReason,
            RefundedAt: p.RefundedAt,
            RefundedBy: p.RefundedBy
        );
    }

    public async Task<TransactionDto> ProcessRefundAsync(
        Guid paymentId,
        ProcessRefundDto request,
        Guid adminId,
        CancellationToken ct = default)
    {
        var payment = await _context.PaymentTransactions
            .Include(p => p.Booking)
                .ThenInclude(b => b!.ParkingLot)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId && !p.IsDeleted, ct);

        if (payment == null)
        {
            throw new NotFoundException("Không tìm thấy giao dịch.");
        }

        if (payment.PaymentStatus != "Success")
        {
            throw new BadRequestException("Chỉ có thể hoàn tiền giao dịch thành công.");
        }

        if (payment.RefundedAt.HasValue)
        {
            throw new BadRequestException("Giao dịch đã được hoàn tiền trước đó.");
        }

        if (request.RefundAmount > payment.Amount)
        {
            throw new BadRequestException("Số tiền hoàn không được vượt quá số tiền gốc.");
        }

        payment.RefundAmount = request.RefundAmount;
        payment.RefundReason = request.Reason;
        payment.RefundedAt = DateTime.UtcNow;
        payment.RefundedBy = adminId;
        payment.PaymentStatus = "Refunded";
        payment.UpdatedAt = DateTime.UtcNow;
        payment.UpdatedBy = adminId;

        if (payment.Booking != null && request.RefundAmount == payment.Amount)
        {
            payment.Booking.Status = "Cancelled";
            payment.Booking.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);

        if (payment.Booking != null)
        {
            var reason = string.IsNullOrWhiteSpace(request.Reason)
                ? "Admin hoàn tiền"
                : $"Admin hoàn tiền: {request.Reason}";

            await _walletService.RefundBookingFullAsync(
                payment.Booking.BookingId,
                payment.UserId,
                request.RefundAmount,
                reason,
                ct);
        }

        return await GetTransactionByIdAsync(paymentId, ct);
    }

    #endregion

    #region Payouts

    public async Task<PagedResult<AdminPayoutDto>> GetPayoutsAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        // Placeholder - would come from OwnerPayouts table when implemented
        return new PagedResult<AdminPayoutDto>(new List<AdminPayoutDto>(), page, pageSize, 0);
    }

    public async Task<AdminPayoutDto> CreatePayoutAsync(
        CreatePayoutDto request,
        Guid adminId,
        CancellationToken ct = default)
    {
        throw new BadRequestException("Payout system is not yet implemented. Please process payouts manually.");
    }

    public async Task<AdminPayoutDto> CompletePayoutAsync(
        Guid payoutId,
        Guid adminId,
        CancellationToken ct = default)
    {
        throw new NotFoundException("Payout not found");
    }

    #endregion

    #region Reports

    public async Task<RevenueReportDto> GetRevenueReportAsync(
        DateTime fromDate,
        DateTime toDate,
        string period,
        CancellationToken ct = default)
    {
        // Admin báo cáo doanh thu: chỉ Subscription (phí nâng cấp + phí bãi tháng/năm)
        var payments = await _context.PaymentTransactions
            .Where(p => !p.IsDeleted && p.PaymentType == "Subscription" && p.CreatedAt >= fromDate && p.CreatedAt <= toDate)
            .ToListAsync(ct);

        var successful = payments.Where(p => p.PaymentStatus == "Success").ToList();
        var failed = payments.Where(p => p.PaymentStatus == "Failed").ToList();
        var refunded = payments.Where(p => p.PaymentStatus == "Refunded").ToList();

        var totalRevenue = successful.Sum(p => p.Amount);
        var commissionRate = _commissionSettings.CommissionRatePercent / 100m;
        var totalCommission = totalRevenue * commissionRate;
        var netOwnerRevenue = totalRevenue - totalCommission;
        var refundedAmount = refunded.Sum(p => p.RefundAmount ?? 0);

        // Group by period
        var breakdown = period.ToLower() switch
        {
            "daily" => successful
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new RevenueBreakdownDto(
                    Period: g.Key.ToString("yyyy-MM-dd"),
                    Revenue: g.Sum(p => p.Amount),
                    Commission: g.Sum(p => p.Amount) * commissionRate,
                    NetRevenue: g.Sum(p => p.Amount) * (1 - commissionRate),
                    TransactionCount: g.Count()
                ))
                .OrderBy(x => x.Period)
                .ToList(),
            "weekly" => successful
                .GroupBy(p => GetWeekNumber(p.CreatedAt))
                .Select(g => new RevenueBreakdownDto(
                    Period: g.Key,
                    Revenue: g.Sum(p => p.Amount),
                    Commission: g.Sum(p => p.Amount) * commissionRate,
                    NetRevenue: g.Sum(p => p.Amount) * (1 - commissionRate),
                    TransactionCount: g.Count()
                ))
                .OrderBy(x => x.Period)
                .ToList(),
            _ => successful
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .Select(g => new RevenueBreakdownDto(
                    Period: $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue: g.Sum(p => p.Amount),
                    Commission: g.Sum(p => p.Amount) * commissionRate,
                    NetRevenue: g.Sum(p => p.Amount) * (1 - commissionRate),
                    TransactionCount: g.Count()
                ))
                .OrderBy(x => x.Period)
                .ToList()
        };

        return new RevenueReportDto(
            FromDate: fromDate,
            ToDate: toDate,
            Period: period,
            TotalRevenue: Math.Round(totalRevenue, 2),
            TotalCommission: Math.Round(totalCommission, 2),
            NetOwnerRevenue: Math.Round(netOwnerRevenue, 2),
            TotalTransactions: payments.Count,
            SuccessfulTransactions: successful.Count,
            FailedTransactions: failed.Count,
            RefundedTransactions: refunded.Count,
            RefundedAmount: Math.Round(refundedAmount, 2),
            Breakdown: breakdown
        );
    }

    public async Task<BookingsReportDto> GetBookingsReportAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken ct = default)
    {
        var bookings = await _context.Bookings
            .Include(b => b.ParkingLot)
                .ThenInclude(pl => pl!.Owner)
            .Where(b => !b.IsDeleted && b.CreatedAt >= fromDate && b.CreatedAt <= toDate)
            .ToListAsync(ct);

        var totalRevenue = bookings.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount);
        var avgAmount = bookings.Any() ? (double)bookings.Average(b => b.TotalAmount) : 0;
        var completedWithDuration = bookings
            .Where(b => b.Status == "Completed" && b.CheckInTime != null && b.CheckOutTime != null)
            .ToList();
        var avgDuration = completedWithDuration.Any()
            ? completedWithDuration.Average(b => (b.CheckOutTime!.Value - b.CheckInTime!.Value).TotalHours)
            : 0;

        // Top parking lots
        var topParkingLots = bookings
            .GroupBy(b => b.ParkingLotId)
            .Select(g =>
            {
                var first = g.First();
                return new TopParkingLotDto(
                    ParkingLotId: g.Key,
                    Name: first.ParkingLot?.Name ?? "Unknown",
                    OwnerName: first.ParkingLot?.Owner?.FullName ?? "Unknown",
                    BookingCount: g.Count(),
                    Revenue: g.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount)
                );
            })
            .OrderByDescending(x => x.BookingCount)
            .Take(10)
            .ToList();

        return new BookingsReportDto(
            FromDate: fromDate,
            ToDate: toDate,
            TotalBookings: bookings.Count,
            PendingBookings: bookings.Count(b => b.Status == "Pending"),
            ConfirmedBookings: bookings.Count(b => b.Status == "Confirmed"),
            InProgressBookings: bookings.Count(b => b.Status == "InProgress"),
            CompletedBookings: bookings.Count(b => b.Status == "Completed"),
            CancelledBookings: bookings.Count(b => b.Status == "Cancelled"),
            TotalRevenue: Math.Round(totalRevenue, 2),
            AverageBookingAmount: Math.Round((decimal)avgAmount, 2),
            AverageBookingDurationHours: Math.Round(avgDuration, 2),
            TopParkingLots: topParkingLots
        );
    }

    #endregion

    #region Settings

    public Task<SystemSettingsDto> GetSettingsAsync(CancellationToken ct = default)
    {
        return Task.FromResult(new SystemSettingsDto(
            CommissionRatePercent: _commissionSettings.CommissionRatePercent,
            MonthlySubscriptionFee: _subscriptionSettings.MonthlyFee,
            YearlySubscriptionFee: _subscriptionSettings.YearlyFee,
            MonthlySubscriptionDays: _subscriptionSettings.MonthlyDurationDays,
            YearlySubscriptionDays: _subscriptionSettings.YearlyDurationDays
        ));
    }

    public Task<SystemSettingsDto> UpdateSettingsAsync(UpdateSystemSettingsDto request, CancellationToken ct = default)
    {
        // Settings are currently stored in appsettings.json
        // To make them dynamic, we would need a SystemSettings table in DB
        throw new BadRequestException("System settings are configured in appsettings.json. Please update the configuration file and restart the application.");
    }

    #endregion

    private static string GetWeekNumber(DateTime date)
    {
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        var week = cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return $"{date.Year}-W{week:D2}";
    }
}
