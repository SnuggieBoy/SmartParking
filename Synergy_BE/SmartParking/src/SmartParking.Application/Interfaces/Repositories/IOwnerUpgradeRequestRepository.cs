using SmartParking.Application.Common.Models;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IOwnerUpgradeRequestRepository
{
    Task<OwnerUpgradeRequest?> GetByIdAsync(Guid requestId, CancellationToken ct = default);

    Task<OwnerUpgradeRequest?> GetLatestByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<PagedResult<OwnerUpgradeRequest>> GetAllAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<OwnerUpgradeRequest> CreateAsync(OwnerUpgradeRequest request, CancellationToken ct = default);

    Task UpdateAsync(OwnerUpgradeRequest request, CancellationToken ct = default);
    Task UpdatePaymentAsync(Guid requestId, Guid paymentTransactionId, CancellationToken ct = default);
}

