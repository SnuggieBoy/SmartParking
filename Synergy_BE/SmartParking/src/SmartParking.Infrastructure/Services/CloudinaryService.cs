using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.Infrastructure.Services;

/// <summary>
/// Cloudinary implementation for image upload.
/// Uploads images to Cloudinary and returns public URLs.
/// </summary>
public sealed class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary? _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryService> logger)
    {
        _logger = logger;
        var config = settings.Value;
        _cloudinary = config.IsConfigured
            ? new Cloudinary(config.GetCloudinaryUrl()) { Api = { Secure = true } }
            : null;
    }

    /// <inheritdoc />
    public async Task<string?> UploadImageAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken ct = default)
    {
        if (_cloudinary == null)
        {
            _logger.LogWarning("Cloudinary not configured. Skipping upload.");
            return null;
        }

        // Validate image content type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(contentType?.ToLowerInvariant()))
        {
            _logger.LogWarning("Invalid image type: {ContentType}", contentType);
            return null;
        }

        try
        {
            // Generate unique public_id to avoid overwrites
            var publicId = $"{folder?.TrimEnd('/') ?? "smartparking"}/{Guid.NewGuid():N}";

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = publicId,
                UseFilename = false,
                UniqueFilename = true,
                Overwrite = false,
                Folder = folder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);

            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
                return null;
            }

            _logger.LogInformation("Image uploaded successfully: {PublicId}", uploadResult.PublicId);
            return uploadResult.SecureUrl?.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image to Cloudinary");
            return null;
        }
    }
}
