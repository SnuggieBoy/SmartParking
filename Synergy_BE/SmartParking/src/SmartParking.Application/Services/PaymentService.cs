using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.DTOs.Payment;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Enums;

namespace SmartParking.Application.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;

    public PaymentService(IPaymentRepository paymentRepository, IBookingRepository bookingRepository)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<PaymentStatusDto> GetPaymentStatusByBookingAsync(
        Guid bookingId,
        Guid userId,
        bool isAdmin,
        CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        if (!isAdmin && booking.UserId != userId)
        {
            throw new ForbiddenException();
        }

        var payment = await _paymentRepository.GetLatestByBookingIdAsync(bookingId, ct);
        if (payment == null)
        {
            throw new NotFoundException(Messages.Payment.TransactionNotFound);
        }

        var paidAt = payment.PaymentStatus == nameof(PaymentStatus.Success)
            ? payment.CreatedAt
            : null;

        return new PaymentStatusDto(
            bookingId,
            payment.Amount,
            payment.PaymentStatus ?? nameof(PaymentStatus.Pending),
            payment.PaymentMethod ?? PaymentConstants.VnPayProvider,
            paidAt
        );
    }
}

