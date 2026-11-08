using YouSummarize.Data;
using YouSummarize.Managers;

namespace YouSummarize.Services;

/// <summary>
/// Service responsable de l'envoi des emails de notification d'approbation utilisateur
/// </summary>
public class UserApprovalEmailService
{
    private readonly EmailManager _emailManager;
    private readonly ILogger<UserApprovalEmailService> _logger;

    public UserApprovalEmailService(
        EmailManager emailManager,
        IConfiguration configuration,
        ILogger<UserApprovalEmailService> logger)
    {
        _emailManager = emailManager;
        _logger = logger;
    }

    /// <summary>
    /// Envoie un email à l'admin pour notifier une nouvelle inscription
    /// </summary>
    public async Task<bool> SendApprovalNotificationAsync(
        ApplicationUser user,
        string approvalToken,
        DateTime expiresAt,
        string baseUrl)
    {
        try
        {
            var approvalUrl = BuildApprovalUrl(baseUrl, user.Id, approvalToken, expiresAt);
            var subject = $"Nouvelle inscription - {user.AccountName} ({user.Email})";
            var bodyText = BuildEmailBodyText(user, approvalUrl, expiresAt, baseUrl);
            var bodyHtml = BuildEmailBodyHtml(user, approvalUrl, expiresAt, baseUrl);

            await _emailManager.SendEmailAsync(subject, bodyText, bodyHtml);

            _logger.LogInformation("Approval notification email sent for user {Email}", user.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval notification email for user {Email}", user.Email);
            return false;
        }
    }

    private string BuildApprovalUrl(string baseUrl, string userId, string token, DateTime expiresAt)
    {
        var expiresTimestamp = new DateTimeOffset(expiresAt).ToUnixTimeSeconds();
        return $"{baseUrl}/api/users/approve/{userId}?token={token}&expires={expiresTimestamp}";
    }

    private string BuildEmailBodyText(ApplicationUser user, string approvalUrl, DateTime expiresAt, string baseUrl)
    {
        var registrationDate = DateTime.UtcNow;
        return $@"Nouvelle inscription sur YouSummarize

Nom du compte: {user.AccountName}
Email: {user.Email}
Date d'inscription: {registrationDate:dd/MM/yyyy HH:mm:ss} UTC

Pour approuver cet utilisateur, cliquez sur le lien:
{approvalUrl}

Ce lien expire le {expiresAt:dd/MM/yyyy HH:mm:ss} UTC (dans 7 jours).

Administration: {baseUrl}/travelplanner/admin";
    }

    private string BuildEmailBodyHtml(ApplicationUser user, string approvalUrl, DateTime expiresAt, string baseUrl)
    {
        var registrationDate = DateTime.UtcNow;
        return $@"<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0;"">
        <h1>✉️ Nouvelle Inscription</h1>
    </div>
    <div style=""background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; border-top: none;"">
        <p>Un nouvel utilisateur s'est inscrit et attend votre approbation.</p>
        
        <div style=""background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #4CAF50;"">
            <p><span style=""font-weight: bold; color: #555;"">👤 Nom du compte:</span> {user.AccountName}</p>
            <p><span style=""font-weight: bold; color: #555;"">📧 Email:</span> {user.Email}</p>
            <p><span style=""font-weight: bold; color: #555;"">📅 Date d'inscription:</span> {registrationDate:dd/MM/yyyy à HH:mm:ss} UTC</p>
        </div>

        <div style=""text-align: center; margin: 20px 0;"">
            <a href=""{approvalUrl}"" style=""display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;"">✅ Approuver cet utilisateur</a>
        </div>

        <div style=""background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 15px 0;"">
            <p><strong>⚠️ Important:</strong></p>
            <ul>
                <li>Ce lien expire le <strong>{expiresAt:dd/MM/yyyy à HH:mm:ss} UTC</strong> (dans 7 jours)</li>
                <li>Il ne peut être utilisé qu'une seule fois</li>
                <li>L'utilisateur recevra automatiquement le rôle ""Reader"" après approbation</li>
            </ul>
        </div>

        <p>Vous pouvez également gérer tous les utilisateurs depuis la <a href=""{baseUrl}/travelplanner/admin"" style=""color: #4CAF50; text-decoration: none;"">page d'administration</a>.</p>
    </div>
    <div style=""text-align: center; padding: 15px; color: #777; font-size: 12px;"">
        <p>YouSummarize - Système de gestion des utilisateurs</p>
    </div>
</body>
</html>";
    }
}