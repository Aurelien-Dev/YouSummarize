using Microsoft.AspNetCore.Identity;
using YouSummarize.Data;
using YouSummarize.Services;

namespace YouSummarize.Api
{
    /// <summary>
    /// Extension methods pour enregistrer les endpoints d'authentification
    /// </summary>
    public static class AuthEndpoints
    {
        /// <summary>
        /// Enregistre tous les endpoints d'authentification
        /// </summary>
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            // Endpoint pour le login
            app.MapPost("/auth/login", HandleLogin)
                .DisableAntiforgery()
                .WithName("AuthLogin");

            // Endpoint pour le register
            app.MapPost("/auth/register", HandleRegister)
                .DisableAntiforgery()
                .WithName("AuthRegister");

            // Endpoint pour le logout
            app.MapPost("/auth/logout", HandleLogout)
                .DisableAntiforgery()
                .WithName("AuthLogout");

            // Endpoint pour la mise à jour du profil
            app.MapPost("/auth/update-profile", HandleUpdateProfile)
                .DisableAntiforgery()
                .RequireAuthorization()
                .WithName("AuthUpdateProfile");

            return app;
        }

        /// <summary>
        /// Gère la connexion de l'utilisateur
        /// </summary>
        private static async Task<IResult> HandleLogin(HttpContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<Program> logger)
        {
            try
            {
                var form = await context.Request.ReadFormAsync();
                var email = form["email"].ToString();
                var password = form["password"].ToString();
                var rememberMeStr = form["rememberMe"].ToString();
                var returnUrl = form["returnUrl"].ToString();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    logger.LogWarning("Login attempt with empty credentials");
                    return Results.Redirect($"/login?error=empty");
                }

                bool rememberMe = !string.IsNullOrEmpty(rememberMeStr) &&
                                  (rememberMeStr.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                   rememberMeStr.Equals("on", StringComparison.OrdinalIgnoreCase));

                var result = await signInManager.PasswordSignInAsync(email, password, isPersistent: rememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    logger.LogInformation("User {Email} logged in successfully", email);

                    // Track last login date
                    try
                    {
                        var user = await userManager.FindByEmailAsync(email);
                        if (user != null)
                        {
                            user.LastLoginDate = DateTime.UtcNow;
                            await userManager.UpdateAsync(user);
                            logger.LogInformation("LastLoginDate updated for user {Email}", email);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to update LastLoginDate for user {Email}", email);
                        // Ne pas bloquer la connexion si le tracking échoue
                    }

                    // WORKAROUND: Pour Blazor Server avec cookies non-persistants
                    // Petit délai pour s'assurer que le cookie est écrit avant la redirection
                    // Cela évite une race condition où le circuit SignalR démarre avant que le cookie soit disponible
                    if (!rememberMe)
                    {
                        await Task.Delay(100); // 100ms de délai pour les cookies de session
                    }

                    var redirectTo = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
                    return Results.Redirect(redirectTo);
                }
                else if (result.RequiresTwoFactor)
                {
                    logger.LogWarning("User {Email} requires two-factor authentication", email);
                    return Results.Redirect("/login?error=2fa");
                }
                else if (result.IsLockedOut)
                {
                    logger.LogWarning("User {Email} account is locked out", email);
                    return Results.Redirect("/login?error=locked");
                }
                else
                {
                    logger.LogWarning("Invalid login attempt for {Email}", email);
                    return Results.Redirect("/login?error=invalid");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception during login");
                return Results.Redirect("/login?error=server");
            }
        }

        /// <summary>
        /// Gère l'inscription d'un nouvel utilisateur
        /// </summary>
        private static async Task<IResult> HandleRegister(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApprovalTokenService tokenService,
            UserApprovalEmailService emailService,
            ILogger<Program> logger)
        {
            try
            {
                var form = await context.Request.ReadFormAsync();
                var accountName = form["accountName"].ToString();
                var email = form["email"].ToString();
                var password = form["password"].ToString();
                var confirmPassword = form["confirmPassword"].ToString();
                var returnUrl = form["returnUrl"].ToString();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    logger.LogWarning("Register attempt with empty credentials");
                    return Results.Redirect($"/register?error=empty");
                }

                if (password != confirmPassword)
                {
                    logger.LogWarning("Register attempt with mismatched passwords for {Email}", email);
                    return Results.Redirect($"/register?error=mismatch");
                }

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    AccountName = string.IsNullOrWhiteSpace(accountName) ? email : accountName
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    logger.LogInformation("User {Email} created successfully", email);

                    // Set initial LastLoginDate
                    user.LastLoginDate = DateTime.UtcNow;

                    // Générer le token d'approbation
                    var expiresAt = tokenService.GetDefaultExpiration();
                    var token = tokenService.GenerateToken(user.Id, user.Email ?? string.Empty, expiresAt);

                    // Stocker le token dans l'utilisateur
                    user.PendingApprovalToken = token;
                    user.PendingApprovalTokenExpires = expiresAt;
                    await userManager.UpdateAsync(user);

                    // Envoyer l'email de notification à l'admin
                    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
                    var emailSent = await emailService.SendApprovalNotificationAsync(user, token, expiresAt, baseUrl);

                    if (!emailSent)
                    {
                        logger.LogWarning("Failed to send approval notification email for user {Email}, but registration succeeded", user.Email);
                    }

                    await signInManager.SignInAsync(user, isPersistent: false);
                    logger.LogInformation("User {Email} signed in after registration", email);

                    var redirectTo = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
                    return Results.Redirect(redirectTo);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to create user {Email}: {Errors}", email, errors);
                    var errorParam = Uri.EscapeDataString(errors);
                    return Results.Redirect($"/register?error=creation&details={errorParam}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception during registration");
                return Results.Redirect("/register?error=server");
            }
        }

