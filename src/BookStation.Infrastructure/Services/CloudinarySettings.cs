namespace BookStation.Infrastructure.Services;

/// <summary>
/// Configuration settings for Cloudinary.
/// </summary>
public class CloudinarySettings
{
    public const string SectionName = "CloudinarySettings";

    /// <summary>
    /// Gets or sets the Cloudinary cloud name.
    /// </summary>
    public string CloudName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Cloudinary API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Cloudinary API secret.
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;
}
