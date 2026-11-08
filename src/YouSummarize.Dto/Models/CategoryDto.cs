namespace YouSummarize.Dto.Models;

/// <summary>
/// DTO représentant une catégorie pour classifier les résumés YouTube.
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
