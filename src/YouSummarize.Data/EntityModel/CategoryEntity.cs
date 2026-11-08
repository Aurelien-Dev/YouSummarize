using System.ComponentModel.DataAnnotations;

namespace YouSummarize.Data.EntityModel;

/// <summary>
/// Entité représentant une catégorie personnalisable pour classifier les résumés YouTube.
/// </summary>
public class CategoryEntity
{
    /// <summary>
    /// Identifiant unique de la catégorie.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nom de la catégorie (ex: "Gastronomie", "Transport", "Hébergement").
    /// </summary>
    [MaxLength(200)]
    public required string Name { get; set; }

    /// <summary>
    /// Couleur optionnelle de la catégorie pour l'affichage (format hex ou nom de couleur).
    /// </summary>
    [MaxLength(20)]
    public string? Color { get; set; }

    /// <summary>
    /// Date de création de la catégorie.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property : résumés YouTube associés à cette catégorie.
    /// </summary>
    public ICollection<YoutubeResumeEntity> YoutubeSummaries { get; set; } = [];
}
