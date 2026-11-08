using YouSummarize.Services.Models;
using YouSummarize.ServicesExternal;

namespace YouSummarize.Services.TravelPlanner.Sources;

/// <summary>
/// Service pour l'extraction de contenu depuis YouTube.
/// Gère l'extraction des métadonnées via yt-dlp et des transcripts via l'API Supadata.
/// Normalise automatiquement les URLs shorts en format /watch?v= standard pour simplifier le traitement.
/// </summary>
public class YoutubeSourceService : IContentSourceService
{
    private readonly ISupadataService _supadataService;
    private readonly YtDlpAudioExtractor _ytDlpAudioExtractor;

    /// <inheritdoc/>
    public string SourceType => "YouTube";

    /// <summary>
    /// Indique si ce service peut traiter l'URL donnée.
    /// Supporte les formats youtube.com et youtu.be.
    /// </summary>
    public bool CanHandle(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        return uri.Host.Contains("youtube.com") || uri.Host == "youtu.be";
    }

    /// <summary>
    /// Initialise une nouvelle instance du service YouTube.
    /// </summary>
    /// <param name="supadataService">Service pour les appels à l'API Supadata.</param>
    /// <param name="ytDlpAudioExtractor">Extracteur yt-dlp pour les métadonnées.</param>
    public YoutubeSourceService(ISupadataService supadataService, YtDlpAudioExtractor ytDlpAudioExtractor)
    {
        _supadataService = supadataService;
        _ytDlpAudioExtractor = ytDlpAudioExtractor;
    }

    /// <inheritdoc/>
    public async Task<ContentMetadata> ExtractMetadata(string sourceUrl, CancellationToken cancellationToken = default)
    {
        var normalizedUrl = NormalizeYoutubeUrl(sourceUrl);

        var metadata = await _ytDlpAudioExtractor.GetMetadataAsync(normalizedUrl, cancellationToken);

        return new ContentMetadata(
            Title: metadata.Title ?? $"Vidéo YouTube {normalizedUrl}",
            Url: metadata.WebpageUrl ?? normalizedUrl,
            SourceType: SourceType,
            Author: metadata.Channel ?? metadata.Uploader,
            PublishedDate: null,
            Duration: metadata.Duration,
            ThumbnailUrl: metadata.Thumbnail
        );
    }

    /// <inheritdoc/>
    public async Task<string> GetContent(string sourceUrl, CancellationToken cancellationToken = default)
    {
        var normalizedUrl = NormalizeYoutubeUrl(sourceUrl);

        var transcriptResponse = await _supadataService.GetTranscriptAsync(normalizedUrl, cancellationToken: cancellationToken);
        return transcriptResponse.Content;
    }

    /// <summary>
    /// Valide et normalise une URL YouTube.
    /// Les shorts sont convertis en format /watch?v= standard pour simplifier le traitement.
    /// Formats supportés : /watch?v=, /shorts/ (converti), /embed/, et youtu.be/
    /// </summary>
    /// <param name="url">L'URL YouTube à normaliser.</param>
    /// <returns>L'URL YouTube normalisée (shorts convertis en /watch?v=, autres formats préservés).</returns>
    /// <exception cref="ArgumentException">Lancée si l'URL n'est pas une URL YouTube valide ou reconnue.</exception>
    private static string NormalizeYoutubeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("L'URL ne peut pas être vide.", nameof(url));
        }

        try
        {
            var uri = new Uri(url);

            // Format: https://www.youtube.com/watch?v=VIDEO_ID ou https://youtube.com/watch?v=VIDEO_ID
            if ((uri.Host.Contains("youtube.com") || uri.Host.Contains("www.youtube.com")) && uri.AbsolutePath == "/watch")
            {
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var videoId = query["v"];

                if (!string.IsNullOrWhiteSpace(videoId))
                {
                    return url; // Déjà au format standard, garder tel quel
                }
            }

            // Format: https://youtu.be/VIDEO_ID
            if (uri.Host == "youtu.be" && uri.AbsolutePath.Length > 1)
            {
                return url; // Format court valide, garder tel quel
            }

            // Format: https://www.youtube.com/embed/VIDEO_ID
            if ((uri.Host.Contains("youtube.com") || uri.Host.Contains("www.youtube.com")) && uri.AbsolutePath.StartsWith("/embed/"))
            {
                var videoId = uri.AbsolutePath.Replace("/embed/", "");
                if (!string.IsNullOrWhiteSpace(videoId))
                {
                    return url; // Format embed valide, garder tel quel
                }
            }

            // Format: https://www.youtube.com/shorts/VIDEO_ID → Normaliser en /watch?v=
            if ((uri.Host.Contains("youtube.com") || uri.Host.Contains("www.youtube.com")) && uri.AbsolutePath.StartsWith("/shorts/"))
            {
                var videoId = uri.AbsolutePath.Replace("/shorts/", "");
                if (!string.IsNullOrWhiteSpace(videoId))
                {
                    // Convertir en format standard /watch?v=
                    return $"https://www.youtube.com/watch?v={videoId}";
                }
            }

            throw new ArgumentException($"L'URL YouTube n'est pas dans un format reconnu : {url}", nameof(url));
        }
        catch (UriFormatException)
        {
            throw new ArgumentException($"L'URL fournie n'est pas valide : {url}", nameof(url));
        }
    }

    /// <summary>
    /// Extrait l'identifiant de la vidéo YouTube depuis une URL normalisée.
    /// Cette méthode suppose que l'URL a déjà été normalisée par NormalizeYoutubeUrl.
    /// Les shorts ont déjà été convertis en /watch?v= à ce stade.
    /// </summary>
    /// <param name="url">L'URL YouTube normalisée.</param>
    /// <returns>L'identifiant de la vidéo (11 caractères).</returns>
    private static string ExtractVideoIdFromUrl(string url)
    {
        var uri = new Uri(url);

        // Format: https://www.youtube.com/watch?v=VIDEO_ID (inclut les shorts normalisés)
        if ((uri.Host.Contains("youtube.com") || uri.Host.Contains("www.youtube.com")) && uri.AbsolutePath == "/watch")
        {
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["v"]!;
        }

        // Format: https://youtu.be/VIDEO_ID
        if (uri.Host == "youtu.be")
        {
            return uri.AbsolutePath.TrimStart('/');
        }

        // Format: https://www.youtube.com/embed/VIDEO_ID
        if ((uri.Host.Contains("youtube.com") || uri.Host.Contains("www.youtube.com")) && uri.AbsolutePath.StartsWith("/embed/"))
        {
            return uri.AbsolutePath.Replace("/embed/", "");
        }

        // Ne devrait jamais arriver si l'URL a été normalisée
        throw new InvalidOperationException($"Impossible d'extraire l'ID de l'URL normalisée : {url}");
    }
}