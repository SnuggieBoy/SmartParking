using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Notification;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;
using System.Text.Json;

namespace SmartParking.Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly SmartParkingDBContext _context;

    public NotificationService(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(
        Guid userId,
        bool? isRead,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Notifications
            .Where(n => n.UserId == userId || n.UserId == null) // User's + broadcasts
            .AsQueryable();

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<NotificationDto>(dtos, page, pageSize, totalCount);
    }

    public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(Guid userId, CancellationToken ct = default)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId || n.UserId == null)
            .ToListAsync(ct);

        var unreadCount = notifications.Count(n => !n.IsRead);
        var recentNotifications = notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(MapToDto)
            .ToList();

        return new NotificationSummaryDto(
            TotalCount: notifications.Count,
            UnreadCount: unreadCount,
            RecentNotifications: recentNotifications
        );
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && 
                                      (n.UserId == userId || n.UserId == null), ct);

        if (notification == null)
        {
            throw new NotFoundException("Notification not found");
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        var notifications = await _context.Notifications
            .Where(n => (n.UserId == userId || n.UserId == null) && !n.IsRead)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<NotificationDto> SendNotificationAsync(
        SendNotificationDto request,
        Guid? senderId,
        CancellationToken ct = default)
    {
        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            Data = request.Data,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = senderId
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);

        return MapToDto(notification);
    }

    public async Task<int> BroadcastNotificationAsync(
        BroadcastNotificationDto request,
        Guid adminId,
        CancellationToken ct = default)
    {
        // Get all active users
        var userIds = await _context.Users
            .Where(u => u.IsActive == true)
            .Select(u => u.UserId)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var notifications = userIds.Select(userId => new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            Data = request.Data,
            IsRead = false,
            CreatedAt = now,
            CreatedBy = adminId
        }).ToList();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(ct);

        return notifications.Count;
    }

    public async Task CreateBookingNotificationAsync(
        Guid userId,
        string title,
        string message,
        Guid bookingId,
        CancellationToken ct = default)
    {
        var data = JsonSerializer.Serialize(new { bookingId });
        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = "Booking",
            Data = data,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);
    }

    public async Task CreatePaymentNotificationAsync(
        Guid userId,
        string title,
        string message,
        Guid paymentId,
        CancellationToken ct = default)
    {
        var data = JsonSerializer.Serialize(new { paymentId });
        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = "Payment",
            Data = data,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);
    }

    private static NotificationDto MapToDto(Notification n)
    {
        return new NotificationDto(
            NotificationId: n.NotificationId,
            Title: n.Title,
            Message: n.Message,
            Type: n.Type,
            Data: n.Data,
            IsRead: n.IsRead,
            ReadAt: n.ReadAt,
            CreatedAt: n.CreatedAt
        );
    }
}
