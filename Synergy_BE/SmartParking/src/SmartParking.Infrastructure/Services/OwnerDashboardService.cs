using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.DTOs.Owner;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Services;

public sealed class OwnerDashboardService : IOwnerDashboardService
{
    private readonly SmartParkingDBContext _context;
    private readonly CommissionSettings _commissionSettings;
    private readonly OwnerSubscriptionSettings _subscriptionSettings;

    public OwnerDashboardService(
        SmartParkingDBContext context,
        IOptions<CommissionSettings> commissionSettings,
        IOptions<OwnerSubscriptionSettings> subscriptionSettings)
    {
        _context = context;
        _commissionSettings = commissionSettings.Value;
        _subscriptionSettings = subscriptionSettings.Value;
    }

    public async Task<OwnerDashboardDto> GetDashboardAsync(Guid ownerId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Parking Lots
        var parkingLots = await _context.ParkingLots
            .Where(p => p.OwnerId == ownerId && !p.IsDeleted)
            .ToListAsync(ct);

        var totalParkingLots = parkingLots.Count;
        var activeParkingLots = parkingLots.Count(p => p.IsActive && p.Status == ParkingLotStatus.Approved);
        var pendingApprovalLots = parkingLots.Count(p => p.Status == ParkingLotStatus.PendingApproval);
        var totalSlots = parkingLots.Sum(p => p.TotalCapacity);
        var occupiedSlots = parkingLots.Sum(p => p.CurrentOccupancy);

        // Get parking lot IDs for this owner
        var parkingLotIds = parkingLots.Select(p => p.ParkingLotId).ToList();

        // Bookings
        var bookingsQuery = _context.Bookings
            .Where(b => parkingLotIds.Contains(b.ParkingLotId) && !b.IsDeleted);

        var totalBookings = await bookingsQuery.CountAsync(ct);
        var activeBookings = await bookingsQuery
            .Where(b => b.Status == "Confirmed" || b.Status == "InProgress")
            .CountAsync(ct);
        var completedBookingsToday = await bookingsQuery
            .Where(b => b.Status == "Completed" && b.CheckOutTime >= today)
            .CountAsync(ct);

        // Earnings (from completed payments for bookings at owner's parking lots)
        var paymentsQuery = _context.PaymentTransactions
            .Include(p => p.Booking)
            .Where(p => !p.IsDeleted &&
                       p.PaymentStatus == "Success" &&
                       p.Booking != null &&
                       parkingLotIds.Contains(p.Booking.ParkingLotId));

        var totalEarningsGross = await paymentsQuery.SumAsync(p => p.Amount, ct);
        var commissionRate = _commissionSettings.CommissionRatePercent / 100m;
        var totalEarnings = totalEarningsGross * (1 - commissionRate);

        var earningsToday = await paymentsQuery
            .Where(p => p.CreatedAt >= today)
            .SumAsync(p => p.Amount, ct) * (1 - commissionRate);

        var earningsThisMonth = await paymentsQuery
            .Where(p => p.CreatedAt >= monthStart)
            .SumAsync(p => p.Amount, ct) * (1 - commissionRate);

        // Pending payout (for now, just show total earnings - this would come from a Payout table later)
        var pendingPayout = totalEarnings; // Simplified - in real system, subtract already paid out

        return new OwnerDashboardDto(
            TotalParkingLots: totalParkingLots,
            ActiveParkingLots: activeParkingLots,
            PendingApprovalLots: pendingApprovalLots,
            TotalSlots: totalSlots,
            OccupiedSlots: occupiedSlots,
            TotalBookings: totalBookings,
            ActiveBookings: activeBookings,
            CompletedBookingsToday: completedBookingsToday,
            TotalEarnings: Math.Round(totalEarnings, 2),
            EarningsToday: Math.Round(earningsToday, 2),
            EarningsThisMonth: Math.Round(earningsThisMonth, 2),
            PendingPayout: Math.Round(pendingPayout, 2)
        );
    }

