using SmartParking.Application.Common.Models;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IUserWalletRepository
{
    Task<UserWallet?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<UserWallet> GetOrCreateAsync(Guid userId, CancellationToken ct = default);
    Task UpdateAsync(UserWallet wallet, CancellationToken ct = default);
    Task<PagedResult<WalletTransaction>> GetTransactionsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task AddTransactionAsync(WalletTransaction transaction, CancellationToken ct = default);
    Task<bool> HasBookingIncomeForBookingAsync(Guid bookingId, CancellationToken ct = default);
    Task<Guid?> GetFirstAdminUserIdAsync(CancellationToken ct = default);
}
