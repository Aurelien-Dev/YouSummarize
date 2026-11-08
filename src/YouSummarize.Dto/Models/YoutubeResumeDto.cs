namespace YouSummarize.Dto.Models;

public class YoutubeResumeDto
{
    public int Id { get; set; }
    public required string Resume { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // V2 Features
    /// <summary>
    /// Statut de traitement du résumé.
    /// </summary>
    public SummaryStatus Status { get; set; } = SummaryStatus.ToReview;

    // Travel relationship
    public int? TravelId { get; set; }
    public string? TravelName { get; set; }

    // Categories relationship
    /// <summary>
    /// Liste des identifiants des catégories associées.
    /// </summary>
    public List<int> CategoryIds { get; set; } = [];

    /// <summary>
    /// Liste des noms des catégories associées (pour l'affichage).
    /// </summary>
    public List<string> CategoryNames { get; set; } = [];

    /// <summary>
    /// Liste des couleurs des catégories associées (pour l'affichage).
    /// </summary>
    public List<string?> CategoryColors { get; set; } = [];

    // V3.1 Features - AI Metadata Detection
    /// <summary>
    /// Nom du pays principal détecté par l'IA (nullable si non identifiable).
    /// </summary>
    public string? MainCountryName { get; set; }

    /// <summary>
    /// Code ISO 3166-1 alpha-2 du pays détecté (ex: "FR", "JP", "US").
    /// </summary>
    public string? MainCountryIsoCode { get; set; }

    /// <summary>
    /// Score de confiance de la détection du pays (entre 0 et 1).
    /// </summary>
    public double? MainCountryConfidence { get; set; }

    /// <summary>
    /// Statut de l'analyse IA des métadonnées.
    /// </summary>
    public AiMetadataStatus AiMetadataStatus { get; set; } = AiMetadataStatus.NotAnalyzed;

    /// <summary>
    /// Message d'erreur en cas d'échec de l'analyse IA.
    /// </summary>
    public string? AiMetadataError { get; set; }

    /// <summary>
    /// Date et heure de l'analyse IA des métadonnées.
    /// </summary>
    public DateTime? AiMetadataAnalyzedAt { get; set; }

    /// <summary>
    /// URL de la miniature de la vidéo YouTube (si disponible).
    /// </summary>
    public string? ThumbnailUrl { get; set; }
}
