using YouSummarize.Dto.Models;

namespace YouSummarize.Services.TravelPlanner;

/// <summary>
/// Service pour la gestion des résumés de contenu (YouTube, articles, podcasts, etc.)
/// Orchestre l'extraction de contenu, la génération de résumés AI, et les opérations CRUD.
/// </summary>
public interface ISummaryService
{
    /// <summary>
    /// Crée un résumé complet à partir d'une URL YouTube.
    /// Orchestre l'extraction du transcript, la génération du résumé AI, et la sauvegarde en base.
    /// </summary>
    /// <param name="youtubeUrl">L'URL complète de la vidéo YouTube.</param>
    /// <param name="travelId">Identifiant optionnel du voyage à associer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le résumé généré avec toutes les métadonnées.</returns>
    Task<YoutubeResumeDto> CreateSummaryFromYoutube(string youtubeUrl, int? travelId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère tous les résumés disponibles.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste de tous les résumés.</returns>
    Task<List<YoutubeResumeDto>> GetAllSummaries(CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère un résumé par son identifiant.
    /// </summary>
    /// <param name="id">L'identifiant du résumé.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le résumé trouvé ou null.</returns>
    Task<YoutubeResumeDto?> GetSummaryById(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un résumé.
    /// </summary>
    /// <param name="id">L'identifiant du résumé à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si la suppression a réussi, false sinon.</returns>
    Task<bool> DeleteSummary(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour le voyage associé à un résumé.
    /// </summary>
    /// <param name="summaryId">L'identifiant du résumé.</param>
    /// <param name="travelId">L'identifiant du voyage (null pour "Non classé").</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si la mise à jour a réussi, false sinon.</returns>
    Task<bool> UpdateSummaryTravel(int summaryId, int? travelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour le statut d'un résumé.
    /// </summary>
    /// <param name="summaryId">L'identifiant du résumé.</param>
    /// <param name="status">Le nouveau statut.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si la mise à jour a réussi, false sinon.</returns>
    Task<bool> UpdateStatus(int summaryId, SummaryStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour les catégories d'un résumé.
    /// </summary>
    /// <param name="summaryId">L'identifiant du résumé.</param>
    /// <param name="categoryIds">Liste des identifiants de catégories.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si la mise à jour a réussi, false sinon.</returns>
    Task<bool> UpdateCategories(int summaryId, List<int> categoryIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour le titre d'un résumé.
    /// </summary>
    /// <param name="summaryId">L'identifiant du résumé.</param>
    /// <param name="title">Le nouveau titre.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si la mise à jour a réussi, false sinon.</returns>
    Task<bool> UpdateTitle(int summaryId, string title, CancellationToken cancellationToken = default);
}
