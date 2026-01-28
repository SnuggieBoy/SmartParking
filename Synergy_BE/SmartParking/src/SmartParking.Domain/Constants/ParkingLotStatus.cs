namespace SmartParking.Domain.Constants;

/// <summary>
/// Well-known status values for parking lots.
/// Helps keep P2P approval flow consistent across the system.
/// </summary>
public static class ParkingLotStatus
{
    /// <summary>
    /// Owner has created the lot, waiting for Admin review.
    /// </summary>
    public const string PendingApproval = "PendingApproval";

    /// <summary>
    /// Lot is approved and can appear in public search / nearby results.
    /// </summary>
    public const string Approved = "Approved";

    /// <summary>
    /// Lot has been rejected by Admin (optionally with a reason).
    /// </summary>
    public const string Rejected = "Rejected";

    /// <summary>
    /// Lot is temporarily disabled (but still approved historically).
    /// </summary>
    public const string Inactive = "Inactive";
}