    public async Task<OwnerEarningsDto> GetEarningsAsync(Guid ownerId, CancellationToken ct = default)
    {
        var parkingLotIds = await _context.ParkingLots
            .Where(p => p.OwnerId == ownerId && !p.IsDeleted)
            .Select(p => p.ParkingLotId)
            .ToListAsync(ct);

        var paymentsQuery = _context.PaymentTransactions
            .Include(p => p.Booking)
            .Where(p => !p.IsDeleted &&
                       p.PaymentStatus == "Success" &&
                       p.Booking != null &&
                       parkingLotIds.Contains(p.Booking.ParkingLotId));

        var totalGross = await paymentsQuery.SumAsync(p => p.Amount, ct);
        var transactionCount = await paymentsQuery.CountAsync(ct);

        var commissionRate = _commissionSettings.CommissionRatePercent / 100m;
        var totalCommission = totalGross * commissionRate;
        var netEarnings = totalGross - totalCommission;

        // Pending payout - in real system, this would be calculated from Payout records
        var pendingPayout = netEarnings;

        return new OwnerEarningsDto(
            TotalEarnings: Math.Round(totalGross, 2),
            TotalCommissionPaid: Math.Round(totalCommission, 2),
            NetEarnings: Math.Round(netEarnings, 2),
            PendingPayout: Math.Round(pendingPayout, 2),
            LastPayoutAmount: 0, // Will come from Payout table
            LastPayoutDate: null,
            TotalTransactions: transactionCount
        );
    }

    public async Task<IEnumerable<OwnerEarningsHistoryDto>> GetEarningsHistoryAsync(
        Guid ownerId,
        int months = 12,
        CancellationToken ct = default)
    {
        var parkingLotIds = await _context.ParkingLots
            .Where(p => p.OwnerId == ownerId && !p.IsDeleted)
            .Select(p => p.ParkingLotId)
            .ToListAsync(ct);

        var startDate = DateTime.UtcNow.AddMonths(-months).Date;
        var commissionRate = _commissionSettings.CommissionRatePercent / 100m;

        var payments = await _context.PaymentTransactions
            .Include(p => p.Booking)
            .Where(p => !p.IsDeleted &&
                       p.PaymentStatus == "Success" &&
                       p.Booking != null &&
                       parkingLotIds.Contains(p.Booking.ParkingLotId) &&
                       p.CreatedAt >= startDate)
            .ToListAsync(ct);

        // Group by month
        var result = payments
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
            .Select(g =>
            {
                var periodStart = new DateTime(g.Key.Year, g.Key.Month, 1);
                var periodEnd = periodStart.AddMonths(1).AddDays(-1);
                var gross = g.Sum(p => p.Amount);
                var commission = gross * commissionRate;

                return new OwnerEarningsHistoryDto(
                    Period: $"{g.Key.Year}-{g.Key.Month:D2}",
                    GrossEarnings: Math.Round(gross, 2),
                    Commission: Math.Round(commission, 2),
                    NetEarnings: Math.Round(gross - commission, 2),
                    TransactionCount: g.Count(),
                    StartDate: periodStart,
                    EndDate: periodEnd
                );
            })
            .OrderByDescending(x => x.Period)
            .ToList();

        return result;
    }

    public async Task<PagedResult<OwnerPayoutDto>> GetPayoutsAsync(
        Guid ownerId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        // For now, return empty result - will be implemented when Payout table is created
        // This is a placeholder that shows the API structure
        return new PagedResult<OwnerPayoutDto>(
            new List<OwnerPayoutDto>(),
            page,
            pageSize,
            0
        );
    }

    public async Task<OwnerSubscriptionDto?> GetSubscriptionAsync(Guid ownerId, CancellationToken ct = default)
    {
        // Get the latest approved upgrade request for this user
        var request = await _context.Set<Domain.Entities.OwnerUpgradeRequest>()
            .Where(r => r.UserId == ownerId && r.Status == "Approved")
            .OrderByDescending(r => r.ApprovedAt)
            .FirstOrDefaultAsync(ct);

        if (request == null)
        {
            return null;
        }

        // Calculate subscription dates based on plan type
        var startDate = request.ApprovedAt ?? request.CreatedAt;
        var daysInPlan = request.PlanType == "Yearly" 
            ? _subscriptionSettings.YearlyDurationDays 
            : _subscriptionSettings.MonthlyDurationDays;
        var expiryDate = startDate.AddDays(daysInPlan);
        var daysRemaining = Math.Max(0, (expiryDate - DateTime.UtcNow).Days);
        var isActive = DateTime.UtcNow < expiryDate;

        return new OwnerSubscriptionDto(
            IsActive: isActive,
            PlanType: request.PlanType,
            PlanPrice: request.FeeAmount,
            StartDate: startDate,
            ExpiryDate: expiryDate,
            DaysRemaining: daysRemaining,
            IsExpiringSoon: daysRemaining <= 7 && daysRemaining > 0,
            CanRenew: daysRemaining <= 30 // Can renew within 30 days of expiry
        );
    }

