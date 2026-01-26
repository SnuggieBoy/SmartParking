using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Email OTP management
/// </summary>
public interface IEmailOtpRepository
{
    /// <summary>
    /// Creates a new OTP record
    /// </summary>
    Task<EmailOtp> CreateAsync(EmailOtp emailOtp, CancellationToken ct = default);
    
    /// <summary>
    /// Gets the latest unused OTP for an email (optionally filtered by type)
    /// </summary>
    Task<EmailOtp?> GetLatestUnusedByEmailAsync(string email, string? otpType = null, CancellationToken ct = default);
    
    /// <summary>
    /// Marks an OTP as used
    /// </summary>
    Task MarkAsUsedAsync(Guid otpId, CancellationToken ct = default);
    
    /// <summary>
    /// Invalidates all unused OTPs for an email (optionally filtered by type)
    /// </summary>
    Task InvalidateAllByEmailAsync(string email, string? otpType = null, CancellationToken ct = default);
    
    /// <summary>
    /// Cleans up expired OTPs (older than specified time)
    /// </summary>
    Task<int> CleanupExpiredAsync(DateTime olderThan, CancellationToken ct = default);
}
