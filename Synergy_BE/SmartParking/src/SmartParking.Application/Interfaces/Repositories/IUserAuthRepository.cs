using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IUserAuthRepository
{
    Task<UserAuth?> GetByUserIdAndProviderAsync(Guid userId, string provider, CancellationToken ct = default);
    Task<UserAuth?> GetByProviderUserIdAsync(string provider, string providerUserId, CancellationToken ct = default);
    Task<UserAuth> CreateAsync(UserAuth userAuth, CancellationToken ct = default);
    Task UpdateAsync(UserAuth userAuth, CancellationToken ct = default);
}
