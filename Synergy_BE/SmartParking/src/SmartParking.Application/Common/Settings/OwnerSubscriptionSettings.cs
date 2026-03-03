namespace SmartParking.Application.Common.Settings;

/// <summary>
/// Configuration for owner subscription fees.
/// Values can be overridden from appsettings.json.
/// </summary>
public sealed class OwnerSubscriptionSettings
{
    public decimal MonthlyFee { get; init; } = 100_000m;

    public decimal YearlyFee { get; init; } = 1_000_000m;

    public int MonthlyDurationDays { get; init; } = 30;

    public int YearlyDurationDays { get; init; } = 365;
}

