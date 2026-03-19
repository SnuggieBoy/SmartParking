using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers.Admin;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/wallet")]
public sealed class AdminWalletController : BaseApiController
{
    private readonly IWalletService _walletService;

    public AdminWalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("balance")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetBalance(CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var balance = await _walletService.GetBalanceAsync(adminId, ct);
        return Ok(ApiResponse<object>.SuccessResponse(new { balance }, "Lấy số dư ví admin thành công"));
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<object>>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var result = await _walletService.GetTransactionsAsync(adminId, page, pageSize, ct);
        var items = result.Items.Select(t => new
        {
            id = t.Id,
            type = t.Type,
            amount = t.Amount,
            balanceAfter = t.BalanceAfter,
            description = t.Description,
            createdAt = t.CreatedAt
        }).ToList();
        var paged = new PagedResult<object>(items, result.Page, result.PageSize, result.TotalCount);
        return Ok(ApiResponse<PagedResult<object>>.SuccessResponse(paged, "Lấy giao dịch ví admin thành công"));
    }
}
