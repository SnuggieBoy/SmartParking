using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers;

/// <summary>
/// Media upload endpoints.
/// POST /api/media/upload - Upload image to Cloudinary (returns URL).
/// </summary>
[Route("api/media")]
public sealed class MediaController : BaseApiController
{
    private readonly ICloudinaryService _cloudinaryService;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public MediaController(ICloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    /// <summary>
    /// Upload an image file. Returns the Cloudinary URL on success.
    /// Supported: jpg, jpeg, png, gif, webp. Max 5MB.
    /// </summary>
    /// <param name="file">Image file (form-data, key: "file")</param>
    /// <param name="folder">Optional folder in Cloudinary (e.g., parking-lots, avatars)</param>
    [Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<string>>> Upload(
        IFormFile? file,
        [FromQuery] string? folder = "smartparking",
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse.FailureResponse("No file provided. Use form-data with key 'file'."));
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return BadRequest(ApiResponse.FailureResponse(
                $"Invalid file type. Allowed: {string.Join(", ", AllowedExtensions)}"));
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(ApiResponse.FailureResponse($"File too large. Max size: {MaxFileSizeBytes / 1024 / 1024} MB"));
        }

        using var stream = file.OpenReadStream();
        var url = await _cloudinaryService.UploadImageAsync(
            stream,
            file.FileName,
            file.ContentType,
            folder,
            ct);

        if (string.IsNullOrEmpty(url))
        {
            return BadRequest(ApiResponse.FailureResponse("Image upload failed. Check Cloudinary configuration."));
        }

        return Ok(ApiResponse<string>.SuccessResponse(url, "Image uploaded successfully"));
    }
}
