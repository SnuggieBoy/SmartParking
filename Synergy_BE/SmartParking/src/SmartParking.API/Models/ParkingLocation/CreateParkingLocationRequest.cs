using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.ParkingLocation;

/// <summary>
/// API model for creating a new parking location.
/// Includes validation attributes for input validation.
/// </summary>
public sealed class CreateParkingLocationRequest
{
    [Required(ErrorMessage = "Tên bãi xe là bắt buộc")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên phải từ 3 đến 100 ký tự")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Vĩ độ là bắt buộc")]
    [Range(-90, 90, ErrorMessage = "Vĩ độ phải từ -90 đến 90")]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "Kinh độ là bắt buộc")]
    [Range(-180, 180, ErrorMessage = "Kinh độ phải từ -180 đến 180")]
    public double Longitude { get; set; }

    [Required(ErrorMessage = "Tỉnh/TP là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tỉnh/TP không được quá 100 ký tự")]
    public string Province { get; set; } = null!;

    [Required(ErrorMessage = "Quận/Huyện là bắt buộc")]
    [StringLength(100, ErrorMessage = "Quận/Huyện không được quá 100 ký tự")]
    public string District { get; set; } = null!;

    [Required(ErrorMessage = "Xã/Phường là bắt buộc")]
    [StringLength(100, ErrorMessage = "Xã/Phường không được quá 100 ký tự")]
    public string Ward { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Đường phố không được quá 200 ký tự")]
    public string? Street { get; set; }

    [StringLength(100, ErrorMessage = "Khu vực không được quá 100 ký tự")]
    public string? Area { get; set; }

    [StringLength(500, ErrorMessage = "Địa chỉ đầy đủ không được quá 500 ký tự")]
    public string? FullAddress { get; set; }

    [Required(ErrorMessage = "Tổng số chỗ là bắt buộc")]
    [Range(1, 10000, ErrorMessage = "Tổng số chỗ phải từ 1 đến 10000")]
    public int TotalSlots { get; set; }

    [Required(ErrorMessage = "Số chỗ trống là bắt buộc")]
    [Range(0, 10000, ErrorMessage = "Số chỗ trống phải từ 0 đến 10000")]
    public int AvailableSlots { get; set; }

    [Required(ErrorMessage = "Giá mỗi giờ là bắt buộc")]
    [Range(0, 1000000, ErrorMessage = "Giá mỗi giờ phải từ 0 đến 1.000.000")]
    public decimal PricePerHour { get; set; }
}
