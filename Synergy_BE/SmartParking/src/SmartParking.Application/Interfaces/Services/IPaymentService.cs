using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Payment;
using SmartParking.Application.DTOs.User;

namespace SmartParking.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentStatusDto> GetPaymentStatusByBookingAsync(Guid bookingId, Guid userId, bool isAdmin, bool isOwner = false, CancellationToken ct = default);

    /// <summary>
    /// Get payment status by orderId (SePay txn ref). Used for polling after bank transfer.
    /// </summary>
    Task<PaymentStatusByOrderDto?> GetPaymentStatusByOrderIdAsync(string orderId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get payment history for a user
    /// </summary>
    Task<PagedResult<PaymentHistoryDto>> GetPaymentHistoryAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default);
}

