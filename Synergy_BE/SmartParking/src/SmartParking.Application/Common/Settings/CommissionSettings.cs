namespace SmartParking.Application.Common.Settings;

/// <summary>
/// Commission settings for revenue calculation
/// </summary>
public sealed class CommissionSettings
{
    /// <summary>
    /// Commission rate as percentage (e.g., 15 means 15%)
    /// </summary>
    public decimal CommissionRatePercent { get; init; } = 10m;
}
