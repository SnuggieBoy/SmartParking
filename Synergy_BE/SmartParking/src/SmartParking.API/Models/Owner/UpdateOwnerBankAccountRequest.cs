namespace SmartParking.API.Models.Owner;

public sealed class UpdateOwnerBankAccountRequest
{
    public string? BankCode { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountHolderName { get; set; }
    public bool? IsDefault { get; set; }
}

