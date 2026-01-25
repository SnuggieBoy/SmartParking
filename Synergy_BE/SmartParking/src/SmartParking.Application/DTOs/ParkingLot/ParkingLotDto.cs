namespace SmartParking.Application.DTOs.ParkingLot;

public sealed record ParkingLotDto(
    Guid ParkingLotId,
    string Name,
    string Address,
    int TotalCapacity,
    int AvailableCapacity,
    int CurrentOccupancy,
    decimal PricePerHour,
    string Status,
    DateTime? CreatedAt
);

public sealed record CreateParkingLotDto(
    string Name,
    string Address,
    int TotalCapacity,
    decimal PricePerHour
);

public sealed record UpdateParkingLotDto(
    string Name,
    string Address,
    int TotalCapacity,
    decimal PricePerHour,
    string Status
);
