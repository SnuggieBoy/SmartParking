using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers;

[Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
[Route("api/wallet")]
public sealed class WalletController : BaseApiController
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("balance")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetBalance(CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var balance = await _walletService.GetBalanceAsync(userId, ct);
        return Ok(ApiResponse<object>.SuccessResponse(new { balance }, "Balance retrieved"));
    }

    [HttpPost("topup")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> TopUp([FromBody] TopUpRequest request, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _walletService.TopUpAsync(userId, request.Amount, ct);
        return Ok(ApiResponse<object>.SuccessResponse(result, "Top-up successful"));
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<object>>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _walletService.GetTransactionsAsync(userId, page, pageSize, ct);
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
        return Ok(ApiResponse<PagedResult<object>>.SuccessResponse(paged, "Transactions retrieved"));
    }

    [HttpPost("pay-booking")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> PayBooking([FromBody] PayBookingRequest request, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _walletService.PayWithWalletAsync(userId, request.BookingId, request.Amount, ct);
        if (!result.Success)
            return BadRequest(ApiResponse.FailureResponse(result.Message ?? "Thanh toán thất bại"));
        return Ok(ApiResponse<object>.SuccessResponse(new { balance = result.NewBalance }, result.Message ?? "Thanh toán thành công"));
    }
    [HttpPost("pay-owner-upgrade")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> PayOwnerUpgrade([FromBody] PayOwnerUpgradeRequest request, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _walletService.PayOwnerUpgradeFromWalletAsync(userId, request.OwnerUpgradeRequestId, request.Amount, ct);
        if (!result.Success)
            return BadRequest(ApiResponse.FailureResponse(result.Message ?? "Thanh toán phí đăng ký thất bại"));
        return Ok(ApiResponse<object>.SuccessResponse(new { balance = result.NewBalance }, result.Message ?? "Thanh toán phí đăng ký thành công"));
    }

    [HttpPost("pay-extension")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> PayExtension([FromBody] PayBookingRequest request, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _walletService.PayExtensionWithWalletAsync(userId, request.BookingId, request.Amount, ct);
        if (!result.Success)
            return BadRequest(ApiResponse.FailureResponse(result.Message ?? "Thanh toán gia hạn thất bại"));
        return Ok(ApiResponse<object>.SuccessResponse(new { balance = result.NewBalance }, result.Message ?? "Thanh toán gia hạn thành công"));
    }
}

public record TopUpRequest(decimal Amount);
public record PayBookingRequest(Guid BookingId, decimal Amount);
public record PayOwnerUpgradeRequest(Guid OwnerUpgradeRequestId, decimal Amount);
