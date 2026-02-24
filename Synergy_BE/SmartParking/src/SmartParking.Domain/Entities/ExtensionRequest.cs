#nullable enable

using System;

namespace SmartParking.Domain.Entities;

public class ExtensionRequest
{
    public Guid ExtensionRequestId { get; set; }

    public Guid BookingId { get; set; }

    public DateTime RequestedEndTime { get; set; }

    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

    public string? RejectReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public Guid? ProcessedBy { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
