using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.DTOs.Notification;
using SmartParking.Application.DTOs.Payment;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;
using SmartParking.Domain.Enums;
using System.Text;
using System.Text.Json;

namespace SmartParking.Infrastructure.Services;

public sealed class SePayService : ISePayService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IOwnerUpgradeRequestRepository _ownerUpgradeRequestRepository;
    private readonly IWalletService _walletService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SePayService> _logger;
    private readonly SePaySettings _settings;

    public SePayService(
        IPaymentRepository paymentRepository,
        IBookingRepository bookingRepository,
        IOwnerUpgradeRequestRepository ownerUpgradeRequestRepository,
        IWalletService walletService,
        INotificationService notificationService,
        IConfiguration configuration,
        ILogger<SePayService> logger)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _ownerUpgradeRequestRepository = ownerUpgradeRequestRepository;
        _walletService = walletService;
        _notificationService = notificationService;
        _logger = logger;
        _settings = configuration.GetSection("SePay").Get<SePaySettings>() 
            ?? throw new InvalidOperationException("SePay configuration is missing");

        // SECURITY: Override with environment variables if present
        _settings.ApiKey = configuration["SePay:ApiKey"] 
            ?? Environment.GetEnvironmentVariable("SEPAY_API_KEY") 
            ?? _settings.ApiKey;

        _settings.WebhookSecret = configuration["SePay:WebhookSecret"] 
            ?? Environment.GetEnvironmentVariable("SEPAY_WEBHOOK_SECRET") 
            ?? _settings.WebhookSecret;

        // Validate critical settings
        ValidateSettings();
    }

    public async Task<SePayPaymentResponseDto> CreatePaymentAsync(
        CreateSePayPaymentDto request,
        Guid userId,
        CancellationToken ct = default)
    {
        // Validate booking exists and belongs to user
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        if (booking.UserId != userId)
        {
            throw new ForbiddenException("You can only create payments for your own bookings");
        }

        // Check if booking is in valid status for payment
        if (booking.Status != nameof(BookingStatus.Pending))
        {
            throw new BadRequestException("Booking is not in pending status");
        }

        // Generate unique order ID
        var orderId = GenerateOrderId();
        var transferContent = GenerateTransferContent(orderId);

        // Create payment transaction record
        var payment = new PaymentTransaction
        {
            PaymentId = Guid.NewGuid(),
            BookingId = request.BookingId,
            UserId = userId,
            Amount = request.Amount,
            PaymentMethod = PaymentConstants.SePayProvider,
            PaymentStatus = nameof(PaymentStatus.Pending),
            PaymentType = "Booking",
            VnpTxnRef = orderId, // Reuse this field for transaction reference
            SePayOrderId = orderId,
            SePayTransferContent = transferContent,
            SePayBankCode = _settings.Bank.Code,
            SePayBankAccount = _settings.Bank.AccountNumber,
            Metadata = JsonSerializer.Serialize(new
            {
                Description = request.Description,
                CreatedBy = userId
            }),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        await _paymentRepository.CreateAsync(payment, ct);

        var qrCodeUrl = BuildSePayVietQrUrl(request.Amount, transferContent);

        _logger.LogInformation(
            "SePay payment created. OrderId: {OrderId}, BookingId: {BookingId}, Amount: {Amount}",
            orderId, request.BookingId, request.Amount);

        return new SePayPaymentResponseDto(
            OrderId: orderId,
            QrCodeBase64: null,
            QrCodeUrl: qrCodeUrl,
            BankCode: _settings.Bank.Code,
            BankAccount: _settings.Bank.AccountNumber,
            AccountName: _settings.Bank.AccountName,
            TransferContent: transferContent,
            Amount: request.Amount,
            Status: nameof(PaymentStatus.Pending)
        );
    }

    public async Task<SePayPaymentResponseDto> CreateOwnerSubscriptionPaymentAsync(
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

        var orderId = GenerateOrderId();
        var transferContent = GenerateTransferContent(orderId);

        var payment = new PaymentTransaction
        {
            PaymentId = Guid.NewGuid(),
            OwnerUpgradeRequestId = request.OwnerUpgradeRequestId,
            UserId = userId,
            Amount = upgradeRequest.FeeAmount,
            PaymentMethod = PaymentConstants.SePayProvider,
            PaymentStatus = nameof(PaymentStatus.Pending),
            PaymentType = "Subscription",
            VnpTxnRef = orderId,
            SePayOrderId = orderId,
            SePayTransferContent = transferContent,
            SePayBankCode = _settings.Bank.Code,
            SePayBankAccount = _settings.Bank.AccountNumber,
            Metadata = JsonSerializer.Serialize(new
            {
                Description = request.Description ?? $"Owner Subscription ({upgradeRequest.PlanType})",
                CreatedBy = userId
            }),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        await _paymentRepository.CreateAsync(payment, ct);

        var qrCodeUrl = BuildSePayVietQrUrl(upgradeRequest.FeeAmount, transferContent);

        _logger.LogInformation(
            "SePay subscription payment created. OrderId: {OrderId}, RequestId: {RequestId}, Amount: {Amount}",
            orderId, request.OwnerUpgradeRequestId, upgradeRequest.FeeAmount);

        return new SePayPaymentResponseDto(
            OrderId: orderId,
            QrCodeBase64: null,
            QrCodeUrl: qrCodeUrl,
            BankCode: _settings.Bank.Code,
            BankAccount: _settings.Bank.AccountNumber,
            AccountName: _settings.Bank.AccountName,
            TransferContent: transferContent,
            Amount: upgradeRequest.FeeAmount,
            Status: nameof(PaymentStatus.Pending)
        );
    }

    public async Task<SePayPaymentResponseDto> CreateWalletTopUpPaymentAsync(
        WalletTopUpRequestDto request,
        Guid userId,
        CancellationToken ct = default)
    {
        if (request.Amount < 10000)
            throw new BadRequestException("Số tiền nạp tối thiểu 10,000 VND");

        var orderId = GenerateOrderId();
        var transferContent = GenerateTransferContent(orderId);

        var payment = new PaymentTransaction
        {
            PaymentId = Guid.NewGuid(),
            BookingId = null,
            OwnerUpgradeRequestId = null,
            UserId = userId,
            Amount = request.Amount,
            PaymentMethod = PaymentConstants.SePayProvider,
            PaymentStatus = nameof(PaymentStatus.Pending),
            PaymentType = "WalletTopUp",
            VnpTxnRef = orderId,
            SePayOrderId = orderId,
            SePayTransferContent = transferContent,
            SePayBankCode = _settings.Bank.Code,
            SePayBankAccount = _settings.Bank.AccountNumber,
            Metadata = JsonSerializer.Serialize(new { Description = request.Description }),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        await _paymentRepository.CreateAsync(payment, ct);
        var qrCodeUrl = BuildSePayVietQrUrl(request.Amount, transferContent);

        _logger.LogInformation(
            "SePay wallet top-up created. OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}",
            orderId, userId, request.Amount);

        return new SePayPaymentResponseDto(
            OrderId: orderId,
            QrCodeBase64: null,
            QrCodeUrl: qrCodeUrl,
            BankCode: _settings.Bank.Code,
            BankAccount: _settings.Bank.AccountNumber,
            AccountName: _settings.Bank.AccountName,
            TransferContent: transferContent,
            Amount: request.Amount,
            Status: nameof(PaymentStatus.Pending)
        );
    }

    /// <summary>
    /// SECURITY CRITICAL: Processes SePay webhook.
    /// API Key verification đã thực hiện tại Controller.
    /// Payload format: https://developer.sepay.vn/sepay-webhooks/tich-hop-webhook
    /// </summary>
    public async Task<(bool Success, string? ErrorReason)> ProcessWebhookAsync(SePayWebhookDto webhook, CancellationToken ct = default)
    {
        var processingStartTime = DateTime.UtcNow;

        // VALIDATION: Chỉ xử lý tiền vào (transferType = "in")
        if (!webhook.TransferType.Equals("in", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("SePay webhook: Bỏ qua transferType={Type}", webhook.TransferType);
            return (false, $"transferType={webhook.TransferType}, expected 'in'");
        }

        if (webhook.TransferAmount <= 0)
        {
            _logger.LogError("SePay webhook: Invalid amount {Amount}", webhook.TransferAmount);
            return (false, $"Invalid amount: {webhook.TransferAmount}");
        }

        // Trích xuất OrderId từ content hoặc description (format: SMARTPARKING SP_20250223_ABC12345)
        var orderId = ExtractOrderIdFromContent(webhook.Content)
            ?? ExtractOrderIdFromContent(webhook.Description ?? "")
            ?? webhook.Code;
        if (string.IsNullOrWhiteSpace(orderId))
        {
            _logger.LogWarning("SePay webhook: Không tìm thấy OrderId. Content={Content}, Description={Desc}", webhook.Content, webhook.Description);
            return (false, $"OrderId not found in content. Content='{webhook.Content}'");
        }

        _logger.LogInformation(
            "SePay webhook received. OrderId: {OrderId}, Id: {Id}, Amount: {Amount}, Content: {Content}",
            orderId, webhook.Id, webhook.TransferAmount, webhook.Content);

        // Find payment transaction
        var payment = await _paymentRepository.GetByTxnRefAsync(orderId, ct);
        if (payment == null)
        {
            _logger.LogWarning(
                "SePay webhook: Payment not found for OrderId: {OrderId}",
                orderId);
            return (false, $"Payment not found for OrderId: {orderId}. Ensure you created payment via app first.");
        }

        // SECURITY: Idempotency check - prevent duplicate processing
        if (payment.PaymentStatus != nameof(PaymentStatus.Pending))
        {
            _logger.LogInformation(
                "SePay webhook: Payment already processed. OrderId: {OrderId}, CurrentStatus: {Status}",
                orderId, payment.PaymentStatus);

            var alreadyOk = payment.PaymentStatus == nameof(PaymentStatus.Success);
            return (alreadyOk, alreadyOk ? null : $"Payment already {payment.PaymentStatus}");
        }

        // STEP 3: Validate amount (prevent fraud) - dung tolerance 1 VND cho lam tron
        if (Math.Abs(webhook.TransferAmount - payment.Amount) > 1)
        {
            _logger.LogError(
                "SePay webhook: Amount mismatch. Expected: {Expected}, Received: {Received}, OrderId: {OrderId}",
                payment.Amount, webhook.TransferAmount, orderId);

            payment.PaymentStatus = nameof(PaymentStatus.Failed);
            payment.Metadata = JsonSerializer.Serialize(new
            {
                Error = "Số tiền không khớp",
                ExpectedAmount = payment.Amount,
                ReceivedAmount = webhook.TransferAmount
            });
            await _paymentRepository.UpdateAsync(payment, ct);
            return (false, $"Số tiền không khớp. Mong đợi: {payment.Amount}, Nhận được: {webhook.TransferAmount}");
        }

        // STEP 4: Cập nhật trạng thái "Đã thanh toán"
        payment.PaymentStatus = nameof(PaymentStatus.Success);
        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, ct);

        // STEP 5: Log webhook for audit trail
        var log = new PaymentLog
        {
            LogId = Guid.NewGuid(),
            PaymentId = payment.PaymentId,
            RawData = JsonSerializer.Serialize(webhook),
            CreatedAt = DateTime.UtcNow
        };
        await _paymentRepository.CreateLogAsync(log, ct);

        // STEP 6: Cập nhật booking "Đã thanh toán" và chuyển tiền sang owner
        if (payment.BookingId.HasValue && payment.PaymentType == "Booking")
        {
            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId.Value, includeDeleted: false, ct);
            if (booking != null)
            {
                if (booking.Status == nameof(BookingStatus.Pending))
                {
                    booking.Status = nameof(BookingStatus.Confirmed);
                    await _bookingRepository.UpdateAsync(booking, ct);
                }
                var totalPaid = await _paymentRepository.GetTotalPaidForBookingAsync(payment.BookingId.Value, ct);
                var ownerId = booking.ParkingLot?.OwnerId ?? Guid.Empty;
                if (ownerId != Guid.Empty && totalPaid > 0)
                {
                    await _walletService.TransferBookingToOwnerAsync(payment.BookingId.Value, totalPaid, ownerId, ct);
                }
                _logger.LogInformation(
                    "Booking confirmed via SePay. BookingId: {BookingId}, OrderId: {OrderId}",
                    booking.BookingId, orderId);

                // Thông báo thanh toán thành công cho user
                await _notificationService.CreatePaymentNotificationAsync(
                    payment.UserId,
                    "Thanh toán thành công",
                    $"Bạn đã thanh toán {payment.Amount:N0}đ cho đặt chỗ tại bãi xe {booking.ParkingLot?.Name ?? "bãi xe"} qua SePay.",
                    payment.PaymentId,
                    ct);
            }
        }
        else if (payment.PaymentType == "Subscription" && payment.OwnerUpgradeRequestId.HasValue)
        {
            var upgradeRequest = await _ownerUpgradeRequestRepository.GetByIdAsync(payment.OwnerUpgradeRequestId.Value, ct);
            if (upgradeRequest != null && (upgradeRequest.Status == "Pending" || upgradeRequest.Status == "PendingPayment"))
            {
                upgradeRequest.Status = "PendingApproval";
                upgradeRequest.PaymentTransactionId = payment.PaymentId;
                await _ownerUpgradeRequestRepository.UpdateAsync(upgradeRequest, ct);

                _logger.LogInformation(
                    "Owner upgrade request moved to PendingApproval via SePay. RequestId: {RequestId}, OrderId: {OrderId}",
                    upgradeRequest.RequestId, orderId);

                // Thông báo thanh toán phí đăng ký thành công
                await _notificationService.SendNotificationAsync(
                    new SendNotificationDto(
                        UserId: upgradeRequest.UserId,
                        Title: "Thanh toán phí đăng ký thành công",
                        Message: $"Bạn đã thanh toán {payment.Amount:N0}đ cho phí đăng ký làm chủ bãi xe qua SePay. Yêu cầu đang chờ Admin duyệt.",
                        Type: "Success"),
                    null, ct);
            }
        }
        else if (payment.PaymentType == "WalletTopUp")
        {
            await _walletService.CreditWalletFromPaymentAsync(
                payment.UserId,
                payment.Amount,
                $"Nạp tiền ví qua SePay - {orderId}",
                ct);
            _logger.LogInformation(
                "Wallet top-up credit via SePay. UserId: {UserId}, Amount: {Amount}, OrderId: {OrderId}",
                payment.UserId, payment.Amount, orderId);
        }

        var processingTime = (DateTime.UtcNow - processingStartTime).TotalMilliseconds;

        _logger.LogInformation(
            "SePay webhook processed successfully. OrderId: {OrderId}, ProcessingTime: {ProcessingTime}ms",
            orderId, processingTime);

        return (true, null);
    }

    /// <summary>Trích xuất OrderId từ nội dung chuyển khoản.
    /// SePay/SMS có thể gửi: SMARTPARKING SP202603022F9F2012 (không gạch dưới) hoặc SMARTPARKING SP_20260302_2F9F2012
    /// Chuẩn hóa về SP_yyyyMMdd_xxxx để tra cứu trong DB.</summary>
    private static string? ExtractOrderIdFromContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;
        // Format 1: SMARTPARKING SP_20260302_2F9F2012 (co gach duoi)
        var m1 = System.Text.RegularExpressions.Regex.Match(content, @"SMARTPARKING\s+(SP_\d{8}_[A-Z0-9]{6,8})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (m1.Success) return m1.Groups[1].Value;
        // Format 2: SMARTPARKING SP202603022F9F2012 (khong gach duoi - tu SMS/SePay)
        var m2 = System.Text.RegularExpressions.Regex.Match(content, @"SMARTPARKING\s+(SP)(\d{8})([A-Z0-9]{6,8})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (m2.Success) return $"{m2.Groups[1].Value}_{m2.Groups[2].Value}_{m2.Groups[3].Value}";
        return null;
    }

    /// <summary>
    /// Generates unique order ID for SePay
    /// Format: SP_YYYYMMDD_UNIQUEID
    /// </summary>
    private static string GenerateOrderId()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return $"SP_{date}_{uniqueId}";
    }

    /// <summary>
    /// Generates transfer content for bank transfer
    /// User must enter this exactly when transferring
    /// </summary>
    private static string GenerateTransferContent(string orderId)
    {
        return $"SMARTPARKING {orderId}";
    }

    /// <summary>
    /// Build SePay VietQR URL - chuẩn QR chuyển khoản VN, app ngân hàng quét được.
    /// API: https://qr.sepay.vn/img?acc=...&bank=...&amount=...&des=...
    /// Tham khảo: https://developer.sepay.vn/vi/tien-ich-khac/tao-qr-code
    /// </summary>
    private string BuildSePayVietQrUrl(decimal amount, string transferContent)
    {
        var bankName = GetSePayBankName(_settings.Bank.Code);
        var amountInt = (int)Math.Round(amount);
        var des = Uri.EscapeDataString(transferContent);
        return $"https://qr.sepay.vn/img?acc={_settings.Bank.AccountNumber}&bank={bankName}&amount={amountInt}&des={des}";
    }

    /// <summary>
    /// Map bank code (MB, VCB...) to SePay bank name (MBBank, Vietcombank...)
    /// Danh sách: https://qr.sepay.vn/banks.json
    /// </summary>
    private static string GetSePayBankName(string code)
    {
        return code?.ToUpperInvariant() switch
        {
            "MB" => "MBBank",
            "VCB" => "Vietcombank",
            "BIDV" => "BIDV",
            "TCB" => "Techcombank",
            "ACB" => "ACB",
            "VPB" => "VPBank",
            "TPB" => "TPBank",
            "STB" => "Sacombank",
            "VIB" => "VIB",
            "HDB" => "HDBank",
            "MSB" => "MSB",
            "OCB" => "OCB",
            "ICB" => "VietinBank",
            "VBA" => "Agribank",
            "LPB" => "LienVietPostBank",
            _ => code ?? "MBBank"
        };
    }

    private void ValidateSettings()
    {
        if (!_settings.Enabled) return;
        if (string.IsNullOrEmpty(_settings.WebhookSecret))
            throw new InvalidOperationException("SePay WebhookSecret (Secret Key) chưa cấu hình. Lấy từ SePay Dashboard > Thông tin đơn vị.");
    }
}

/// <summary>
/// SePay configuration settings (Production-Ready)
/// SECURITY: ApiKey and WebhookSecret should come from environment variables
/// </summary>
public sealed class SePaySettings
{
    public bool Enabled { get; set; }
    public string MerchantId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.sepay.vn";
    public SePayBankSettings Bank { get; set; } = new();
    public SePayUrlSettings Urls { get; set; } = new();
    public SePayWebhookSettings Webhook { get; set; } = new();

    // Legacy properties for backward compatibility
    [Obsolete("Use Bank.Code instead")]
    public string BankCode
    {
        get => Bank.Code;
        set => Bank.Code = value;
    }

    [Obsolete("Use Bank.AccountNumber instead")]
    public string BankAccount
    {
        get => Bank.AccountNumber;
        set => Bank.AccountNumber = value;
    }

    [Obsolete("Use Bank.AccountName instead")]
    public string AccountName
    {
        get => Bank.AccountName;
        set => Bank.AccountName = value;
    }

    [Obsolete("Use Urls.ReturnUrl instead")]
    public string ReturnUrl
    {
        get => Urls.ReturnUrl;
        set => Urls.ReturnUrl = value;
    }

    [Obsolete("Use Urls.WebhookUrl instead")]
    public string WebhookUrl
    {
        get => Urls.WebhookUrl;
        set => Urls.WebhookUrl = value;
    }
}

public sealed class SePayBankSettings
{
    public string Code { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
}

public sealed class SePayUrlSettings
{
    public string ReturnUrl { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
}

public sealed class SePayWebhookSettings
{
    public bool RequireHttps { get; set; } = true;
    public List<string> AllowedIpAddresses { get; set; } = new();
    public int TimeoutSeconds { get; set; } = 30;
}
