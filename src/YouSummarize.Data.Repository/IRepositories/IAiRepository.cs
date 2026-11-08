using YouSummarize.Dto.Models;

namespace YouSummarize.Data.Repository.IRepositories;

/// <summary>
/// Interface pour la gestion des opérations liées à l'intelligence artificielle.
/// Gère les prompts et la consommation des ressources AI.
/// </summary>
public interface IAiRepository
{
    /// <summary>
    /// Ajoute un nouveau prompt dans la base de données.
    /// </summary>
    /// <param name="prompt">Le prompt à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si l'ajout a réussi, sinon False.</returns>
    Task<bool> AddPrompt(PromptDto? prompt, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère un prompt spécifique par son nom.
    /// </summary>
    /// <param name="promptName">Le nom du prompt à récupérer.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le prompt correspondant au nom spécifié.</returns>
    Task<PromptDto> GetPrompt(string promptName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère tous les prompts disponibles.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de tous les prompts.</returns>
    Task<List<PromptDto>> GetPrompt(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Enregistre la consommation de ressources AI pour le suivi et la facturation.
    /// </summary>
    /// <param name="aiConsumptionDto">Les informations de consommation à enregistrer.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Une tâche représentant l'opération asynchrone.</returns>
    Task ReportConsumption(AiConsumptionDto aiConsumptionDto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Met à jour un prompt existant.
    /// </summary>
    /// <param name="prompt">Le prompt avec les informations mises à jour.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Une tâche représentant l'opération asynchrone.</returns>
    Task UpdatePrompt(PromptDto? prompt, CancellationToken cancellationToken = default);
}