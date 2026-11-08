namespace YouSummarize.Dto.Models
{
    public class PromptDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
    }
}
