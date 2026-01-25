using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class UserAuthRepository : IUserAuthRepository
{
    private readonly SmartParkingDBContext _context;

    public UserAuthRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<UserAuth?> GetByUserIdAndProviderAsync(Guid userId, string provider, CancellationToken ct = default)
    {
        return await _context.UserAuths
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.Provider == provider, ct);
    }

    public async Task<UserAuth?> GetByProviderUserIdAsync(string provider, string providerUserId, CancellationToken ct = default)
    {
        return await _context.UserAuths
            .Include(ua => ua.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(ua => ua.Provider == provider && ua.ProviderUserId == providerUserId, ct);
    }

    public async Task<UserAuth> CreateAsync(UserAuth userAuth, CancellationToken ct = default)
    {
        _context.UserAuths.Add(userAuth);
        await _context.SaveChangesAsync(ct);
        return userAuth;
    }

    public async Task UpdateAsync(UserAuth userAuth, CancellationToken ct = default)
    {
        _context.UserAuths.Update(userAuth);
        await _context.SaveChangesAsync(ct);
    }
}