    public async Task<OwnerSubscriptionDto> RenewSubscriptionAsync(
        Guid ownerId,
        RenewSubscriptionDto request,
        CancellationToken ct = default)
    {
        // This would create a new payment for subscription renewal
        // For now, throw not implemented - this needs payment integration
        throw new BadRequestException("Subscription renewal requires payment. Please use the payment endpoint to renew your subscription.");
    }

    public async Task<ParkingLotStatisticsDto> GetParkingLotStatisticsAsync(
        Guid parkingLotId,
        Guid ownerId,
        CancellationToken ct = default)
    {
        var parkingLot = await _context.ParkingLots
            .FirstOrDefaultAsync(p => p.ParkingLotId == parkingLotId && !p.IsDeleted, ct);

        if (parkingLot == null)
        {
            throw new NotFoundException("Parking lot not found");
        }

        if (parkingLot.OwnerId != ownerId)
        {
            throw new ForbiddenException();
        }

        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Bookings
        var bookingsQuery = _context.Bookings
            .Where(b => b.ParkingLotId == parkingLotId && !b.IsDeleted);

        var totalBookings = await bookingsQuery.CountAsync(ct);
        var bookingsToday = await bookingsQuery.Where(b => b.CreatedAt >= today).CountAsync(ct);
        var bookingsThisWeek = await bookingsQuery.Where(b => b.CreatedAt >= weekStart).CountAsync(ct);
        var bookingsThisMonth = await bookingsQuery.Where(b => b.CreatedAt >= monthStart).CountAsync(ct);

        // Revenue
        var paymentsQuery = _context.PaymentTransactions
            .Include(p => p.Booking)
            .Where(p => !p.IsDeleted &&
                       p.PaymentStatus == "Success" &&
                       p.Booking != null &&
                       p.Booking.ParkingLotId == parkingLotId);

        var totalRevenue = await paymentsQuery.SumAsync(p => p.Amount, ct);
        var revenueToday = await paymentsQuery.Where(p => p.CreatedAt >= today).SumAsync(p => p.Amount, ct);
        var revenueThisWeek = await paymentsQuery.Where(p => p.CreatedAt >= weekStart).SumAsync(p => p.Amount, ct);
        var revenueThisMonth = await paymentsQuery.Where(p => p.CreatedAt >= monthStart).SumAsync(p => p.Amount, ct);

        // Averages
        var completedBookings = await bookingsQuery
            .Where(b => b.Status == "Completed" && b.CheckInTime != null && b.CheckOutTime != null)
            .ToListAsync(ct);

        var avgDuration = completedBookings.Any()
            ? completedBookings.Average(b => (b.CheckOutTime!.Value - b.CheckInTime!.Value).TotalHours)
            : 0;

        var avgAmount = completedBookings.Any()
            ? (double)completedBookings.Average(b => b.TotalAmount)
            : 0;

        var occupancyRate = parkingLot.TotalCapacity > 0
            ? (decimal)parkingLot.CurrentOccupancy / parkingLot.TotalCapacity * 100
            : 0;

        return new ParkingLotStatisticsDto(
            ParkingLotId: parkingLot.ParkingLotId,
            ParkingLotName: parkingLot.Name ?? "",
            TotalSlots: parkingLot.TotalCapacity,
            CurrentOccupancy: parkingLot.CurrentOccupancy,
            OccupancyRate: Math.Round(occupancyRate, 2),
            TotalBookings: totalBookings,
            BookingsToday: bookingsToday,
            BookingsThisWeek: bookingsThisWeek,
            BookingsThisMonth: bookingsThisMonth,
            TotalRevenue: Math.Round(totalRevenue, 2),
            RevenueToday: Math.Round(revenueToday, 2),
            RevenueThisWeek: Math.Round(revenueThisWeek, 2),
            RevenueThisMonth: Math.Round(revenueThisMonth, 2),
            AverageBookingDurationHours: Math.Round((decimal)avgDuration, 2),
            AverageBookingAmount: Math.Round((decimal)avgAmount, 2)
        );
    }
}
