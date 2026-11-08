namespace YouSummarize.Services.Models;

/// <summary>
/// Représente la réponse de l'API Supadata pour la récupération d'un transcript YouTube.
/// </summary>
public class SupadataTranscriptResponse
{
    /// <summary>
    /// Le contenu du transcript de la vidéo YouTube.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Le code de langue ISO 639-1 du transcript retourné.
    /// </summary>
    public string? Lang { get; set; }
}
