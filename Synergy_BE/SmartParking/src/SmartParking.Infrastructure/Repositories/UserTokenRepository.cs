using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class UserTokenRepository : IUserTokenRepository
{
    private readonly SmartParkingDBContext _context;

    public UserTokenRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<UserToken?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return await _context.UserTokens
            .Include(ut => ut.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(ut => ut.RefreshToken == refreshToken && ut.IsRevoked == false, ct);
    }

    public async Task<UserToken> CreateAsync(UserToken token, CancellationToken ct = default)
    {
        _context.UserTokens.Add(token);
        await _context.SaveChangesAsync(ct);
        return token;
    }

    public async Task RevokeByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        await _context.UserTokens
            .Where(ut => ut.UserId == userId && ut.IsRevoked == false)
            .ExecuteUpdateAsync(s => s.SetProperty(ut => ut.IsRevoked, true), ct);
    }

    public async Task RevokeTokenAsync(Guid tokenId, CancellationToken ct = default)
    {
        await _context.UserTokens
            .Where(ut => ut.TokenId == tokenId)
            .ExecuteUpdateAsync(s => s.SetProperty(ut => ut.IsRevoked, true), ct);
    }
}
