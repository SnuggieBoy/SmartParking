namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends OTP verification email to user
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="otpCode">6-digit OTP code</param>
    /// <param name="fullName">User's full name for personalization</param>
    /// <param name="ct">Cancellation token</param>
    Task SendOtpEmailAsync(string email, string otpCode, string fullName, CancellationToken ct = default);
    
    /// <summary>
    /// Sends password reset OTP email
    /// </summary>
    Task SendPasswordResetOtpAsync(string email, string otpCode, string fullName, CancellationToken ct = default);
    
    /// <summary>
    /// Sends welcome email after successful registration
    /// </summary>
    Task SendWelcomeEmailAsync(string email, string fullName, CancellationToken ct = default);
}
