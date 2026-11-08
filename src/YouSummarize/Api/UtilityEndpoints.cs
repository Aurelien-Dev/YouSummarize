using Microsoft.AspNetCore.Mvc;
using YouSummarize.Managers.AiGenerators;
using System.Text.Json;

namespace YouSummarize.Api;

/// <summary>
/// Extension methods pour enregistrer les endpoints utilitaires
/// </summary>
public static class UtilityEndpoints
{
    /// <summary>
    /// Enregistre tous les endpoints utilitaires
    /// </summary>
    public static IEndpointRouteBuilder MapUtilityEndpoints(this IEndpointRouteBuilder app)
    {
        // Note: YouTube summary endpoint has been moved to TravelPlannerEndpoints
        // No utility endpoints currently registered
        return app;
    }
}

/// <summary>
/// Modèle de requête pour le résumé YouTube
/// Note: This class is still used by AiManager and will be refactored in the future
/// </summary>
public class YoutubeSummaryJson
{
    public required string Transcript { get; set; }
    public required string Url { get; set; }
    public required string Title { get; set; }
}
