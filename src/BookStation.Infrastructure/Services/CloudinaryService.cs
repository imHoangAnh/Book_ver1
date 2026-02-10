using BookStation.Application.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookStation.Infrastructure.Services;

/// <summary>
/// Cloudinary image upload service implementation.
/// This service provides centralized image upload functionality for the entire application.
/// It can be easily extended for additional image types or replaced with another provider.
/// </summary>
public class CloudinaryService : IImageUploadService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    // Folder constants for organized storage
    private const string AvatarsFolder = "bookstation/avatars";
    private const string BookCoversFolder = "bookstation/books/covers";

    public CloudinaryService(
        IOptions<CloudinarySettings> settings,
        ILogger<CloudinaryService> logger)
    {
        _logger = logger;

        var cloudinarySettings = settings.Value;
        
        if (string.IsNullOrEmpty(cloudinarySettings.CloudName) ||
            string.IsNullOrEmpty(cloudinarySettings.ApiKey) ||
            string.IsNullOrEmpty(cloudinarySettings.ApiSecret))
        {
            _logger.LogWarning("Cloudinary settings are not fully configured. Image upload will not work.");
            _cloudinary = null!;
            return;
        }

        var account = new Account(
            cloudinarySettings.CloudName,
            cloudinarySettings.ApiKey,
            cloudinarySettings.ApiSecret
        );
        
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    /// <inheritdoc />
    public async Task<ImageUploadResult> UploadAvatarAsync(
        Stream imageStream, 
        string fileName, 
        CancellationToken cancellationToken = default)
    {
        // Avatar-specific transformation: 300x300, face detection cropping
        var transformation = new Transformation()
            .Width(300)
            .Height(300)
            .Crop("fill")
            .Gravity("face")
            .Quality("auto");

        return await UploadImageInternalAsync(
            imageStream, 
            fileName, 
            AvatarsFolder,
            transformation,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<ImageUploadResult> UploadBookCoverAsync(
        Stream imageStream, 
        string fileName, 
        CancellationToken cancellationToken = default)
    {
        // Book cover-specific transformation: optimized for book display
        var transformation = new Transformation()
            .Width(400)
            .Height(600)
            .Crop("fill")
            .Gravity("center")
            .Quality("auto");

        return await UploadImageInternalAsync(
            imageStream, 
            fileName, 
            BookCoversFolder,
            transformation,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<ImageUploadResult> UploadImageAsync(
        Stream imageStream, 
        string fileName,
        string folder,
        CancellationToken cancellationToken = default)
    {
        return await UploadImageInternalAsync(
            imageStream, 
            fileName, 
            folder,
            new Transformation().Quality("auto"),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (_cloudinary == null)
        {
            _logger.LogError("Cloudinary is not configured. Cannot delete image.");
            return false;
        }

        try
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            
            if (result.Result == "ok")
            {
                _logger.LogInformation("Successfully deleted image with public ID: {PublicId}", publicId);
                return true;
            }

            _logger.LogWarning("Failed to delete image with public ID: {PublicId}. Result: {Result}", 
                publicId, result.Result);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image with public ID: {PublicId}", publicId);
            return false;
        }
    }

    private async Task<ImageUploadResult> UploadImageInternalAsync(
        Stream imageStream,
        string fileName,
        string folder,
        Transformation transformation,
        CancellationToken cancellationToken)
    {
        if (_cloudinary == null)
        {
            _logger.LogError("Cloudinary is not configured. Cannot upload image.");
            return new ImageUploadResult(string.Empty, string.Empty, false, "Cloudinary is not configured.");
        }

        try
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = folder,
                Transformation = transformation,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation(
                    "Successfully uploaded image to Cloudinary. Folder: {Folder}, URL: {Url}, PublicId: {PublicId}",
                    folder,
                    uploadResult.SecureUrl,
                    uploadResult.PublicId
                );

                return new ImageUploadResult(
                    uploadResult.SecureUrl.ToString(),
                    uploadResult.PublicId,
                    true
                );
            }

            _logger.LogWarning(
                "Failed to upload image to Cloudinary. Status: {StatusCode}, Error: {Error}",
                uploadResult.StatusCode,
                uploadResult.Error?.Message
            );

            return new ImageUploadResult(
                string.Empty,
                string.Empty,
                false,
                uploadResult.Error?.Message ?? "Upload failed"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to Cloudinary");
            return new ImageUploadResult(string.Empty, string.Empty, false, ex.Message);
        }
    }
}
