using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Owner;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for viewing and verifying owner bank accounts.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/owners")]
public sealed class AdminOwnerBankAccountsController : BaseApiController
{
    private readonly IOwnerBankAccountService _service;

    public AdminOwnerBankAccountsController(IOwnerBankAccountService service)
    {
        _service = service;
    }

    /// <summary>
    /// ADMIN: Get bank accounts of a specific owner.
    /// </summary>
    [HttpGet("{ownerId:guid}/bank-accounts")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OwnerBankAccountDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OwnerBankAccountDto>>>> GetOwnerBankAccounts(
        Guid ownerId,
        CancellationToken ct = default)
    {
        var accounts = await _service.GetForOwnerAsync(ownerId, ct);
        return Ok(ApiResponse<IEnumerable<OwnerBankAccountDto>>.SuccessResponse(
            accounts,
            "Owner bank accounts retrieved successfully"));
    }

    /// <summary>
    /// ADMIN: Mark a bank account as verified (after KYC/offline checks).
    /// </summary>
    [HttpPost("bank-accounts/{id:guid}/verify")]
    [ProducesResponseType(typeof(ApiResponse<OwnerBankAccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OwnerBankAccountDto>>> VerifyBankAccount(
        Guid id,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var result = await _service.VerifyAsync(id, adminId, ct);

        return Ok(ApiResponse<OwnerBankAccountDto>.SuccessResponse(
            result,
            "Owner bank account verified successfully"));
    }
}

