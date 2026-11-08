using System.ComponentModel.DataAnnotations;

namespace YouSummarize.Data.EntityModel;

public class TravelEntity
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsArchived { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<YoutubeResumeEntity> YoutubeSummaries { get; set; } = [];
}
