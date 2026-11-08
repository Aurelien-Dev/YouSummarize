using Microsoft.AspNetCore.Identity;
using YouSummarize.Data.EntityModel;
using YouSummarize.Data.Repository.IRepositories;

namespace YouSummarize.Data.Repository.Repositories;

public class UserRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : IUserRepository
{
    public async Task AddFavoriteRecipeAsync(string userEmail, RecipeEntity recipe)
    {
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
            return;

        user.Favorites.Add(recipe);
        await userManager.UpdateAsync(user);
    }

    public async Task AssignRoleAsync(string userEmail, string role)
    {
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
            return;

        var roleExist = await roleManager.RoleExistsAsync(role);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }

    public async Task RemoveRoleAsync(string userEmail, string role)
    {
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null || !await userManager.IsInRoleAsync(user, role))
            return;

        await userManager.RemoveFromRoleAsync(user, role);
    }

    public async Task<List<(ApplicationUser User, List<string> Roles)>> GetAllUsersWithRolesAsync()
    {
        var users = userManager.Users.ToList();
        var usersWithRoles = new List<(ApplicationUser, List<string>)>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            usersWithRoles.Add((user, roles.ToList()));
        }

        return usersWithRoles;
    }

    public async Task<IdentityResult> DeleteUserAsync(string userEmail)
    {
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        return await userManager.DeleteAsync(user);
    }
}
