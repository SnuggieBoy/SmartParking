namespace SmartParking.Application.DTOs.ParkingLot;

/// <summary>
/// DTO used by Admin to reject a parking lot registration with a reason.
/// </summary>
public sealed record RejectParkingLotDto(string Reason);

