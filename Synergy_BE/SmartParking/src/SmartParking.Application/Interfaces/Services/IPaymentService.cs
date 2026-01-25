using SmartParking.Application.DTOs.Payment;

namespace SmartParking.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentStatusDto> GetPaymentStatusByBookingAsync(Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct = default);
}

