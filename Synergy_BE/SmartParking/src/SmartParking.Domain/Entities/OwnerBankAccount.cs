// Custom entity for Owner bank account linking.
#nullable enable

using System;

namespace SmartParking.Domain.Entities;

public partial class OwnerBankAccount
{
    public Guid OwnerBankAccountId { get; set; }

    public Guid UserId { get; set; }

    public string BankCode { get; set; } = null!;

    public string BankName { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string AccountHolderName { get; set; } = null!;

    public bool IsDefault { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public Guid? VerifiedBy { get; set; }

    public virtual User User { get; set; } = null!;
}

