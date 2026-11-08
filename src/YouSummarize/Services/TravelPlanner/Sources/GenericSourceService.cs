using YouSummarize.Services.Models;
using YouSummarize.ServicesExternal;

namespace YouSummarize.Services.TravelPlanner.Sources;

/// <summary>
/// Service générique pour l'extraction de contenu depuis toute URL supportée par Supadata.
/// Supporte YouTube, TikTok, Instagram, X (Twitter), Facebook et fichiers publics.
/// Utilisé comme service de base ou en fallback lorsqu'aucun service spécialisé ne prend en charge l'URL.
/// </summary>
public class GenericSourceService : IContentSourceService
{
    private readonly ISupadataService _supadataService;
    private readonly YtDlpAudioExtractor _ytDlpAudioExtractor;

    /// <inheritdoc/>
    public string SourceType => "Generic";

    /// <summary>
    /// Initialise une nouvelle instance du service générique.
    /// </summary>
    /// <param name="supadataService">Service pour les appels à l'API Supadata.</param>
    /// <param name="ytDlpAudioExtractor">Extracteur yt-dlp pour les métadonnées.</param>
    public GenericSourceService(ISupadataService supadataService, YtDlpAudioExtractor ytDlpAudioExtractor)
    {
        _supadataService = supadataService;
        _ytDlpAudioExtractor = ytDlpAudioExtractor;
    }

    /// <summary>
    /// Indique si ce service peut traiter l'URL donnée.
    /// Le service générique accepte toujours toute URL valide.
    /// </summary>
    public bool CanHandle(string url) => Uri.TryCreate(url, UriKind.Absolute, out _);

    /// <inheritdoc/>
    public async Task<ContentMetadata> ExtractMetadata(string sourceUrl, CancellationToken cancellationToken = default)
    {
        var metadata = await _ytDlpAudioExtractor.GetMetadataAsync(sourceUrl, cancellationToken);

        return new ContentMetadata(
            Title: metadata.Title ?? sourceUrl,
            Url: metadata.WebpageUrl ?? sourceUrl,
            SourceType: metadata.Extractor ?? SourceType,
            Author: metadata.Channel ?? metadata.Uploader,
            PublishedDate: null,
            Duration: metadata.Duration,
            ThumbnailUrl: metadata.Thumbnail
        );
    }

    /// <inheritdoc/>
    public async Task<string> GetContent(string sourceUrl, CancellationToken cancellationToken = default)
    {
        var transcriptResponse = await _supadataService.GetTranscriptAsync(sourceUrl, cancellationToken: cancellationToken);
        return transcriptResponse.Content;
    }
}
