using System.Text.Json.Serialization;

namespace YouSummarize.Services.Models;

/// <summary>
/// Représente la réponse de l'API YouTube oEmbed pour récupérer les métadonnées d'une vidéo.
/// </summary>
public class YoutubeOEmbedResponse
{
    /// <summary>
    /// Le titre de la vidéo YouTube.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Le nom de l'auteur de la vidéo.
    /// </summary>
    [JsonPropertyName("author_name")]
    public string? AuthorName { get; set; }

    /// <summary>
    /// L'URL de la chaîne de l'auteur.
    /// </summary>
    [JsonPropertyName("author_url")]
    public string? AuthorUrl { get; set; }

    /// <summary>
    /// Le type de contenu (généralement "video").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// La largeur de la vidéo en pixels.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// La hauteur de la vidéo en pixels.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// La hauteur de la vidéo en pixels.
    /// </summary>
    [JsonPropertyName("thumbnail_url")]
    public string ThumbnailUrl { get; set; }
}
