using System.ComponentModel.DataAnnotations;

namespace YouSummarize.Data.EntityModel
{
    public class AiConsumptionEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int? InputToken { get; set; }
        public int? OutputToken { get; set; }
        public decimal InputPrice { get; set; }
        public decimal OutputPrice { get; set; }

        [MaxLength(30)]
        public string UseCase { get; set; } = string.Empty;
        [MaxLength(30)]
        public string AiModelName { get; set; } = string.Empty;
    }
}
