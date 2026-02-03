using Microsoft.Extensions.Options;
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.DTOs.Payment;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;
using SmartParking.Domain.Enums;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace SmartParking.Infrastructure.Services;

public sealed class VnPayService : IVnPayService
{
    private readonly VnPaySettings _settings;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IOwnerUpgradeRequestRepository _ownerUpgradeRequestRepository;

    public VnPayService(
        IOptions<VnPaySettings> settings,
        IPaymentRepository paymentRepository,
        IBookingRepository bookingRepository,
        IOwnerUpgradeRequestRepository ownerUpgradeRequestRepository)
    {
        _settings = settings.Value;
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _ownerUpgradeRequestRepository = ownerUpgradeRequestRepository;
    }

    public async Task<PaymentResponseDto> CreatePaymentUrlAsync(
        CreatePaymentRequestDto request,
        Guid userId,
        CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        // SECURITY: Validate booking ownership
        if (booking.UserId != userId)
        {
            throw new ForbiddenException(Messages.Common.Forbidden);
        }

        var txnRef = $"PAY{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

        var payment = new PaymentTransaction
        {
            PaymentId = Guid.NewGuid(),
            BookingId = request.BookingId,
            UserId = userId,
            Amount = request.Amount,
            PaymentMethod = PaymentConstants.VnPayProvider,
            PaymentStatus = nameof(PaymentStatus.Pending),
            PaymentType = "Booking",
            VnpTxnRef = txnRef,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.CreateAsync(payment, ct);

        return await GenerateVnPayUrlAsync(txnRef, request.Amount, request.Description ?? "Parking Booking Payment");
    }

    public async Task<PaymentResponseDto> CreateOwnerSubscriptionPaymentUrlAsync(
        CreateOwnerSubscriptionPaymentDto request,
        Guid userId,
        CancellationToken ct = default)
    {
        var upgradeRequest = await _ownerUpgradeRequestRepository.GetByIdAsync(request.OwnerUpgradeRequestId, ct);
        if (upgradeRequest == null)
        {
            throw new NotFoundException("Owner upgrade request not found.");
        }

        if (upgradeRequest.UserId != userId)
        {
            throw new ForbiddenException(Messages.Common.Forbidden);
        }

        var txnRef = $"SUB{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

        var payment = new PaymentTransaction
        {
            PaymentId = Guid.NewGuid(),
            OwnerUpgradeRequestId = request.OwnerUpgradeRequestId,
            UserId = userId,
            Amount = upgradeRequest.FeeAmount,
            PaymentMethod = PaymentConstants.VnPayProvider,
            PaymentStatus = nameof(PaymentStatus.Pending),
            PaymentType = "Subscription",
            VnpTxnRef = txnRef,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.CreateAsync(payment, ct);

        return await GenerateVnPayUrlAsync(txnRef, upgradeRequest.FeeAmount, request.Description ?? $"Owner Subscription ({upgradeRequest.PlanType})");
    }

    private Task<PaymentResponseDto> GenerateVnPayUrlAsync(string txnRef, decimal amount, string description)
    {
        var vnpParams = new Dictionary<string, string>
        {
            { "vnp_Version", _settings.Version },
            { "vnp_Command", _settings.Command },
            { "vnp_TmnCode", _settings.TmnCode },
            { "vnp_Amount", ((long)(amount * 100)).ToString() },
            { "vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", _settings.CurrencyCode },
            { "vnp_IpAddr", "127.0.0.1" },
            { "vnp_Locale", _settings.Locale },
            { "vnp_OrderInfo", description },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", _settings.ReturnUrl },
            { "vnp_TxnRef", txnRef }
        };

        var sortedParams = vnpParams.OrderBy(x => x.Key).ToList();
        var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value)}"));
        var secureHash = HmacSHA512(_settings.HashSecret, queryString);

        var paymentUrl = $"{_settings.PaymentUrl}?{queryString}&vnp_SecureHash={secureHash}";

        return Task.FromResult(new PaymentResponseDto(paymentUrl, txnRef));
    }

    /// <summary>
    /// SECURITY CRITICAL: Processes VNPay payment callback.
    /// </summary>
    public async Task<bool> ProcessCallbackAsync(VnPayCallbackDto callback, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByTxnRefAsync(callback.vnp_TxnRef, ct);
        if (payment == null)
        {
            return false;
        }

        if (payment.PaymentStatus != nameof(PaymentStatus.Pending))
        {
            return payment.PaymentStatus == nameof(PaymentStatus.Success);
        }

        var vnpParams = new Dictionary<string, string>
        {
            { "vnp_TmnCode", callback.vnp_TmnCode },
            { "vnp_Amount", callback.vnp_Amount },
            { "vnp_BankCode", callback.vnp_BankCode },
            { "vnp_BankTranNo", callback.vnp_BankTranNo },
            { "vnp_CardType", callback.vnp_CardType },
            { "vnp_PayDate", callback.vnp_PayDate },
            { "vnp_OrderInfo", callback.vnp_OrderInfo },
            { "vnp_TransactionNo", callback.vnp_TransactionNo },
            { "vnp_ResponseCode", callback.vnp_ResponseCode },
            { "vnp_TransactionStatus", callback.vnp_TransactionStatus },
            { "vnp_TxnRef", callback.vnp_TxnRef },
            { "vnp_SecureHashType", callback.vnp_SecureHashType }
        };

        var sortedParams = vnpParams.OrderBy(x => x.Key).ToList();
        var signData = string.Join("&", sortedParams.Select(x => $"{x.Key}={x.Value}"));
        var secureHash = HmacSHA512(_settings.HashSecret, signData);

        if (!secureHash.Equals(callback.vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var isSuccess = callback.vnp_ResponseCode == PaymentConstants.VnPayResponseCodes.Success;
        payment.VnpTransactionNo = callback.vnp_TransactionNo;
        payment.VnpResponseCode = callback.vnp_ResponseCode;
        payment.PaymentStatus = isSuccess ? nameof(PaymentStatus.Success) : nameof(PaymentStatus.Failed);
        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, ct);

        // Update target entity status
        if (isSuccess)
        {
            if (payment.PaymentType == "Booking" && payment.BookingId.HasValue)
            {
                var booking = await _bookingRepository.GetByIdAsync(payment.BookingId.Value, includeDeleted: false, ct);
                if (booking != null && booking.Status == nameof(BookingStatus.Pending))
                {
                    booking.Status = nameof(BookingStatus.Confirmed);
                    await _bookingRepository.UpdateAsync(booking, ct);
                }
            }
            else if (payment.PaymentType == "Subscription" && payment.OwnerUpgradeRequestId.HasValue)
            {
                var upgradeRequest = await _ownerUpgradeRequestRepository.GetByIdAsync(payment.OwnerUpgradeRequestId.Value, ct);
                if (upgradeRequest != null && upgradeRequest.Status == "Pending")
                {
                    upgradeRequest.Status = "PendingApproval"; // Move to approval stage after payment
                    await _ownerUpgradeRequestRepository.UpdateAsync(upgradeRequest, ct);
                }
            }
        }

        return isSuccess;
    }



    private static string HmacSHA512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA512(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
