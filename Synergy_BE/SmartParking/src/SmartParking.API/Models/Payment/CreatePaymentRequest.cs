using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Payment;

public sealed class CreatePaymentRequest
{
    [Required(ErrorMessage = "Booking ID is required")]
    public Guid BookingId { get; init; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(1000, double.MaxValue, ErrorMessage = "Amount must be at least 1000 VND")]
    public decimal Amount { get; init; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
    public string Description { get; init; } = string.Empty;
}
