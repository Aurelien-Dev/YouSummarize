using YouSummarize.Data.EntityModel;
using YouSummarize.Dto.Models;

namespace YouSummarize.Data.Repository.IRepositories;

/// <summary>
/// Interface pour la gestion des résumés de vidéos YouTube.
/// Fournit des méthodes pour ajouter et récupérer des résumés de vidéos.
/// </summary>
public interface IYoutubeRepository
{
    /// <summary>
    /// Ajoute un nouveau résumé de vidéo YouTube.
    /// </summary>
    /// <param name="youtubeSummary">Le résumé de vidéo à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le résumé ajouté avec son identifiant.</returns>
    Task<YoutubeResumeDto> AddSummary(YoutubeResumeDto youtubeSummary, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère tous les résumés de vidéos YouTube disponibles.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de tous les résumés de vidéos.</returns>
    Task<List<YoutubeResumeDto>> GetAllSummary(CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un résumé de vidéo YouTube par son identifiant.
    /// </summary>
    /// <param name="id">L'identifiant du résumé à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la suppression a réussi, false sinon.</returns>
    Task<bool> DeleteSummary(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour le voyage associé à un résumé YouTube.
    /// </summary>
    /// <param name="summaryId">L'identifiant du résumé à mettre à jour.</param>
    /// <param name="travelId">L'identifiant du voyage à associer (null pour "Non classé").</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la mise à jour a réussi, false sinon.</returns>
    Task<bool> UpdateSummaryTravel(int summaryId, int? travelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour le statut d'un résumé YouTube.
    /// </summary>
    /// <param name="summaryId">L'identifiant du résumé à mettre à jour.</param>
    /// <param name="status">Le nouveau statut.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la mise à jour a réussi, false sinon.</returns>
    Task<bool> UpdateStatus(int summaryId, Dto.Models.SummaryStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour les catégories associées à un résumé YouTube.
    /// </summary>
    /// <param name="summaryId">L'identifiant du résumé à mettre à jour.</param>
    /// <param name="categoryIds">Liste des identifiants de catégories à associer.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la mise à jour a réussi, false sinon.</returns>
    Task<bool> UpdateCategories(int summaryId, List<int> categoryIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour le titre d'un résumé YouTube.
    /// </summary>
    /// <param name="summaryId">L'identifiant du résumé à mettre à jour.</param>
    /// <param name="title">Le nouveau titre.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la mise à jour a réussi, false sinon.</returns>
    Task<bool> UpdateTitle(int summaryId, string title, CancellationToken cancellationToken = default);
}
