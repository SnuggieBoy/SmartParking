using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class OwnerUpgradeRequestRepository : IOwnerUpgradeRequestRepository
{
    private readonly SmartParkingDBContext _context;

    public OwnerUpgradeRequestRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<OwnerUpgradeRequest?> GetByIdAsync(Guid requestId, CancellationToken ct = default)
    {
        return await _context.OwnerUpgradeRequests
            .Include(r => r.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RequestId == requestId, ct);
    }

    public async Task<OwnerUpgradeRequest?> GetLatestByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.OwnerUpgradeRequests
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<OwnerUpgradeRequest>> GetAllAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.OwnerUpgradeRequests
            .Include(r => r.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return new PagedResult<OwnerUpgradeRequest>(items, page, pageSize, totalCount);
    }

    public async Task<OwnerUpgradeRequest> CreateAsync(OwnerUpgradeRequest request, CancellationToken ct = default)
    {
        _context.OwnerUpgradeRequests.Add(request);
        await _context.SaveChangesAsync(ct);
        return request;
    }

    public async Task UpdateAsync(OwnerUpgradeRequest request, CancellationToken ct = default)
    {
        _context.OwnerUpgradeRequests.Update(request);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdatePaymentAsync(Guid requestId, Guid paymentTransactionId, CancellationToken ct = default)
    {
        var entity = await _context.OwnerUpgradeRequests.FindAsync([requestId], ct);
        if (entity != null)
        {
            entity.PaymentTransactionId = paymentTransactionId;
            entity.Status = "PendingApproval"; // Đã thanh toán, chờ admin duyệt
            await _context.SaveChangesAsync(ct);
        }
    }
}

