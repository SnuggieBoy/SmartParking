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
            .Where(p => !p.IsDeleted && p.Status == "PendingApproval")
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

        // Get successful payments today
        var paymentsToday = await _context.PaymentTransactions
            .Where(p => p.PaymentStatus == "Success" &&
                       p.CreatedAt >= today &&
                       p.CreatedAt < tomorrow &&
                       !p.IsDeleted)
            .ToListAsync(ct);

        var totalRevenue = paymentsToday.Sum(p => p.Amount);
        
        // Calculate commission using configured rate
        var commissionRate = _commissionSettings.CommissionRatePercent / 100m;
        var totalCommission = totalRevenue * commissionRate;

        // Count completed bookings today (bookings with successful payment)
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
            .ThenInclude(b => b.ParkingLot)
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

        // Sort by CreatedAt descending and take limit
        return activities
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit);
    }
}
