namespace SmartParking.Application.DTOs.Auth;

public sealed record RegisterRequestDto(
    string FullName,
    string Email,
    string Phone,
    string Password
);
