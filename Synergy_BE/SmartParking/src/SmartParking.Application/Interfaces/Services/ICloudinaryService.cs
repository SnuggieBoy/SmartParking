namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service for uploading images to Cloudinary.
/// Used for parking lot images, user avatars, and other media.
/// </summary>
public interface ICloudinaryService
{
    /// <summary>
    /// Uploads an image file to Cloudinary and returns the public URL.
    /// </summary>
    /// <param name="fileStream">Stream of the image file</param>
    /// <param name="fileName">Original file name (used for Cloudinary public_id)</param>
    /// <param name="contentType">MIME type (e.g., image/jpeg, image/png)</param>
    /// <param name="folder">Optional folder in Cloudinary (e.g., "parking-lots", "avatars")</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Public URL of the uploaded image, or null if upload fails or Cloudinary is not configured</returns>
    Task<string?> UploadImageAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken ct = default);
}
