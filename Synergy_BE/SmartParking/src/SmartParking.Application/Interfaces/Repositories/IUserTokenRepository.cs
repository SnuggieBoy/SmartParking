using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IUserTokenRepository
{
    Task<UserToken?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<UserToken> CreateAsync(UserToken token, CancellationToken ct = default);
    Task RevokeByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task RevokeTokenAsync(Guid tokenId, CancellationToken ct = default);
}