        /// <summary>
        /// Gère la déconnexion de l'utilisateur
        /// </summary>
        private static async Task<IResult> HandleLogout(
            HttpContext context,
            SignInManager<ApplicationUser> signInManager,
            ILogger<Program> logger)
        {
            try
            {
                var userName = context.User?.Identity?.Name ?? "Unknown";
                await signInManager.SignOutAsync();
                logger.LogInformation("User {UserName} logged out", userName);
                return Results.Redirect("/login");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception during logout");
                return Results.Redirect("/login");
            }
        }

        /// <summary>
        /// Gère la mise à jour du profil utilisateur
        /// </summary>
        private static async Task<IResult> HandleUpdateProfile(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<Program> logger)
        {
            try
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null)
                {
                    logger.LogWarning("Update profile attempted but user not found");
                    return Results.Redirect("/profile?error=notfound");
                }

                var form = await context.Request.ReadFormAsync();
                var accountName = form["accountName"].ToString();
                var phoneNumber = form["phoneNumber"].ToString();

                bool profileUpdated = false;

                // Mise à jour du nom d'affichage
                if (!string.IsNullOrWhiteSpace(accountName) && accountName != user.AccountName)
                {
                    user.AccountName = accountName;
                    var updateResult = await userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        logger.LogWarning("Failed to update AccountName for user {UserId}", user.Id);
                        return Results.Redirect("/profile?error=update");
                    }
                    profileUpdated = true;
                }

                // Mise à jour du numéro de téléphone
                var currentPhone = await userManager.GetPhoneNumberAsync(user);
                if (phoneNumber != currentPhone)
                {
                    var setPhoneResult = await userManager.SetPhoneNumberAsync(user, phoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        logger.LogWarning("Failed to set phone number for user {UserId}", user.Id);
                        return Results.Redirect("/profile?error=update");
                    }
                    profileUpdated = true;
                }

                if (profileUpdated)
                {
                    await signInManager.RefreshSignInAsync(user);
                    logger.LogInformation("User {UserId} updated their profile", user.Id);
                }

                return Results.Redirect("/profile?success=true");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception during profile update");
                return Results.Redirect("/profile?error=server");
            }
        }
    }
}
