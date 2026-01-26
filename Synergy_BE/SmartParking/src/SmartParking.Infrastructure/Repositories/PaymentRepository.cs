using Microsoft.EntityFrameworkCore;
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
}
