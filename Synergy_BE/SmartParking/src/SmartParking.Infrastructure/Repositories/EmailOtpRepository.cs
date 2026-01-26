using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class EmailOtpRepository : IEmailOtpRepository
{
    private readonly SmartParkingDBContext _context;

    public EmailOtpRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<EmailOtp> CreateAsync(EmailOtp emailOtp, CancellationToken ct = default)
    {
        await _context.EmailOtps.AddAsync(emailOtp, ct);
        await _context.SaveChangesAsync(ct);
        return emailOtp;
    }

    public async Task<EmailOtp?> GetLatestUnusedByEmailAsync(string email, string? otpType = null, CancellationToken ct = default)
    {
        var query = _context.EmailOtps
            .Where(otp => otp.Email == email && !otp.IsUsed && otp.ExpiredAt > DateTime.UtcNow);
        
        if (!string.IsNullOrEmpty(otpType))
        {
            query = query.Where(otp => otp.OtpType == otpType);
        }
        
        return await query
            .OrderByDescending(otp => otp.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task MarkAsUsedAsync(Guid otpId, CancellationToken ct = default)
    {
        var otp = await _context.EmailOtps.FindAsync(new object[] { otpId }, ct);
        if (otp != null)
        {
            otp.IsUsed = true;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task InvalidateAllByEmailAsync(string email, string? otpType = null, CancellationToken ct = default)
    {
        var query = _context.EmailOtps
            .Where(otp => otp.Email == email && !otp.IsUsed);
        
        if (!string.IsNullOrEmpty(otpType))
        {
            query = query.Where(otp => otp.OtpType == otpType);
        }
        
        await query.ExecuteUpdateAsync(s => s.SetProperty(otp => otp.IsUsed, true), ct);
    }

    public async Task<int> CleanupExpiredAsync(DateTime olderThan, CancellationToken ct = default)
    {
        return await _context.EmailOtps
            .Where(otp => otp.ExpiredAt < olderThan)
            .ExecuteDeleteAsync(ct);
    }
}
