namespace YouSummarize.Services.TravelPlanner.Sources;

/// <summary>
/// Metadata extracted from a content source (YouTube video, article, podcast, etc.)
/// </summary>
/// <param name="Title">The title of the content</param>
/// <param name="Url">The standardized URL of the content</param>
/// <param name="SourceType">The type of content source (e.g., "YouTube", "Article", "Podcast")</param>
/// <param name="Author">Optional author or channel name</param>
/// <param name="PublishedDate">Optional publication date</param>
/// <param name="Duration">Optional duration for video/audio content</param>
public record ContentMetadata(
    string Title,
    string Url,
    string SourceType,
    string? Author = null,
    DateTime? PublishedDate = null,
    TimeSpan? Duration = null,
    string? ThumbnailUrl = null);

/// <summary>
/// Generic interface for extracting content from various sources (YouTube, articles, podcasts, etc.)
/// Implementations should handle source-specific logic for content extraction and metadata retrieval.
/// </summary>
public interface IContentSourceService
{
    /// <summary>
    /// Gets the type of content source this service handles (e.g., "YouTube", "Article", "Podcast")
    /// </summary>
    string SourceType { get; }

    /// <summary>
    /// Indicates whether this service can handle the given URL.
    /// </summary>
    bool CanHandle(string url);

    /// <summary>
    /// Extracts metadata from the content source URL
    /// </summary>
    /// <param name="sourceUrl">The URL of the content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Content metadata including title, URL, author, etc.</returns>
    Task<ContentMetadata> ExtractMetadata(string sourceUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the main content/transcript from the source
    /// </summary>
    /// <param name="sourceUrl">The URL of the content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The content text (transcript, article text, etc.)</returns>
    Task<string> GetContent(string sourceUrl, CancellationToken cancellationToken = default);
}
