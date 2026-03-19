using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Helpers;
using SmartParking.API.Models.Auth;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Auth;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartParking.API.Controllers;

[Route("api/auth")]
public sealed class AuthenticationController : BaseApiController
{
    private readonly IAuthenticationService _authService;
    private readonly IConfiguration _configuration;

    public AuthenticationController(IAuthenticationService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    /// <summary>
    /// STEP 1: Request registration - Sends OTP to email
    /// </summary>
    [HttpPost("register-request")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> RegisterRequest(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var dto = new RegisterRequestDto(
            request.FullName,
            request.Email,
            request.Phone,
            request.Password
        );

        await _authService.RegisterRequestOtpAsync(dto, ct);
        return Ok(ApiResponse.SuccessResponse(Messages.Auth.OtpSentSuccess));
    }

    /// <summary>
    /// STEP 2: Verify OTP and complete registration
    /// </summary>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> VerifyOtp(
        [FromBody] VerifyOtpRequest request,
        CancellationToken ct)
    {
        var dto = new VerifyOtpRequestDto(request.Email, request.OtpCode);
        var response = await _authService.VerifyOtpAndRegisterAsync(dto, ct);
        var result = Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, Messages.Auth.OtpVerifiedSuccess));
        if (Request.Headers["X-Use-Cookies"].FirstOrDefault() == "true")
        {
            AuthCookieHelper.SetAuthCookies(Response, response.AccessToken, response.RefreshToken, _configuration);
        }
        return result;
    }

    /// <summary>
    /// STEP 2.5: Resend OTP if expired or not received
    /// </summary>
    [HttpPost("resend-otp")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ApiResponse>> ResendOtp(
        [FromBody] ResendOtpRequest request,
        CancellationToken ct)
    {
        var dto = new ResendOtpRequestDto(request.Email);
        await _authService.ResendOtpAsync(dto, ct);
        return Ok(ApiResponse.SuccessResponse(Messages.Auth.OtpResentSuccess));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var dto = new LoginRequestDto(request.Email, request.Password);
        var response = await _authService.LoginAsync(dto, ct);
        var result = Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, Messages.Auth.LoginSuccess));
        if (Request.Headers["X-Use-Cookies"].FirstOrDefault() == "true")
        {
            AuthCookieHelper.SetAuthCookies(Response, response.AccessToken, response.RefreshToken, _configuration);
        }
        return result;
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken(
        [FromBody] RefreshTokenRequest? request,
        CancellationToken ct)
    {
        var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(ApiResponse.FailureResponse(Messages.Common.Unauthorized));
        }
        var dto = new RefreshTokenRequestDto(refreshToken);
        var response = await _authService.RefreshTokenAsync(dto, ct);
        var result = Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, Messages.Auth.TokenRefreshed));
        if (Request.Headers["X-Use-Cookies"].FirstOrDefault() == "true")
        {
            AuthCookieHelper.SetAuthCookies(Response, response.AccessToken, response.RefreshToken, _configuration);
        }
        return result;
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> Logout(CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.FailureResponse(Messages.Common.Unauthorized));
        }

        await _authService.LogoutAsync(userId, ct);
        var result = Ok(ApiResponse.SuccessResponse(Messages.Auth.LogoutSuccess));
        if (Request.Headers["X-Use-Cookies"].FirstOrDefault() == "true")
        {
            AuthCookieHelper.ClearAuthCookies(Response);
        }
        return result;
    }

    /// <summary>
    /// Logout when using cookie auth - clears auth cookies (no token required)
    /// </summary>
    [HttpPost("logout-cookies")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public IActionResult LogoutCookies()
    {
        AuthCookieHelper.ClearAuthCookies(Response);
        return Ok(ApiResponse.SuccessResponse(Messages.Auth.LogoutSuccess));
    }

    /// <summary>
    /// Forgot Password - Step 1: Send OTP to email for password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        var dto = new ForgotPasswordRequestDto(request.Email);
        await _authService.ForgotPasswordAsync(dto, ct);
        return Ok(ApiResponse.SuccessResponse(Messages.Auth.PasswordResetOtpSent));
    }

    /// <summary>
    /// Reset Password - Step 2: Verify OTP and set new password
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        var dto = new ResetPasswordRequestDto(
            request.Email,
            request.OtpCode,
            request.NewPassword
        );
        
        await _authService.ResetPasswordAsync(dto, ct);
        return Ok(ApiResponse.SuccessResponse(Messages.Auth.PasswordResetSuccess));
    }
}
