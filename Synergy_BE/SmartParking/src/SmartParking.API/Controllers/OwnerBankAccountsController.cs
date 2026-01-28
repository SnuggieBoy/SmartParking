using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.API.Models.Owner;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Owner;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers;

/// <summary>
/// Owner endpoints for managing bank accounts used for payouts.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
[Route("api/owners/bank-accounts")]
public sealed class OwnerBankAccountsController : BaseApiController
{
    private readonly IOwnerBankAccountService _service;

    public OwnerBankAccountsController(IOwnerBankAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OwnerBankAccountDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OwnerBankAccountDto>>>> GetMyBankAccounts(
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var accounts = await _service.GetForCurrentOwnerAsync(userId, ct);
        return Ok(ApiResponse<IEnumerable<OwnerBankAccountDto>>.SuccessResponse(
            accounts,
            "Owner bank accounts retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OwnerBankAccountDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<OwnerBankAccountDto>>> CreateBankAccount(
        [FromBody] CreateOwnerBankAccountRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();

        var dto = new CreateOwnerBankAccountDto(
            request.BankCode,
            request.BankName,
            request.AccountNumber,
            request.AccountHolderName,
            request.IsDefault);

        var result = await _service.CreateAsync(userId, dto, ct);

        return CreatedAtAction(
            nameof(GetMyBankAccounts),
            null,
            ApiResponse<OwnerBankAccountDto>.SuccessResponse(
                result,
                "Owner bank account created successfully"));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OwnerBankAccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OwnerBankAccountDto>>> UpdateBankAccount(
        Guid id,
        [FromBody] UpdateOwnerBankAccountRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();

        var dto = new UpdateOwnerBankAccountDto(
            request.BankCode,
            request.BankName,
            request.AccountNumber,
            request.AccountHolderName,
            request.IsDefault);

        var result = await _service.UpdateAsync(userId, id, dto, isAdmin, ct);

        return Ok(ApiResponse<OwnerBankAccountDto>.SuccessResponse(
            result,
            "Owner bank account updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteBankAccount(
        Guid id,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();

        await _service.DeleteAsync(userId, id, isAdmin, ct);
        return Ok(ApiResponse.SuccessResponse("Owner bank account deleted successfully"));
    }
}

