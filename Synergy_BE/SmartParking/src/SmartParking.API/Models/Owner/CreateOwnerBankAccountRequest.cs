namespace SmartParking.API.Models.Owner;

public sealed class CreateOwnerBankAccountRequest
{
    public string BankCode { get; set; } = null!;
    public string BankName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string AccountHolderName { get; set; } = null!;
    public bool IsDefault { get; set; } = true;
}

