using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Models.Auth;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Auth;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartParking.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthenticationController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var dto = new RegisterRequestDto(
            request.FullName,
            request.Email,
            request.Phone,
            request.Password
        );

        var response = await _authService.RegisterAsync(dto, ct);
        return CreatedAtAction(
            nameof(Register),
            ApiResponse<AuthResponseDto>.SuccessResponse(response, Messages.Auth.RegisterSuccess)
        );
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
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, Messages.Auth.LoginSuccess));
    }

    [HttpPost("google")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> GoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken ct)
    {
        var dto = new GoogleLoginRequestDto(request.GoogleToken);
        var response = await _authService.GoogleLoginAsync(dto, ct);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, Messages.Auth.LoginSuccess));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var dto = new RefreshTokenRequestDto(request.RefreshToken);
        var response = await _authService.RefreshTokenAsync(dto, ct);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, Messages.Auth.TokenRefreshed));
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
        return Ok(ApiResponse.SuccessResponse(Messages.Auth.LogoutSuccess));
    }
}
