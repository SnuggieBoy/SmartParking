using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Admin;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for transaction management.
/// SECURITY: Only Admin can access these endpoints.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/transactions")]
public sealed class AdminTransactionsController : BaseApiController
{
    private readonly IAdminService _adminService;

    public AdminTransactionsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Get all transactions with filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TransactionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<TransactionDto>>>> GetTransactions(
        [FromQuery] string? status,
        [FromQuery] string? paymentMethod,
        [FromQuery] Guid? userId,
        [FromQuery] Guid? parkingLotId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new TransactionFilterDto(status, paymentMethod, userId, parkingLotId, fromDate, toDate, page, pageSize);
        var result = await _adminService.GetTransactionsAsync(filter, ct);
        return Ok(ApiResponse<PagedResult<TransactionDto>>.SuccessResponse(result, "Transactions retrieved successfully"));
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> GetTransaction(Guid id, CancellationToken ct = default)
    {
        var transaction = await _adminService.GetTransactionByIdAsync(id, ct);
        return Ok(ApiResponse<TransactionDto>.SuccessResponse(transaction, "Transaction retrieved successfully"));
    }

    /// <summary>
    /// Process refund for a transaction
    /// </summary>
    [HttpPost("{id:guid}/refund")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> ProcessRefund(
        Guid id,
        [FromBody] ProcessRefundDto request,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var transaction = await _adminService.ProcessRefundAsync(id, request, adminId, ct);
        return Ok(ApiResponse<TransactionDto>.SuccessResponse(transaction, "Refund processed successfully"));
    }
}
