using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.DTOs.Dashboard;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly SmartParkingDBContext _context;
    private readonly IRoleRepository _roleRepository;
    private readonly CommissionSettings _commissionSettings;

    public DashboardService(
        SmartParkingDBContext context, 
        IRoleRepository roleRepository,
        IOptions<CommissionSettings> commissionSettings)
    {
        _context = context;
        _roleRepository = roleRepository;
        _commissionSettings = commissionSettings.Value;
    }

    public async Task<UsersSummaryDto> GetUsersSummaryAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        // Get role IDs
        var userRole = await _roleRepository.GetByNameAsync(AuthConstants.Roles.User, ct);
        var ownerRole = await _roleRepository.GetByNameAsync(AuthConstants.Roles.Owner, ct);

        if (userRole == null || ownerRole == null)
        {
            return new UsersSummaryDto(0, 0, 0, 0);
        }

        var totalUsers = await _context.Users
            .Where(u => u.IsActive == null || u.IsActive.Value)
            .CountAsync(ct);

        var totalHosts = await _context.Users
            .Where(u => u.RoleId == ownerRole.RoleId && (u.IsActive == null || u.IsActive.Value))
            .CountAsync(ct);

        var totalDrivers = await _context.Users
            .Where(u => u.RoleId == userRole.RoleId && (u.IsActive == null || u.IsActive.Value))
            .CountAsync(ct);

        var newUsersToday = await _context.Users
            .Where(u => u.CreatedAt.HasValue && 
                       u.CreatedAt.Value.Date == today &&
                       (u.IsActive == null || u.IsActive.Value))
            .CountAsync(ct);

        return new UsersSummaryDto(
            TotalUsers: totalUsers,
            TotalHosts: totalHosts,
            TotalDrivers: totalDrivers,
            NewUsersToday: newUsersToday
        );
    }

    public async Task<ParkingLotsSummaryDto> GetParkingLotsSummaryAsync(CancellationToken ct = default)
    {
        var totalActive = await _context.ParkingLots
            .Where(p => !p.IsDeleted && p.IsActive)
            .CountAsync(ct);

        var totalInactive = await _context.ParkingLots
            .Where(p => !p.IsDeleted && !p.IsActive)
            .CountAsync(ct);

        var pendingApproval = await _context.ParkingLots
            .Where(p => !p.IsDeleted && p.Status == ParkingLotStatus.PendingApproval)
            .CountAsync(ct);

        return new ParkingLotsSummaryDto(
            TotalActiveParkingLots: totalActive,
            TotalInactiveParkingLots: totalInactive,
            PendingApprovalParkingLots: pendingApproval
        );
    }

    public async Task<RevenueTodayDto> GetRevenueTodayAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // Admin chỉ tính: phí nâng cấp owner + phí bãi gửi xe tháng/năm (PaymentType = Subscription)
        var paymentsToday = await _context.PaymentTransactions
            .Where(p => p.PaymentStatus == "Success" &&
                       p.PaymentType == "Subscription" &&
                       p.CreatedAt >= today &&
                       p.CreatedAt < tomorrow &&
                       !p.IsDeleted)
            .ToListAsync(ct);

        var totalRevenue = paymentsToday.Sum(p => p.Amount);
        var commissionRate = _commissionSettings.CommissionRatePercent / 100m;
        var totalCommission = totalRevenue * commissionRate;

        var completedBookingsToday = await _context.Bookings
            .Where(b => b.Status == "Completed" &&
                       b.UpdatedAt.HasValue &&
                       b.UpdatedAt.Value.Date == today &&
                       !b.IsDeleted)
            .CountAsync(ct);

        return new RevenueTodayDto(
            Date: today.ToString("yyyy-MM-dd"),
            TotalCommission: totalCommission,
            TotalRevenue: totalRevenue,
            TotalCompletedBookings: completedBookingsToday
        );
    }

    public async Task<BookingsSummaryDto> GetBookingsSummaryAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        // Active bookings (Checked in, not checked out yet)
        var activeBookings = await _context.Bookings
            .Where(b => b.Status == "Active" &&
                       b.CheckInTime.HasValue &&
                       !b.CheckOutTime.HasValue &&
                       !b.IsDeleted)
            .CountAsync(ct);

        // Pending bookings (Pending, Confirmed but not checked in)
        var pendingBookings = await _context.Bookings
            .Where(b => (b.Status == "Pending" || b.Status == "Confirmed") &&
                       !b.CheckInTime.HasValue &&
                       !b.IsDeleted)
            .CountAsync(ct);

        // Completed today
        var completedToday = await _context.Bookings
            .Where(b => b.Status == "Completed" &&
                       b.UpdatedAt.HasValue &&
                       b.UpdatedAt.Value.Date == today &&
                       !b.IsDeleted)
            .CountAsync(ct);

        return new BookingsSummaryDto(
            ActiveBookings: activeBookings,
            PendingBookings: pendingBookings,
            CompletedToday: completedToday
        );
    }

    public async Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int limit = 10, CancellationToken ct = default)
    {
        var activities = new List<RecentActivityDto>();

        // Get recent bookings
        var recentBookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.ParkingLot)
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        foreach (var booking in recentBookings)
        {
            activities.Add(new RecentActivityDto(
                Type: "BookingCreated",
                Description: $"Tài xế {booking.User?.FullName ?? "Unknown"} đặt chỗ tại {booking.ParkingLot?.Name ?? "Unknown"}",
                RelatedId: booking.BookingId,
                CreatedAt: booking.CreatedAt,
                UserName: booking.User?.FullName,
                ParkingLotName: booking.ParkingLot?.Name,
                Amount: booking.TotalAmount,
                Status: booking.Status
            ));
        }

        // Get recent successful payments
        var recentPayments = await _context.PaymentTransactions
            .Include(p => p.User)
            .Include(p => p.Booking)
                .ThenInclude(b => b!.ParkingLot)
            .Where(p => p.PaymentStatus == "Success" && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        foreach (var payment in recentPayments)
        {
            activities.Add(new RecentActivityDto(
                Type: "PaymentSucceeded",
                Description: $"Thanh toán thành công {payment.Amount:N0} VNĐ cho booking tại {payment.Booking?.ParkingLot?.Name ?? "Unknown"}",
                RelatedId: payment.PaymentId,
                CreatedAt: payment.CreatedAt,
                UserName: payment.User?.FullName,
                ParkingLotName: payment.Booking?.ParkingLot?.Name,
                Amount: payment.Amount,
                Status: payment.PaymentStatus
            ));
        }

        // Get recent user registrations
        var recentUsers = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.IsActive == null || u.IsActive.Value)
            .OrderByDescending(u => u.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        foreach (var user in recentUsers)
        {
            activities.Add(new RecentActivityDto(
                Type: "UserRegistered",
                Description: $"{user.Role?.RoleName ?? "User"} {user.FullName} vừa đăng ký tài khoản",
                RelatedId: user.UserId,
                CreatedAt: user.CreatedAt ?? DateTime.UtcNow,
                UserName: user.FullName,
                ParkingLotName: null,
                Amount: null,
                Status: null
            ));
        }

        // Get recent wallet transactions (TopUp, BookingIncome, Refund)
        var recentWallets = await _context.WalletTransactions
            .Include(w => w.User)
            .Where(w => w.Type == "TopUp" || w.Type == "BookingIncome" || w.Type == "Refund" || w.Type == "EarlyCheckoutRefund")
            .OrderByDescending(w => w.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        foreach (var w in recentWallets)
        {
            var desc = w.Type switch
            {
                "TopUp" => $"User {w.User?.FullName ?? "Unknown"} nạp tiền ví: +{w.Amount:N0} đ",
                "BookingIncome" => $"Owner {w.User?.FullName ?? "Unknown"} nhận tiền từ booking: +{w.Amount:N0} đ",
                "Refund" => $"User {w.User?.FullName ?? "Unknown"} nhận hoàn tiền: +{w.Amount:N0} đ",
                "EarlyCheckoutRefund" => $"User {w.User?.FullName ?? "Unknown"} nhận hoàn 70% thời gian chưa dùng: +{w.Amount:N0} đ",
                _ => $"{w.Type}: {w.Amount:N0} đ"
            };
            activities.Add(new RecentActivityDto(
                Type: w.Type,
                Description: desc,
                RelatedId: w.WalletTransactionId,
                CreatedAt: w.CreatedAt,
                UserName: w.User?.FullName,
                ParkingLotName: null,
                Amount: w.Amount,
                Status: "Success"
            ));
        }

        // Sort by CreatedAt descending and take limit
        return activities
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit);
    }

    public async Task<IEnumerable<RevenueChartDto>> GetRevenueChartAsync(string period = "week", CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        DateTime fromDate = period.ToLower() switch
        {
            "day" => now.Date,
            "month" => now.AddMonths(-1).Date,
            "year" => now.AddYears(-1).Date,
            _ => now.AddDays(-7).Date // week
        };

        var payments = await _context.PaymentTransactions
            .Where(p => p.PaymentStatus == "Success" && p.PaymentType == "Subscription" &&
                       p.CreatedAt >= fromDate && p.CreatedAt <= now && !p.IsDeleted)
            .ToListAsync(ct);

        var commissionRate = _commissionSettings.CommissionRatePercent / 100m;
        var list = period.ToLower() switch
        {
            "day" => payments
                .GroupBy(p => p.CreatedAt.Hour)
                .OrderBy(g => g.Key)
                .Select(g => new RevenueChartDto($"{g.Key:D2}:00", g.Sum(p => p.Amount) * commissionRate))
                .ToList(),
            "month" => payments
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month, p.CreatedAt.Day })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month).ThenBy(g => g.Key.Day)
                .Take(31)
                .Select(g => new RevenueChartDto($"{g.Key.Day:D2}/{g.Key.Month:D2}", g.Sum(p => p.Amount) * commissionRate))
                .ToList(),
            "year" => payments
                .GroupBy(p => p.CreatedAt.Month)
                .OrderBy(g => g.Key)
                .Select(g => new RevenueChartDto(GetMonthName(g.Key), g.Sum(p => p.Amount) * commissionRate))
                .ToList(),
            _ => payments
                .GroupBy(p => p.CreatedAt.DayOfWeek)
                .OrderBy(g => (int)g.Key == 0 ? 7 : (int)g.Key)
                .Select(g => new RevenueChartDto(GetDayName(g.Key), g.Sum(p => p.Amount) * commissionRate))
                .ToList()
        };
        return list;
    }

    public async Task<IEnumerable<TopParkingLotChartDto>> GetTopParkingLotsAsync(int limit = 10, CancellationToken ct = default)
    {
        var lots = await _context.Bookings
            .Include(b => b.ParkingLot)
            .Where(b => !b.IsDeleted && b.Status == "Completed" && b.ParkingLot != null)
            .GroupBy(b => b.ParkingLotId)
            .Select(g => new
            {
                ParkingLotId = g.Key,
                Revenue = g.Sum(b => b.TotalAmount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .Take(limit)
            .ToListAsync(ct);

        var result = new List<TopParkingLotChartDto>();
        foreach (var item in lots)
        {
            var pl = await _context.ParkingLots
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.ParkingLotId == item.ParkingLotId, ct);
            if (pl == null) continue;
            var validReviews = pl.Reviews?.Where(r => !r.IsDeleted).ToList() ?? [];
            var avgRating = validReviews.Count > 0 ? validReviews.Average(r => r.Rating) : 0;
            var addr = pl.Address;
            result.Add(new TopParkingLotChartDto(pl.ParkingLotId, pl.Name ?? "N/A", addr, item.Revenue, Math.Round(avgRating, 1)));
        }
        return result;
    }

    public async Task<IEnumerable<UserDistributionDto>> GetUserDistributionAsync(CancellationToken ct = default)
    {
        var userRole = await _roleRepository.GetByNameAsync(AuthConstants.Roles.User, ct);
        var ownerRole = await _roleRepository.GetByNameAsync(AuthConstants.Roles.Owner, ct);
        if (userRole == null || ownerRole == null)
            return [new UserDistributionDto("Driver", 0), new UserDistributionDto("Owner", 0)];

        var drivers = await _context.Users
            .Where(u => u.RoleId == userRole.RoleId && (u.IsActive == null || u.IsActive.Value))
            .CountAsync(ct);
        var owners = await _context.Users
            .Where(u => u.RoleId == ownerRole.RoleId && (u.IsActive == null || u.IsActive.Value))
            .CountAsync(ct);
        return [new UserDistributionDto("Driver", drivers), new UserDistributionDto("Owner", owners)];
    }

    public async Task<IEnumerable<RecentReviewDto>> GetRecentReviewsAsync(int limit = 5, CancellationToken ct = default)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.ParkingLot)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        return reviews.Select(r => new RecentReviewDto(
            r.ReviewId,
            r.User?.FullName ?? "Unknown",
            r.Rating,
            r.Comment,
            r.CreatedAt,
            r.User?.FullName?[0].ToString() ?? "?"
        )).ToList();
    }

    private static string GetDayName(DayOfWeek d) => d switch
    {
        DayOfWeek.Monday => "Thứ 2",
        DayOfWeek.Tuesday => "Thứ 3",
        DayOfWeek.Wednesday => "Thứ 4",
        DayOfWeek.Thursday => "Thứ 5",
        DayOfWeek.Friday => "Thứ 6",
        DayOfWeek.Saturday => "Thứ 7",
        _ => "CN"
    };

    private static string GetMonthName(int m) => m switch
    {
        1 => "T1", 2 => "T2", 3 => "T3", 4 => "T4", 5 => "T5", 6 => "T6",
        7 => "T7", 8 => "T8", 9 => "T9", 10 => "T10", 11 => "T11", _ => "T12"
    };
}
