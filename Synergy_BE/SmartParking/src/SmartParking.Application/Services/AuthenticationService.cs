using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.DTOs.Auth;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAuthRepository _userAuthRepository;
    private readonly IUserTokenRepository _tokenRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IEmailOtpRepository _emailOtpRepository;
    private readonly IEmailService _emailService;
    
    private const int OTP_LENGTH = 6;
    private const int OTP_EXPIRY_MINUTES = 5;
    private const int RESEND_OTP_COOLDOWN_SECONDS = 60;

    public AuthenticationService(
        IUserRepository userRepository,
        IUserAuthRepository userAuthRepository,
        IUserTokenRepository tokenRepository,
        IRoleRepository roleRepository,
        IJwtTokenService jwtService,
        IPasswordHasher passwordHasher,
        IGoogleAuthService googleAuthService,
        IEmailOtpRepository emailOtpRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _userAuthRepository = userAuthRepository;
        _tokenRepository = tokenRepository;
        _roleRepository = roleRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _googleAuthService = googleAuthService;
        _emailOtpRepository = emailOtpRepository;
        _emailService = emailService;
    }

    #region OTP-based Registration Flow

    public async Task RegisterRequestOtpAsync(RegisterRequestDto request, CancellationToken ct = default)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser != null)
        {
            if (existingUser.EmailConfirmed)
            {
                throw new BadRequestException(Messages.Auth.EmailAlreadyExists);
            }
            else
            {
                throw new BadRequestException(Messages.Auth.PendingRegistration);
            }
        }

        // Check if there's a recent OTP request (rate limiting)
        var existingOtp = await _emailOtpRepository.GetLatestUnusedByEmailAsync(
            request.Email, 
            Domain.Constants.OtpType.Registration, 
            ct);
        if (existingOtp != null && existingOtp.CreatedAt.AddSeconds(RESEND_OTP_COOLDOWN_SECONDS) > DateTime.UtcNow)
        {
            throw new BadRequestException(Messages.Auth.TooManyOtpRequests);
        }

        // Invalidate all previous registration OTPs for this email
        await _emailOtpRepository.InvalidateAllByEmailAsync(
            request.Email, 
            Domain.Constants.OtpType.Registration, 
            ct);

        // Generate secure random OTP
        var otpCode = GenerateOtpCode();

        // Hash password for temporary storage
        var hashedPassword = _passwordHasher.HashPassword(request.Password);

        // Create OTP record
        var emailOtp = new EmailOtp
        {
            OtpId = Guid.NewGuid(),
            Email = request.Email,
            OtpCode = otpCode,
            OtpType = Domain.Constants.OtpType.Registration,
            ExpiredAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            TemporaryPasswordHash = hashedPassword,
            TemporaryFullName = request.FullName,
            TemporaryPhone = request.Phone
        };

        await _emailOtpRepository.CreateAsync(emailOtp, ct);

        // Send OTP email
        await _emailService.SendOtpEmailAsync(request.Email, otpCode, request.FullName, ct);
    }

    public async Task<AuthResponseDto> VerifyOtpAndRegisterAsync(VerifyOtpRequestDto request, CancellationToken ct = default)
    {
        // Get latest unused registration OTP for email
        var emailOtp = await _emailOtpRepository.GetLatestUnusedByEmailAsync(
            request.Email, 
            Domain.Constants.OtpType.Registration, 
            ct);
        
        if (emailOtp == null)
        {
            throw new BadRequestException(Messages.Auth.OtpNotFound);
        }

        // Check if OTP is expired
        if (emailOtp.ExpiredAt < DateTime.UtcNow)
        {
            throw new BadRequestException(Messages.Auth.OtpExpired);
        }

        // Verify OTP code
        if (emailOtp.OtpCode != request.OtpCode)
        {
            throw new BadRequestException(Messages.Auth.OtpInvalid);
        }

        // Check if already used (race condition protection)
        if (emailOtp.IsUsed)
        {
            throw new BadRequestException(Messages.Auth.OtpAlreadyUsed);
        }

        // Mark OTP as used
        await _emailOtpRepository.MarkAsUsedAsync(emailOtp.OtpId, ct);

        // Get user role
        var userRole = await _roleRepository.GetByNameAsync(AuthConstants.Roles.User, ct);
        if (userRole == null)
        {
            throw new NotFoundException(Messages.Auth.RoleNotFound);
        }

        // Create user account
        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = emailOtp.TemporaryFullName ?? "User",
            Email = emailOtp.Email,
            Phone = emailOtp.TemporaryPhone ?? string.Empty,
            RoleId = userRole.RoleId,
            IsActive = true,
            EmailConfirmed = true, // Mark email as confirmed
            CreatedAt = DateTime.UtcNow
        };

        user = await _userRepository.CreateAsync(user, ct);

        // Create user auth
        var userAuth = new UserAuth
        {
            AuthId = Guid.NewGuid(),
            UserId = user.UserId,
            Provider = AuthConstants.LocalProvider,
            ProviderUserId = user.Email,
            PasswordHash = emailOtp.TemporaryPasswordHash,
            CreatedAt = DateTime.UtcNow
        };

        await _userAuthRepository.CreateAsync(userAuth, ct);

        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName, ct);

        // Generate auth response
        user.Role = userRole;
        return await GenerateAuthResponse(user, ct);
    }

    public async Task ResendOtpAsync(ResendOtpRequestDto request, CancellationToken ct = default)
    {
        // Check if user already exists with confirmed email
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser != null && existingUser.EmailConfirmed)
        {
            throw new BadRequestException(Messages.Auth.EmailAlreadyExists);
        }

        // Check for recent OTP request (rate limiting)
        var latestOtp = await _emailOtpRepository.GetLatestUnusedByEmailAsync(
            request.Email, 
            Domain.Constants.OtpType.Registration, 
            ct);
        if (latestOtp != null && latestOtp.CreatedAt.AddSeconds(RESEND_OTP_COOLDOWN_SECONDS) > DateTime.UtcNow)
        {
            throw new BadRequestException(Messages.Auth.TooManyOtpRequests);
        }

        // Check if there's any OTP record for this email
        if (latestOtp == null)
        {
            throw new BadRequestException(Messages.Auth.OtpNotFound);
        }

        // Invalidate all previous registration OTPs
        await _emailOtpRepository.InvalidateAllByEmailAsync(
            request.Email, 
            Domain.Constants.OtpType.Registration, 
            ct);

        // Generate new OTP
        var otpCode = GenerateOtpCode();

        // Create new OTP record (reuse stored data from previous OTP)
        var emailOtp = new EmailOtp
        {
            OtpId = Guid.NewGuid(),
            Email = request.Email,
            OtpCode = otpCode,
            OtpType = Domain.Constants.OtpType.Registration,
            ExpiredAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            TemporaryPasswordHash = latestOtp.TemporaryPasswordHash,
            TemporaryFullName = latestOtp.TemporaryFullName,
            TemporaryPhone = latestOtp.TemporaryPhone
        };

        await _emailOtpRepository.CreateAsync(emailOtp, ct);

        // Send new OTP email
        await _emailService.SendOtpEmailAsync(
            request.Email, 
            otpCode, 
            latestOtp.TemporaryFullName ?? "User", 
            ct);
    }

    /// <summary>
    /// Generates a cryptographically secure random 6-digit OTP code
    /// </summary>
    private static string GenerateOtpCode()
    {
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var randomNumber = BitConverter.ToUInt32(bytes, 0);
        // Generate 6-digit number (100000 to 999999)
        var otpCode = (randomNumber % 900000 + 100000).ToString();
        return otpCode;
    }

    #endregion

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var userAuth = await _userAuthRepository.GetByProviderUserIdAsync(AuthConstants.LocalProvider, request.Email, ct);
        if (userAuth == null || userAuth.PasswordHash == null)
        {
            throw new UnauthorizedException(Messages.Auth.InvalidCredentials);
        }

        if (!_passwordHasher.VerifyPassword(request.Password, userAuth.PasswordHash))
        {
            throw new UnauthorizedException(Messages.Auth.InvalidCredentials);
        }

        var user = userAuth.User;
        if (user?.IsActive != true)
        {
            throw new UnauthorizedException(Messages.Auth.AccountInactive);
        }

        // Check if email is confirmed (for local auth only)
        if (!user.EmailConfirmed)
        {
            throw new UnauthorizedException(Messages.Auth.EmailNotVerified);
        }

        return await GenerateAuthResponse(user, ct);
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request, CancellationToken ct = default)
    {
        var googleUser = await _googleAuthService.ValidateGoogleTokenAsync(request.GoogleToken, ct);
        if (googleUser == null)
        {
            throw new UnauthorizedException(Messages.Auth.InvalidGoogleToken);
        }

        var userAuth = await _userAuthRepository.GetByProviderUserIdAsync(AuthConstants.GoogleProvider, googleUser.GoogleUserId, ct);
        
        User user;
        if (userAuth != null)
        {
            user = userAuth.User;
            if (user?.IsActive != true)
            {
                throw new UnauthorizedException(Messages.Auth.AccountInactive);
            }
        }
        else
        {
            var userRole = await _roleRepository.GetByNameAsync(AuthConstants.Roles.User, ct);
            if (userRole == null)
            {
                throw new NotFoundException(Messages.Auth.RoleNotFound);
            }

            user = new User
            {
                UserId = Guid.NewGuid(),
                FullName = googleUser.Name,
                Email = googleUser.Email,
                Phone = string.Empty,
                RoleId = userRole.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user, ct);

            userAuth = new UserAuth
            {
                AuthId = Guid.NewGuid(),
                UserId = user.UserId,
                Provider = AuthConstants.GoogleProvider,
                ProviderUserId = googleUser.GoogleUserId,
                PasswordHash = null,
                CreatedAt = DateTime.UtcNow
            };

            await _userAuthRepository.CreateAsync(userAuth, ct);
            user.Role = userRole;
        }

        return await GenerateAuthResponse(user, ct);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default)
    {
        var userToken = await _tokenRepository.GetByRefreshTokenAsync(request.RefreshToken, ct);
        if (userToken == null || userToken.ExpiryDate < DateTime.UtcNow)
        {
            throw new UnauthorizedException(Messages.Auth.InvalidToken);
        }

        var user = userToken.User;
        if (user?.IsActive != true)
        {
            throw new UnauthorizedException(Messages.Auth.AccountInactive);
        }

        await _tokenRepository.RevokeTokenAsync(userToken.TokenId, ct);

        return await GenerateAuthResponse(user, ct);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        await _tokenRepository.RevokeByUserIdAsync(userId, ct);
    }

    #region Password Reset with OTP

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken ct = default)
    {
        // Check if user exists
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user == null)
        {
            throw new NotFoundException(Messages.Auth.PasswordResetUserNotFound);
        }

        // Check if user has email verified
        if (!user.EmailConfirmed)
        {
            throw new BadRequestException(Messages.Auth.EmailNotVerified);
        }

        // Check for rate limiting (prevent OTP spam)
        var latestOtp = await _emailOtpRepository.GetLatestUnusedByEmailAsync(
            request.Email, 
            Domain.Constants.OtpType.PasswordReset, 
            ct);
            
        if (latestOtp != null && 
            latestOtp.CreatedAt.AddSeconds(RESEND_OTP_COOLDOWN_SECONDS) > DateTime.UtcNow)
        {
            throw new BadRequestException(Messages.Auth.TooManyOtpRequests);
        }

        // Invalidate all previous password reset OTPs for this email
        await _emailOtpRepository.InvalidateAllByEmailAsync(
            request.Email, 
            Domain.Constants.OtpType.PasswordReset, 
            ct);

        // Generate new OTP
        var otpCode = GenerateOtpCode();

        // Create OTP record
        var emailOtp = new EmailOtp
        {
            OtpId = Guid.NewGuid(),
            Email = request.Email,
            OtpCode = otpCode,
            OtpType = Domain.Constants.OtpType.PasswordReset,
            ExpiredAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _emailOtpRepository.CreateAsync(emailOtp, ct);

        // Send password reset OTP email
        await _emailService.SendPasswordResetOtpAsync(request.Email, otpCode, user.FullName, ct);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default)
    {
        // Get latest unused password reset OTP for email
        var emailOtp = await _emailOtpRepository.GetLatestUnusedByEmailAsync(
            request.Email, 
            Domain.Constants.OtpType.PasswordReset, 
            ct);
        
        if (emailOtp == null)
        {
            throw new BadRequestException(Messages.Auth.InvalidPasswordResetOtp);
        }

        // Check if OTP is expired
        if (emailOtp.ExpiredAt < DateTime.UtcNow)
        {
            throw new BadRequestException(Messages.Auth.OtpExpired);
        }

        // Verify OTP code
        if (emailOtp.OtpCode != request.OtpCode)
        {
            throw new BadRequestException(Messages.Auth.InvalidPasswordResetOtp);
        }

        // Check if already used (race condition protection)
        if (emailOtp.IsUsed)
        {
            throw new BadRequestException(Messages.Auth.OtpAlreadyUsed);
        }

        // Mark OTP as used
        await _emailOtpRepository.MarkAsUsedAsync(emailOtp.OtpId, ct);

        // Get user
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user == null)
        {
            throw new NotFoundException(Messages.Auth.UserNotFound);
        }

        // Get user auth for local provider
        var userAuth = await _userAuthRepository.GetByProviderUserIdAsync(
            AuthConstants.LocalProvider, 
            user.Email, 
            ct);

        // If user auth doesn't exist, create it (for Google-only users who want to set password)
        if (userAuth == null)
        {
            userAuth = new UserAuth
            {
                AuthId = Guid.NewGuid(),
                UserId = user.UserId,
                Provider = AuthConstants.LocalProvider,
                ProviderUserId = user.Email,
                PasswordHash = _passwordHasher.HashPassword(request.NewPassword),
                CreatedAt = DateTime.UtcNow
            };
            await _userAuthRepository.CreateAsync(userAuth, ct);
        }
        else
        {
            // Update existing password
            userAuth.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            await _userAuthRepository.UpdateAsync(userAuth, ct);
        }

        // Revoke all existing tokens (force re-login with new password)
        await _tokenRepository.RevokeByUserIdAsync(user.UserId, ct);
    }

    #endregion

    private async Task<AuthResponseDto> GenerateAuthResponse(User user, CancellationToken ct)
    {
        var accessToken = _jwtService.GenerateAccessToken(user.UserId, user.Email, user.Role.RoleName);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var userToken = new UserToken
        {
            TokenId = Guid.NewGuid(),
            UserId = user.UserId,
            RefreshToken = refreshToken,
            ExpiryDate = expiresAt,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _tokenRepository.CreateAsync(userToken, ct);

        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: expiresAt,
            User: new UserInfoDto(
                UserId: user.UserId,
                FullName: user.FullName,
                Email: user.Email,
                Phone: user.Phone,
                Role: user.Role.RoleName,
                IsActive: user.IsActive ?? true
            )
        );
    }
}
