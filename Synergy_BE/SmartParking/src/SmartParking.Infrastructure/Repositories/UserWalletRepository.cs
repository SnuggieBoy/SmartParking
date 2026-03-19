using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class UserWalletRepository : IUserWalletRepository
{
    private readonly SmartParkingDBContext _context;

    public UserWalletRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<UserWallet?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.UserWallets.FindAsync([userId], ct);
    }

    public async Task<UserWallet> GetOrCreateAsync(Guid userId, CancellationToken ct = default)
    {
        var wallet = await GetByUserIdAsync(userId, ct);
        if (wallet != null) return wallet;

        wallet = new UserWallet
        {
            UserId = userId,
            Balance = 0,
            UpdatedAt = DateTime.UtcNow
        };
        _context.UserWallets.Add(wallet);
        await _context.SaveChangesAsync(ct);
        return wallet;
    }

    public async Task UpdateAsync(UserWallet wallet, CancellationToken ct = default)
    {
        _context.UserWallets.Update(wallet);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<WalletTransaction>> GetTransactionsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var query = _context.WalletTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WalletTransaction>(items, page, pageSize, totalCount);
    }

    public async Task AddTransactionAsync(WalletTransaction transaction, CancellationToken ct = default)
    {
        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> HasBookingIncomeForBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await _context.WalletTransactions
            .AnyAsync(w => w.BookingId == bookingId && w.Type == "BookingIncome", ct);
    }

    public async Task<Guid?> GetFirstAdminUserIdAsync(CancellationToken ct = default)
    {
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin", ct);
        if (adminRole == null) return null;
        var admin = await _context.Users.FirstOrDefaultAsync(u => u.RoleId == adminRole.RoleId, ct);
        return admin?.UserId;
    }
}
