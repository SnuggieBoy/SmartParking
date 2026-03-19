using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Services;

public sealed class DevicePushTokenService : IDevicePushTokenService
{
    private readonly SmartParkingDBContext _context;

    public DevicePushTokenService(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task RegisterTokenAsync(Guid userId, string expoPushToken, string? platform = null, CancellationToken ct = default)
    {
        var token = expoPushToken?.Trim();
        if (string.IsNullOrEmpty(token)) return;

        var now = DateTime.UtcNow;
        var existing = await _context.DevicePushTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.ExpoPushToken == token, ct);

        if (existing != null)
        {
            existing.LastUsedAt = now;
            existing.Platform = platform ?? existing.Platform;
            existing.IsActive = true;
            await _context.SaveChangesAsync(ct);
            return;
        }

        _context.DevicePushTokens.Add(new DevicePushToken
        {
            DevicePushTokenId = Guid.NewGuid(),
            UserId = userId,
            ExpoPushToken = token,
            Platform = platform,
            CreatedAt = now,
            LastUsedAt = now,
            IsActive = true
        });
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetActiveTokensForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.DevicePushTokens
            .Where(t => t.UserId == userId && t.IsActive)
            .Select(t => t.ExpoPushToken)
            .ToListAsync(ct);
    }
}
