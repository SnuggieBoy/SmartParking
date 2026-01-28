using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IOwnerBankAccountRepository
{
    Task<IReadOnlyList<OwnerBankAccount>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<OwnerBankAccount?> GetByIdAsync(Guid ownerBankAccountId, CancellationToken ct = default);

    Task<OwnerBankAccount> CreateAsync(OwnerBankAccount account, CancellationToken ct = default);

    Task UpdateAsync(OwnerBankAccount account, CancellationToken ct = default);

    Task DeleteAsync(OwnerBankAccount account, CancellationToken ct = default);
}

