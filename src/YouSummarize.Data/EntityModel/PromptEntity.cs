using System.ComponentModel.DataAnnotations;

namespace YouSummarize.Data.EntityModel
{
    public class PromptEntity
    {
        public int Id { get; set; }
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
    }
}
