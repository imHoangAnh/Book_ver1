namespace BookStation.Application.Services;

/// <summary>
/// Service interface for uploading images to cloud storage.
/// This service is designed to be scalable and reusable for different image types.
/// </summary>
public interface IImageUploadService
{
    /// <summary>
    /// Uploads an avatar image with face-detection cropping.
    /// Images are stored in the "bookstation/avatars" folder.
    /// </summary>
    /// <param name="imageStream">The image stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload result containing URL and metadata.</returns>
    Task<ImageUploadResult> UploadAvatarAsync(
        Stream imageStream, 
        string fileName, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a book cover image with optimized settings for book displays.
    /// Images are stored in the "bookstation/books/covers" folder.
    /// </summary>
    /// <param name="imageStream">The image stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload result containing URL and metadata.</returns>
    Task<ImageUploadResult> UploadBookCoverAsync(
        Stream imageStream, 
        string fileName, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a general image to a specified folder.
    /// </summary>
    /// <param name="imageStream">The image stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="folder">The folder to upload to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload result containing URL and metadata.</returns>
    Task<ImageUploadResult> UploadImageAsync(
        Stream imageStream, 
        string fileName,
        string folder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an image by its public ID.
    /// </summary>
    /// <param name="publicId">The public ID of the image.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted successfully.</returns>
    Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an image upload operation.
/// </summary>
public record ImageUploadResult(
    string Url,
    string PublicId,
    bool Success,
    string? Error = null
);
