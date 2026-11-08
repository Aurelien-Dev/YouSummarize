using Microsoft.AspNetCore.Identity;
using YouSummarize.Data.EntityModel;

namespace YouSummarize.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string AccountName { get; set; } = string.Empty;

    public ICollection<RecipeEntity> Favorites { get; set; } = null!;

    public ICollection<RecipeEntity> CreatedRecipes { get; set; } = [];

    /// <summary>
    /// Date et heure de la dernière connexion de l'utilisateur
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Token d'approbation en attente pour les nouveaux utilisateurs
    /// </summary>
    public string? PendingApprovalToken { get; set; }

    /// <summary>
    /// Date d'expiration du token d'approbation
    /// </summary>
    public DateTime? PendingApprovalTokenExpires { get; set; }
}
