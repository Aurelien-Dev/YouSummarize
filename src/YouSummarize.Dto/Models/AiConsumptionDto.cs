using System.ComponentModel.DataAnnotations;

namespace YouSummarize.Dto.Models
{
    public class AiConsumptionDto
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int? InputToken { get; set; }
        public int? OutputToken { get; set; }
        public Decimal InputPrice { get; set; }
        public Decimal OutputPrice { get; set; }

        public string UseCase { get; set; } = string.Empty;
        public string AiModelName { get; set; } = string.Empty;
    }
}
