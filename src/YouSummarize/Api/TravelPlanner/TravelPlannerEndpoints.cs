using Microsoft.AspNetCore.Mvc;
using YouSummarize.Services.TravelPlanner;
using System.Text.Json;

namespace YouSummarize.Api.TravelPlanner;

/// <summary>
/// Extension methods pour enregistrer les endpoints TravelPlanner (YouSummarize)
/// </summary>
public static class TravelPlannerEndpoints
{
    /// <summary>
    /// Enregistre tous les endpoints TravelPlanner
    /// </summary>
    public static IEndpointRouteBuilder MapTravelPlannerEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoint pour créer un résumé YouTube
        app.MapPost("/api/travelplanner/summaries/youtube", HandleYoutubeSummary)
            .WithName("CreateYoutubeSummary")
            .WithTags("TravelPlanner");

        return app;
    }

    /// <summary>
    /// Gère la demande de création d'un résumé YouTube
    /// </summary>
    private static async Task<IResult> HandleYoutubeSummary(
        HttpRequest request,
        [FromServices] ISummaryService summaryService,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);
        var requestBody = JsonSerializer.Deserialize<YoutubeSummaryRequest>(body);

        if (requestBody is null || string.IsNullOrWhiteSpace(requestBody.Url))
        {
            return Results.BadRequest("Invalid request body. URL is required.");
        }

        try
        {
            var summary = await summaryService.CreateSummaryFromYoutube(
                requestBody.Url,
                requestBody.TravelId,
                cancellationToken);

            return Results.Ok(new { result = summary.Resume, summary });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return Results.Json(new { error = "Failed to fetch YouTube content", details = ex.Message }, statusCode: 502);
        }
        catch (Exception ex)
        {
            return Results.Json(new { error = "An error occurred while processing the request", details = ex.Message }, statusCode: 500);
        }
    }
}

/// <summary>
/// Modèle de requête pour la création d'un résumé YouTube
/// </summary>
public class YoutubeSummaryRequest
{
    /// <summary>
    /// L'URL de la vidéo YouTube
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// L'identifiant optionnel du voyage à associer
    /// </summary>
    public int? TravelId { get; set; }
}
