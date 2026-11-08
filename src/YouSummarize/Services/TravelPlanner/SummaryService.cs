using YouSummarize.Api;
using YouSummarize.Data.Repository.IRepositories;
using YouSummarize.Data.Repository.Repositories;
using YouSummarize.Dto.Models;
using YouSummarize.Managers.AiGenerators;
using YouSummarize.Services.TravelPlanner.Sources;

namespace YouSummarize.Services.TravelPlanner;

/// <summary>
/// Service orchestrateur pour la gestion des résumés de contenu.
/// Coordonne l'extraction de contenu, la génération AI, et les opérations de base de données.
/// </summary>
public class SummaryService : ISummaryService
{
    private readonly ContentSourceResolver _contentSourceResolver;
    private readonly IAiManager _aiManager;
    private readonly IYoutubeRepository _youtubeRepository;

    /// <summary>
    /// Initialise une nouvelle instance du service de résumés.
    /// </summary>
    public SummaryService(
        ContentSourceResolver contentSourceResolver,
        IAiManager aiManager,
        IYoutubeRepository youtubeRepository)
    {
        _contentSourceResolver = contentSourceResolver;
        _aiManager = aiManager;
        _youtubeRepository = youtubeRepository;
    }

    /// <inheritdoc/>
    public async Task<YoutubeResumeDto> CreateSummaryFromYoutube(string youtubeUrl, int? travelId = null, CancellationToken cancellationToken = default)
    {
        // 1. Extraire les métadonnées (le resolver choisit le service adapté avec fallback)
        var metadata = await _contentSourceResolver.ExtractMetadata(youtubeUrl, cancellationToken);

        // 2. Récupérer le transcript (réutilise l'URL normalisée depuis les métadonnées)
        var transcript = await _contentSourceResolver.GetContent(metadata.Url, cancellationToken);

        // 3. Créer l'objet de requête pour l'AI Manager
        var youtubeSummaryRequest = new YoutubeSummaryJson
        {
            Transcript = transcript,
            Url = metadata.Url,
            Title = metadata.Title
        };

        // 4. Générer le résumé via l'AI Manager (qui gère aussi la sauvegarde en base)
        var youtubeResume = await _aiManager.GetYoutubeResume(transcript, cancellationToken);

        youtubeResume.Title = metadata.Title;
        youtubeResume.TravelId = travelId;
        youtubeResume.Url = metadata.Url;
        youtubeResume.ThumbnailUrl = metadata.ThumbnailUrl;

        // 5. Sauvegarder en base de données
        await _youtubeRepository.AddSummary(youtubeResume, cancellationToken);

        // 6. Retourner le résumé complet
        return youtubeResume;
    }

    /// <inheritdoc/>
    public async Task<List<YoutubeResumeDto>> GetAllSummaries(CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.GetAllSummary(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<YoutubeResumeDto?> GetSummaryById(int id, CancellationToken cancellationToken = default)
    {
        var allSummaries = await _youtubeRepository.GetAllSummary(cancellationToken);
        return allSummaries.FirstOrDefault(s => s.Id == id);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteSummary(int id, CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.DeleteSummary(id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateSummaryTravel(int summaryId, int? travelId, CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.UpdateSummaryTravel(summaryId, travelId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateStatus(int summaryId, SummaryStatus status, CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.UpdateStatus(summaryId, status, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateCategories(int summaryId, List<int> categoryIds, CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.UpdateCategories(summaryId, categoryIds, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateTitle(int summaryId, string title, CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.UpdateTitle(summaryId, title, cancellationToken);
    }
}
