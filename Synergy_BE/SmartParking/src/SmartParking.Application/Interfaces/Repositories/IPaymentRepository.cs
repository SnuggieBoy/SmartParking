using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<PaymentTransaction?> GetByTxnRefAsync(string txnRef, CancellationToken ct = default);
    Task<PaymentTransaction?> GetLatestByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
    Task<PaymentTransaction> CreateAsync(PaymentTransaction payment, CancellationToken ct = default);
    Task UpdateAsync(PaymentTransaction payment, CancellationToken ct = default);
    Task CreateLogAsync(PaymentLog log, CancellationToken ct = default);
}
