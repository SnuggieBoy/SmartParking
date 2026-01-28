using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.DTOs.Owner;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Services;

public sealed class OwnerBankAccountService : IOwnerBankAccountService
{
    private readonly IOwnerBankAccountRepository _repository;
    private readonly IUserRepository _userRepository;

    public OwnerBankAccountService(
        IOwnerBankAccountRepository repository,
        IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<OwnerBankAccountDto>> GetForCurrentOwnerAsync(Guid userId, CancellationToken ct = default)
    {
        var accounts = await _repository.GetByUserIdAsync(userId, ct);
        return accounts.Select(MapToDto).ToList();
    }

    public async Task<OwnerBankAccountDto> CreateAsync(Guid userId, CreateOwnerBankAccountDto request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
                   ?? throw new NotFoundException(Messages.Auth.UserNotFound);

        // SECURITY: Only Owner or Admin should create bank accounts for payouts
        if (user.Role.RoleName != AuthConstants.Roles.Owner &&
            user.Role.RoleName != AuthConstants.Roles.Admin)
        {
            throw new ForbiddenException("Only Owners or Admins can create owner bank accounts");
        }

        var now = DateTime.UtcNow;

        var account = new OwnerBankAccount
        {
            OwnerBankAccountId = Guid.NewGuid(),
            UserId = userId,
            BankCode = request.BankCode.Trim(),
            BankName = request.BankName.Trim(),
            AccountNumber = request.AccountNumber.Trim(),
            AccountHolderName = request.AccountHolderName.Trim(),
            IsDefault = request.IsDefault,
            IsVerified = false,
            CreatedAt = now,
            UpdatedAt = null,
            VerifiedAt = null,
            VerifiedBy = null
        };

        // If this is default, unset previous default
        if (request.IsDefault)
        {
            var existing = await _repository.GetByUserIdAsync(userId, ct);
            foreach (var acc in existing.Where(a => a.IsDefault))
            {
                acc.IsDefault = false;
                acc.UpdatedAt = now;
                await _repository.UpdateAsync(acc, ct);
            }
        }

        var created = await _repository.CreateAsync(account, ct);
        return MapToDto(created);
    }

    public async Task<OwnerBankAccountDto> UpdateAsync(
        Guid userId,
        Guid ownerBankAccountId,
        UpdateOwnerBankAccountDto request,
        bool isAdmin,
        CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(ownerBankAccountId, ct)
                      ?? throw new NotFoundException("Owner bank account not found");

        if (!isAdmin && account.UserId != userId)
        {
            throw new ForbiddenException();
        }

        if (!string.IsNullOrWhiteSpace(request.BankCode))
        {
            account.BankCode = request.BankCode.Trim();
        }
        if (!string.IsNullOrWhiteSpace(request.BankName))
        {
            account.BankName = request.BankName.Trim();
        }
        if (!string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            account.AccountNumber = request.AccountNumber.Trim();
        }
        if (!string.IsNullOrWhiteSpace(request.AccountHolderName))
        {
            account.AccountHolderName = request.AccountHolderName.Trim();
        }

        var now = DateTime.UtcNow;

        if (request.IsDefault.HasValue && request.IsDefault.Value)
        {
            var existing = await _repository.GetByUserIdAsync(account.UserId, ct);
            foreach (var acc in existing.Where(a => a.IsDefault && a.OwnerBankAccountId != ownerBankAccountId))
            {
                acc.IsDefault = false;
                acc.UpdatedAt = now;
                await _repository.UpdateAsync(acc, ct);
            }

            account.IsDefault = true;
        }

        account.UpdatedAt = now;
        await _repository.UpdateAsync(account, ct);

        return MapToDto(account);
    }

    public async Task DeleteAsync(Guid userId, Guid ownerBankAccountId, bool isAdmin, CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(ownerBankAccountId, ct)
                      ?? throw new NotFoundException("Owner bank account not found");

        if (!isAdmin && account.UserId != userId)
        {
            throw new ForbiddenException();
        }

        await _repository.DeleteAsync(account, ct);
    }

    public async Task<IReadOnlyList<OwnerBankAccountDto>> GetForOwnerAsync(Guid ownerId, CancellationToken ct = default)
    {
        var accounts = await _repository.GetByUserIdAsync(ownerId, ct);
        return accounts.Select(MapToDto).ToList();
    }

    public async Task<OwnerBankAccountDto> VerifyAsync(Guid ownerBankAccountId, Guid adminId, CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(ownerBankAccountId, ct)
                      ?? throw new NotFoundException("Owner bank account not found");

        account.IsVerified = true;
        account.VerifiedAt = DateTime.UtcNow;
        account.VerifiedBy = adminId;
        account.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(account, ct);
        return MapToDto(account);
    }

    private static OwnerBankAccountDto MapToDto(OwnerBankAccount account)
    {
        return new OwnerBankAccountDto(
            OwnerBankAccountId: account.OwnerBankAccountId,
            BankCode: account.BankCode,
            BankName: account.BankName,
            AccountNumber: account.AccountNumber,
            AccountHolderName: account.AccountHolderName,
            IsDefault: account.IsDefault,
            IsVerified: account.IsVerified,
            CreatedAt: account.CreatedAt,
            VerifiedAt: account.VerifiedAt);
    }
}

