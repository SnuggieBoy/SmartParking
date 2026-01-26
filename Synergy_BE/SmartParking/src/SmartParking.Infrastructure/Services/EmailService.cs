using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartParking.Application.Interfaces.Services;
using System.Net;
using System.Net.Mail;

namespace SmartParking.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string? _smtpHost;
    private readonly int _smtpPort;
    private readonly string? _smtpUser;
    private readonly string? _smtpPassword;
    private readonly string? _fromEmail;
    private readonly string? _fromName;
    private readonly bool _enableSsl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Load SMTP settings from appsettings
        _smtpHost = _configuration["EmailSettings:SmtpHost"];
        _smtpPort = int.TryParse(_configuration["EmailSettings:SmtpPort"], out var port) ? port : 587;
        _smtpUser = _configuration["EmailSettings:SmtpUser"];
        _smtpPassword = _configuration["EmailSettings:SmtpPassword"];
        _fromEmail = _configuration["EmailSettings:FromEmail"];
        _fromName = _configuration["EmailSettings:FromName"] ?? "SmartParking";
        _enableSsl = bool.TryParse(_configuration["EmailSettings:EnableSsl"], out var ssl) ? ssl : true;
    }

    public async Task SendOtpEmailAsync(string email, string otpCode, string fullName, CancellationToken ct = default)
    {
        var subject = "Verify Your SmartParking Account";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .otp-code {{ 
            font-size: 32px; 
            font-weight: bold; 
            color: #4CAF50; 
            text-align: center; 
            padding: 20px;
            letter-spacing: 5px;
            background-color: #ffffff;
            border: 2px dashed #4CAF50;
            margin: 20px 0;
        }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .warning {{ color: #ff5722; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🅿️ SmartParking</h1>
        </div>
        <div class='content'>
            <h2>Hello {fullName},</h2>
            <p>Thank you for registering with SmartParking!</p>
            <p>To complete your registration, please verify your email address using the OTP code below:</p>
            
            <div class='otp-code'>{otpCode}</div>
            
            <p>This code will expire in <span class='warning'>5 minutes</span>.</p>
            <p>If you didn't request this verification, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 SmartParking. All rights reserved.</p>
            <p>This is an automated email, please do not reply.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, subject, body, ct);
    }

    public async Task SendPasswordResetOtpAsync(string email, string otpCode, string fullName, CancellationToken ct = default)
    {
        var subject = "Reset Your SmartParking Password";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ff5722; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .otp-code {{ 
            font-size: 32px; 
            font-weight: bold; 
            color: #ff5722; 
            text-align: center; 
            padding: 20px;
            letter-spacing: 5px;
            background-color: #ffffff;
            border: 2px dashed #ff5722;
            margin: 20px 0;
        }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .warning {{ color: #ff5722; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🅿️ SmartParking</h1>
        </div>
        <div class='content'>
            <h2>Hello {fullName},</h2>
            <p>We received a request to reset your password.</p>
            <p>Use the OTP code below to reset your password:</p>
            
            <div class='otp-code'>{otpCode}</div>
            
            <p>This code will expire in <span class='warning'>5 minutes</span>.</p>
            <p class='warning'>If you didn't request a password reset, please ignore this email and your password will remain unchanged.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 SmartParking. All rights reserved.</p>
            <p>This is an automated email, please do not reply.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, subject, body, ct);
    }

    public async Task SendWelcomeEmailAsync(string email, string fullName, CancellationToken ct = default)
    {
        var subject = "Welcome to SmartParking!";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🅿️ Welcome to SmartParking!</h1>
        </div>
        <div class='content'>
            <h2>Hello {fullName},</h2>
            <p>Your account has been successfully verified!</p>
            <p>You can now:</p>
            <ul>
                <li>🚗 Find parking spots near you</li>
                <li>📅 Book parking in advance</li>
                <li>💳 Pay securely online</li>
                <li>🔔 Get real-time notifications</li>
            </ul>
            <p>Thank you for choosing SmartParking. We're excited to have you on board!</p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 SmartParking. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, subject, body, ct);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        try
        {
            // Check if SMTP is configured
            if (string.IsNullOrWhiteSpace(_smtpHost) || string.IsNullOrWhiteSpace(_smtpUser))
            {
                _logger.LogWarning("Email service not configured. Email would be sent to: {Email}", toEmail);
                _logger.LogInformation("Email Subject: {Subject}", subject);
                _logger.LogInformation("SMTP settings missing. Configure in appsettings.json under 'EmailSettings'");
                return;
            }

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = _enableSsl,
                Credentials = new NetworkCredential(_smtpUser, _smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail ?? _smtpUser ?? "noreply@smartparking.com", _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage, ct);
            
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}. Subject: {Subject}", toEmail, subject);
            // Don't throw - we don't want email failures to break registration flow
            // In production, you might want to queue failed emails for retry
        }
    }
}
