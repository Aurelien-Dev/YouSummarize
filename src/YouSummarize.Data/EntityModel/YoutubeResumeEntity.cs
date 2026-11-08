namespace YouSummarize.Data.EntityModel
{
    public class YoutubeResumeEntity
    {
        public required int Id { get; set; }
        public required string Resume { get; set; }
        public required string Url { get; set; }
        public required string Title { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // V2 Features
        /// <summary>
        /// Statut de traitement du résumé (À consulter, Consulté, Retenu, Écarté).
        /// </summary>
        public SummaryStatus Status { get; set; } = SummaryStatus.ToReview;

        // Travel relationship
        public int? TravelId { get; set; }
        public TravelEntity? Travel { get; set; }

        // Categories relationship (many-to-many)
        /// <summary>
        /// Catégories associées à ce résumé YouTube.
        /// </summary>
        public ICollection<CategoryEntity> Categories { get; set; } = [];

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
        /// URL de la miniature de la vidéo YouTube (nullable si non disponible).
        /// </summary>
        public string? ThumbnailUrl { get; set; }
    }
}
