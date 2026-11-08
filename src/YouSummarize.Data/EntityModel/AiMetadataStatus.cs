namespace YouSummarize.Data.EntityModel;

/// <summary>
/// Statut de l'analyse des métadonnées IA d'un résumé YouTube.
/// </summary>
public enum AiMetadataStatus
{
    /// <summary>
    /// Aucune analyse IA n'a été effectuée (valeur par défaut pour les anciens résumés).
    /// </summary>
    NotAnalyzed = 0,

    /// <summary>
    /// L'analyse IA s'est terminée avec succès.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// L'analyse IA a échoué (erreur de désérialisation, validation, etc.).
    /// </summary>
    Failed = 2
}
