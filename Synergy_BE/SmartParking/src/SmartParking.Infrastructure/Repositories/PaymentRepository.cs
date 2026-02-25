using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly SmartParkingDBContext _context;

    public PaymentRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<PaymentTransaction?> GetByTxnRefAsync(string txnRef, CancellationToken ct = default)
    {
        return await _context.PaymentTransactions
            .Where(p => p.VnpTxnRef == txnRef && !p.IsDeleted)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PaymentTransaction?> GetLatestByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await _context.PaymentTransactions
            .Where(p => p.BookingId == bookingId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<decimal> GetTotalPaidForBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await _context.PaymentTransactions
            .Where(p => p.BookingId == bookingId && !p.IsDeleted
                && p.PaymentStatus == "Success"
                && (p.PaymentType == "Booking" || p.PaymentType == "Extension"))
            .SumAsync(p => p.Amount, ct);
    }

    public async Task<PaymentTransaction> CreateAsync(PaymentTransaction payment, CancellationToken ct = default)
    {
        _context.PaymentTransactions.Add(payment);
        await _context.SaveChangesAsync(ct);
        return payment;
    }

    public async Task UpdateAsync(PaymentTransaction payment, CancellationToken ct = default)
    {
        _context.PaymentTransactions.Update(payment);
        await _context.SaveChangesAsync(ct);
    }

    public async Task CreateLogAsync(PaymentLog log, CancellationToken ct = default)
    {
        _context.PaymentLogs.Add(log);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<PaymentTransaction>> GetByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.PaymentTransactions
            .Include(p => p.Booking)
                .ThenInclude(b => b!.ParkingLot)
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PaymentTransaction>(items, page, pageSize, totalCount);
    }

    public async Task<PaymentTransaction?> GetByIdAsync(Guid paymentId, CancellationToken ct = default)
    {
        return await _context.PaymentTransactions
            .Include(p => p.Booking)
                .ThenInclude(b => b!.ParkingLot)
            .Include(p => p.User)
            .Where(p => p.PaymentId == paymentId && !p.IsDeleted)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<PaymentTransaction>> GetAllAsync(
        string? status,
        string? paymentMethod,
        Guid? userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _context.PaymentTransactions
            .Include(p => p.Booking)
                .ThenInclude(b => b!.ParkingLot)
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.PaymentStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(paymentMethod))
        {
            query = query.Where(p => p.PaymentMethod == paymentMethod);
        }

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PaymentTransaction>(items, page, pageSize, totalCount);
    }
}
