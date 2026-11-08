using System.ComponentModel.DataAnnotations;
using YouSummarize.Data.EntityModel.Blocks;

using System.Security.Cryptography;
using System.Text;

namespace YouSummarize.Data.EntityModel;

public class RecipeEntity
{
    public int Id { get; set; }
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string InformationPreparation { get; set; } = string.Empty;
    public int PrepTime { get; set; }
    public int CookingTime { get; set; }
    public int RestTime { get; set; }
    public int Portion { get; set; }
    public DateOnly CreatedDate { get; set; }
    public DateOnly? UpdatedDate { get; set; }

    [MaxLength(200)]
    public string Tags { get; set; } = string.Empty;

    public string? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }

    public ICollection<BlockBaseEntity> BlocksInstructions { get; set; } = [];

    public ICollection<ApplicationUser> FavoritedByUsers { get; set; } = [];

}
