using YouSummarize.Dto.Models;

namespace YouSummarize.Data.Repository.IRepositories;

/// <summary>
/// Interface pour la gestion des catégories de résumés YouTube.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Récupère toutes les catégories.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de toutes les catégories.</returns>
    Task<List<CategoryDto>> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une catégorie par son identifiant.
    /// </summary>
    /// <param name="id">L'identifiant de la catégorie.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>La catégorie correspondante ou null si non trouvée.</returns>
    Task<CategoryDto?> GetById(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute une nouvelle catégorie.
    /// </summary>
    /// <param name="category">La catégorie à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>La catégorie ajoutée avec son identifiant.</returns>
    Task<CategoryDto> Add(CategoryDto category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour une catégorie existante.
    /// </summary>
    /// <param name="category">La catégorie à mettre à jour.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la mise à jour a réussi, false sinon.</returns>
    Task<bool> Update(CategoryDto category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime une catégorie et toutes ses associations avec les résumés.
    /// </summary>
    /// <param name="id">L'identifiant de la catégorie à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la suppression a réussi, false sinon.</returns>
    Task<bool> Delete(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si une catégorie est utilisée par au moins un résumé.
    /// </summary>
    /// <param name="id">L'identifiant de la catégorie.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la catégorie est utilisée, false sinon.</returns>
    Task<bool> IsUsedByAnySummary(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère le nombre de résumés associés à chaque catégorie.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Dictionnaire avec l'ID de catégorie comme clé et le nombre de résumés comme valeur.</returns>
    Task<Dictionary<int, int>> GetSummariesCountByCategory(CancellationToken cancellationToken = default);
}
