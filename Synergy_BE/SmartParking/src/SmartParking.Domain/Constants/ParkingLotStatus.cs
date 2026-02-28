namespace SmartParking.Domain.Constants;

/// <summary>
/// Well-known status values for parking lots.
/// Must match DB CHECK constraint CK_ParkingLots_Status: Pending, Maintenance, Full, Closed, Inactive, Active.
/// </summary>
public static class ParkingLotStatus
{
    /// <summary>
    /// Owner has created the lot, waiting for Admin review.
    /// Maps to DB value "Pending".
    /// </summary>
    public const string PendingApproval = "Pending";

    /// <summary>
    /// Lot is approved and can appear in public search / nearby results.
    /// Maps to DB value "Active".
    /// </summary>
    public const string Approved = "Active";

    /// <summary>
    /// Lot has been rejected by Admin (optionally with a reason).
    /// Maps to DB value "Inactive".
    /// </summary>
    public const string Rejected = "Inactive";

    /// <summary>
    /// Lot is temporarily disabled (but still approved historically).
    /// </summary>
    public const string Inactive = "Inactive";
}

