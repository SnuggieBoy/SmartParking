#nullable enable
using System;

namespace SmartParking.Domain.Entities;

/// <summary>
/// Entity for storing Email OTP verification codes
/// Used for registration verification before creating User account
/// </summary>
public partial class EmailOtp
{
    public Guid OtpId { get; set; }
    
    public string Email { get; set; } = null!;
    
    public string OtpCode { get; set; } = null!;
    
    public DateTime ExpiredAt { get; set; }
    
    public bool IsUsed { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Stores hashed password temporarily until OTP is verified
    /// Will be used to create User after successful verification
    /// </summary>
    public string? TemporaryPasswordHash { get; set; }
    
    /// <summary>
    /// Stores full name temporarily until OTP is verified
    /// </summary>
    public string? TemporaryFullName { get; set; }
    
    /// <summary>
    /// Stores phone temporarily until OTP is verified
    /// </summary>
    public string? TemporaryPhone { get; set; }
}
