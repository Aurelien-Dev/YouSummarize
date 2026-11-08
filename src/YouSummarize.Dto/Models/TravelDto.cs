namespace YouSummarize.Dto.Models;

public class TravelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
