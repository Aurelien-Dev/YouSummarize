using YouSummarize.Api;
using YouSummarize.Dto.Models;

using YouSummarize.Managers.AiGenerators.Models;

namespace YouSummarize.Managers.AiGenerators;

/// <summary>
/// Interface pour la gestion des fonctionnalités d'intelligence artificielle.
/// Provides AI-generated summaries for YouTube transcripts.
/// </summary>
public interface IAiManager
{
    /// <summary>
    /// Génère un résumé d'une vidéo YouTube à partir des données fournies.
    /// </summary>
    /// <param name="transcript">The video transcript to summarize.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le résumé généré de la vidéo YouTube.</returns>
    Task<YoutubeResumeDto> GetYoutubeResume(string transcript, CancellationToken cancellationToken = default);
}