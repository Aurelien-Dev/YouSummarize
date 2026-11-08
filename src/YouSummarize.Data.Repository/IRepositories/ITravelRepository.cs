using YouSummarize.Dto.Models;

namespace YouSummarize.Data.Repository.IRepositories;

/// <summary>
/// Interface pour la gestion des voyages.
/// Fournit des méthodes pour créer, modifier, supprimer et récupérer des voyages.
/// </summary>
public interface ITravelRepository
{
    /// <summary>
    /// Récupère tous les voyages.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de tous les voyages.</returns>
    Task<List<TravelDto>> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère un voyage par son identifiant.
    /// </summary>
    /// <param name="id">L'identifiant du voyage.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le voyage correspondant ou null si non trouvé.</returns>
    Task<TravelDto?> GetById(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute un nouveau voyage.
    /// </summary>
    /// <param name="travel">Le voyage à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le voyage ajouté avec son identifiant.</returns>
    Task<TravelDto> Add(TravelDto travel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour un voyage existant.
    /// </summary>
    /// <param name="travel">Le voyage à mettre à jour.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la mise à jour a réussi, false sinon.</returns>
    Task<bool> Update(TravelDto travel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un voyage par son identifiant.
    /// Les résumés YouTube associés seront conservés et remis en "Non classés" (TravelId = null).
    /// </summary>
    /// <param name="id">L'identifiant du voyage à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la suppression a réussi, false sinon.</returns>
    Task<bool> Delete(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère le nombre de résumés YouTube associés à chaque voyage.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Dictionnaire avec l'identifiant du voyage comme clé et le nombre de résumés comme valeur.</returns>
    Task<Dictionary<int, int>> GetSummariesCountByTravel(CancellationToken cancellationToken = default);
}
