using System.ComponentModel.DataAnnotations;

namespace YouSummarize.Data.EntityModel;

public class TagEntity
{
    public int Id { get; set; }
    [MaxLength(40)]
    public string TagName { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
}
