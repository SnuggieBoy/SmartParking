using SmartParking.Application.DTOs.Payment;

namespace SmartParking.Application.Interfaces.Services;

public interface IVnPayService
{
    Task<PaymentResponseDto> CreatePaymentUrlAsync(CreatePaymentRequestDto request, Guid userId, CancellationToken ct = default);
    Task<PaymentResponseDto> CreateOwnerSubscriptionPaymentUrlAsync(CreateOwnerSubscriptionPaymentDto request, Guid userId, CancellationToken ct = default);
    Task<bool> ProcessCallbackAsync(VnPayCallbackDto callback, CancellationToken ct = default);
}
