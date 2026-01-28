using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Admin;

namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service for admin-specific operations (transactions, payouts, reports, settings)
/// </summary>
public interface IAdminService
{
    #region Transactions

    /// <summary>
    /// Get all transactions with filters
    /// </summary>
    Task<PagedResult<TransactionDto>> GetTransactionsAsync(TransactionFilterDto filter, CancellationToken ct = default);

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    Task<TransactionDto> GetTransactionByIdAsync(Guid paymentId, CancellationToken ct = default);

    /// <summary>
    /// Process refund for a transaction
    /// </summary>
    Task<TransactionDto> ProcessRefundAsync(Guid paymentId, ProcessRefundDto request, Guid adminId, CancellationToken ct = default);

    #endregion

    #region Payouts

    /// <summary>
    /// Get all payouts
    /// </summary>
    Task<PagedResult<AdminPayoutDto>> GetPayoutsAsync(string? status, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Create a payout for owner
    /// </summary>
    Task<AdminPayoutDto> CreatePayoutAsync(CreatePayoutDto request, Guid adminId, CancellationToken ct = default);

    /// <summary>
    /// Mark payout as completed
    /// </summary>
    Task<AdminPayoutDto> CompletePayoutAsync(Guid payoutId, Guid adminId, CancellationToken ct = default);

    #endregion

    #region Reports

    /// <summary>
    /// Get revenue report
    /// </summary>
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime fromDate, DateTime toDate, string period, CancellationToken ct = default);

    /// <summary>
    /// Get bookings report
    /// </summary>
    Task<BookingsReportDto> GetBookingsReportAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);

    #endregion

    #region Settings

    /// <summary>
    /// Get system settings
    /// </summary>
    Task<SystemSettingsDto> GetSettingsAsync(CancellationToken ct = default);

    /// <summary>
    /// Update system settings (note: for now, settings are read from appsettings.json)
    /// </summary>
    Task<SystemSettingsDto> UpdateSettingsAsync(UpdateSystemSettingsDto request, CancellationToken ct = default);

    #endregion
}
