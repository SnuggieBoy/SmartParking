using SmartParking.Application.Common.Models;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<PaymentTransaction?> GetByTxnRefAsync(string txnRef, CancellationToken ct = default);
    Task<PaymentTransaction?> GetLatestByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
    Task<PaymentTransaction> CreateAsync(PaymentTransaction payment, CancellationToken ct = default);
    Task UpdateAsync(PaymentTransaction payment, CancellationToken ct = default);
    Task CreateLogAsync(PaymentLog log, CancellationToken ct = default);

    /// <summary>
    /// Get payment history for a user (paginated)
    /// </summary>
    Task<PagedResult<PaymentTransaction>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Get payment by ID
    /// </summary>
    Task<PaymentTransaction?> GetByIdAsync(Guid paymentId, CancellationToken ct = default);

    /// <summary>
    /// Get all payments (admin - paginated with filters)
    /// </summary>
    Task<PagedResult<PaymentTransaction>> GetAllAsync(
        string? status,
        string? paymentMethod,
        Guid? userId,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
