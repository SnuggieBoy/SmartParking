namespace SmartParking.Application.DTOs.Owner;

public sealed record OwnerBankAccountDto(
    Guid OwnerBankAccountId,
    string BankCode,
    string BankName,
    string AccountNumber,
    string AccountHolderName,
    bool IsDefault,
    bool IsVerified,
    DateTime CreatedAt,
    DateTime? VerifiedAt);

public sealed record CreateOwnerBankAccountDto(
    string BankCode,
    string BankName,
    string AccountNumber,
    string AccountHolderName,
    bool IsDefault);

public sealed record UpdateOwnerBankAccountDto(
    string? BankCode,
    string? BankName,
    string? AccountNumber,
    string? AccountHolderName,
    bool? IsDefault);

