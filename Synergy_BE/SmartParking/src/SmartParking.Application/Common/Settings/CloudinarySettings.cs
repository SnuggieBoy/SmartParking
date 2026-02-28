namespace SmartParking.Application.Common.Settings;

/// <summary>
/// Cloudinary configuration for image upload and storage.
/// Bind from appsettings.json "CloudinarySettings" section.
/// </summary>
public sealed class CloudinarySettings
{
    public const string SectionName = "CloudinarySettings";

    /// <summary>
    /// Cloudinary cloud name (e.g., dogogoyuj)
    /// </summary>
    public string CloudName { get; init; } = string.Empty;

    /// <summary>
    /// Cloudinary API key
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Cloudinary API secret (keep secure, use User Secrets in development)
    /// </summary>
    public string ApiSecret { get; init; } = string.Empty;

    /// <summary>
    /// Builds the CLOUDINARY_URL format: cloudinary://ApiKey:ApiSecret@CloudName
    /// </summary>
    public string GetCloudinaryUrl() =>
        $"cloudinary://{ApiKey}:{ApiSecret}@{CloudName}";

    /// <summary>
    /// Returns true if all required credentials are configured
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(CloudName) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(ApiSecret);
}
