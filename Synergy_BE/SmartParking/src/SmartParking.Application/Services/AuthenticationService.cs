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

    public AuthenticationService(
        IUserRepository userRepository,
        IUserAuthRepository userAuthRepository,
        IUserTokenRepository tokenRepository,
        IRoleRepository roleRepository,
        IJwtTokenService jwtService,
        IPasswordHasher passwordHasher,
        IGoogleAuthService googleAuthService)
    {
        _userRepository = userRepository;
        _userAuthRepository = userAuthRepository;
        _tokenRepository = tokenRepository;
        _roleRepository = roleRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _googleAuthService = googleAuthService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser != null)
        {
            throw new BadRequestException(Messages.Auth.EmailAlreadyExists);
        }

        var userRole = await _roleRepository.GetByNameAsync(AuthConstants.Roles.User, ct);
        if (userRole == null)
        {
            throw new NotFoundException(Messages.Auth.RoleNotFound);
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            RoleId = userRole.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user = await _userRepository.CreateAsync(user, ct);

        var userAuth = new UserAuth
        {
            AuthId = Guid.NewGuid(),
            UserId = user.UserId,
            Provider = AuthConstants.LocalProvider,
            ProviderUserId = user.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userAuthRepository.CreateAsync(userAuth, ct);

        user.Role = userRole;
        return await GenerateAuthResponse(user, ct);
    }

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
