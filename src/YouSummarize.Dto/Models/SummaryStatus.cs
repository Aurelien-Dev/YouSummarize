namespace YouSummarize.Dto.Models;

/// <summary>
/// Statut de traitement d'un résumé YouTube.
/// </summary>
public enum SummaryStatus
{
    /// <summary>
    /// À consulter - Statut par défaut pour un nouveau résumé.
    /// </summary>
    ToReview = 0,

    /// <summary>
    /// Consulté - Le résumé a été lu.
    /// </summary>
    Reviewed = 1,

    /// <summary>
    /// Retenu - Le résumé est marqué comme intéressant pour un futur itinéraire.
    /// </summary>
    Selected = 2,

    /// <summary>
    /// Écarté - Le résumé n'est pas pertinent.
    /// </summary>
    Rejected = 3
}
