using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IExtensionRequestRepository
{
    Task<ExtensionRequest> CreateAsync(ExtensionRequest request, CancellationToken ct = default);
    Task<ExtensionRequest?> GetByIdAsync(Guid extensionRequestId, CancellationToken ct = default);
    Task<IEnumerable<ExtensionRequest>> GetPendingByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task UpdateAsync(ExtensionRequest request, CancellationToken ct = default);
}
