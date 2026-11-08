using Microsoft.AspNetCore.Identity;
using YouSummarize.Data;
using YouSummarize.Services;

namespace YouSummarize.Api;

/// <summary>
/// Extension methods pour enregistrer les endpoints d'approbation utilisateur
/// </summary>
public static class UserApprovalEndpoints
{
    /// <summary>
    /// Enregistre les endpoints d'approbation utilisateur
    /// </summary>
    public static IEndpointRouteBuilder MapUserApprovalEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoint pour approuver un utilisateur via lien email
        app.MapGet("/api/users/approve/{userId}", HandleUserApproval)
            .DisableAntiforgery()
            .WithName("ApproveUser");

        return app;
    }

    /// <summary>
    /// Gère l'approbation d'un utilisateur via token sécurisé
    /// </summary>
    private static async Task<IResult> HandleUserApproval(string userId, HttpContext context, UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager, ApprovalTokenService tokenService, ILogger<Program> logger)
    {
        try
        {
            // Récupérer les paramètres de la query string
            var token = context.Request.Query["token"].ToString();
            var expiresStr = context.Request.Query["expires"].ToString();

            // Validation des paramètres
            if (string.IsNullOrWhiteSpace(token))
            {
                logger.LogWarning("Approval attempt with missing token for user {UserId}", userId);
                return Results.Redirect("/approval-error?reason=missing-token");
            }

            if (string.IsNullOrWhiteSpace(expiresStr) || !long.TryParse(expiresStr, out var expiresTimestamp))
            {
                logger.LogWarning("Approval attempt with invalid expires parameter for user {UserId}", userId);
                return Results.Redirect("/approval-error?reason=invalid-expires");
            }

            // Récupérer l'utilisateur
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("Approval attempt for non-existent user {UserId}", userId);
                return Results.Redirect("/approval-error?reason=user-not-found");
            }

            // Vérifier si l'utilisateur a déjà un rôle (déjà approuvé)
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Count > 0)
            {
                logger.LogInformation("User {UserId} ({Email}) already has roles: {Roles}", userId, user.Email, string.Join(", ", roles));
                return Results.Redirect("/approval-success?status=already-approved");
            }

            // Vérifier si le token correspond à celui stocké
            if (string.IsNullOrWhiteSpace(user.PendingApprovalToken))
            {
                logger.LogWarning("No pending approval token found for user {UserId}", userId);
                return Results.Redirect("/approval-error?reason=no-pending-token");
            }

            if (user.PendingApprovalToken != token)
            {
                logger.LogWarning("Token mismatch for user {UserId}", userId);
                return Results.Redirect("/approval-error?reason=token-mismatch");
            }

            // Vérifier si le token a déjà été utilisé (PendingApprovalTokenExpires réinitialisé)
            if (user.PendingApprovalTokenExpires == null)
            {
                logger.LogWarning("Token already used for user {UserId}", userId);
                return Results.Redirect("/approval-error?reason=token-already-used");
            }

            // Valider le token avec le service
            if (!tokenService.ValidateToken(userId, user.Email ?? string.Empty, token, expiresTimestamp))
            {
                logger.LogWarning("Invalid or expired token for user {UserId}", userId);
                return Results.Redirect("/approval-error?reason=invalid-token");
            }

            // Vérifier que le rôle "Reader" existe, sinon le créer
            if (!await roleManager.RoleExistsAsync("Reader"))
            {
                logger.LogInformation("Creating 'Reader' role as it doesn't exist");
                await roleManager.CreateAsync(new IdentityRole("Reader"));
            }

            // Attribuer le rôle "Reader" à l'utilisateur
            var addRoleResult = await userManager.AddToRoleAsync(user, "Reader");
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to add Reader role to user {UserId}: {Errors}", userId, errors);
                return Results.Redirect("/approval-error?reason=role-assignment-failed");
            }

            // Marquer le token comme utilisé
            user.PendingApprovalToken = null;
            user.PendingApprovalTokenExpires = null;
            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                logger.LogError("Failed to update user {UserId} after approval", userId);
                // Ne pas bloquer l'approbation si la mise à jour échoue (le rôle est déjà attribué)
            }

            logger.LogInformation("User {UserId} ({Email}) approved successfully and granted Reader role", userId, user.Email);

            return Results.Redirect("/approval-success");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during user approval for {UserId}", userId);
            return Results.Redirect("/approval-error?reason=server-error");
        }
    }
}
