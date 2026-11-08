using YouSummarize.Services.Models;

namespace YouSummarize.ServicesExternal;

/// <summary>
/// Interface pour les appels à l'API Supadata.
/// </summary>
public interface ISupadataService
{
    /// <summary>
    /// Récupère le transcript d'une vidéo YouTube via l'API Supadata.
    /// </summary>
    /// <param name="videoId">L'identifiant de la vidéo YouTube.</param>
    /// <param name="cancellationToken">Token d'annulation pour l'opération asynchrone.</param>
    /// <returns>La réponse de l'API Supadata contenant le transcript et les métadonnées.</returns>
    Task<SupadataTranscriptResponse> GetYoutubeTranscriptAsync(string videoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère le transcript d'un contenu via l'endpoint générique Supadata.
    /// Supporte YouTube, TikTok, Instagram, X (Twitter), Facebook et fichiers publics.
    /// </summary>
    /// <param name="url">L'URL complète du contenu (doit être encodée si nécessaire).</param>
    /// <param name="lang">Code de langue ISO 639-1 préféré (défaut : "en").</param>
    /// <param name="mode">Mode de transcription : "native", "generate" ou "auto" (défaut : "auto").</param>
    /// <param name="cancellationToken">Token d'annulation pour l'opération asynchrone.</param>
    /// <returns>La réponse de l'API Supadata contenant le transcript.</returns>
    Task<SupadataTranscriptResponse> GetTranscriptAsync(string url, string lang = "en", string mode = "auto", CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les métadonnées d'un contenu via l'API Supadata.
    /// Supporte YouTube, TikTok, Instagram, X (Twitter) et Facebook.
    /// </summary>
    /// <param name="url">L'URL complète du contenu.</param>
    /// <param name="cancellationToken">Token d'annulation pour l'opération asynchrone.</param>
    /// <returns>La réponse de l'API Supadata contenant les métadonnées.</returns>
    Task<SupadataMetadataResponse> GetMetadataAsync(string url, CancellationToken cancellationToken = default);
}
