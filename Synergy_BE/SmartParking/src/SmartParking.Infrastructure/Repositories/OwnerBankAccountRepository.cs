using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class OwnerBankAccountRepository : IOwnerBankAccountRepository
{
    private readonly SmartParkingDBContext _context;

    public OwnerBankAccountRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<OwnerBankAccount>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.OwnerBankAccounts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<OwnerBankAccount?> GetByIdAsync(Guid ownerBankAccountId, CancellationToken ct = default)
    {
        return await _context.OwnerBankAccounts
            .Include(a => a.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.OwnerBankAccountId == ownerBankAccountId, ct);
    }

    public async Task<OwnerBankAccount> CreateAsync(OwnerBankAccount account, CancellationToken ct = default)
    {
        _context.OwnerBankAccounts.Add(account);
        await _context.SaveChangesAsync(ct);
        return account;
    }

    public async Task UpdateAsync(OwnerBankAccount account, CancellationToken ct = default)
    {
        _context.OwnerBankAccounts.Update(account);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(OwnerBankAccount account, CancellationToken ct = default)
    {
        _context.OwnerBankAccounts.Remove(account);
        await _context.SaveChangesAsync(ct);
    }
}

