using SmartParking.Application.DTOs.Owner;

namespace SmartParking.Application.Interfaces.Services;

public interface IOwnerBankAccountService
{
    Task<IReadOnlyList<OwnerBankAccountDto>> GetForCurrentOwnerAsync(Guid userId, CancellationToken ct = default);

    Task<OwnerBankAccountDto> CreateAsync(Guid userId, CreateOwnerBankAccountDto request, CancellationToken ct = default);

    Task<OwnerBankAccountDto> UpdateAsync(Guid userId, Guid ownerBankAccountId, UpdateOwnerBankAccountDto request, bool isAdmin, CancellationToken ct = default);

    Task DeleteAsync(Guid userId, Guid ownerBankAccountId, bool isAdmin, CancellationToken ct = default);

    Task<IReadOnlyList<OwnerBankAccountDto>> GetForOwnerAsync(Guid ownerId, CancellationToken ct = default);

    Task<OwnerBankAccountDto> VerifyAsync(Guid ownerBankAccountId, Guid adminId, CancellationToken ct = default);
}

