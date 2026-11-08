using System.Security.Cryptography;
using System.Text;

namespace YouSummarize.Services;

/// <summary>
/// Service pour générer et valider les tokens d'approbation sécurisés pour les nouveaux utilisateurs
/// </summary>
public class ApprovalTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApprovalTokenService> _logger;

    public ApprovalTokenService(IConfiguration configuration, ILogger<ApprovalTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Génère un token d'approbation sécurisé pour un utilisateur
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <param name="email">Email de l'utilisateur</param>
    /// <param name="expiresAt">Date d'expiration du token</param>
    /// <returns>Token HMAC-SHA256 hexadécimal</returns>
    public string GenerateToken(string userId, string email, DateTime expiresAt)
    {
        var secret = GetSecret();
        var expiresTimestamp = new DateTimeOffset(expiresAt).ToUnixTimeSeconds();

        // Créer la chaîne à signer : userId|email|timestamp
        var dataToSign = $"{userId}|{email}|{expiresTimestamp}";

        // Calculer le HMAC-SHA256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));

        // Convertir en hexadécimal
        var token = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        _logger.LogInformation("Generated approval token for user {UserId} (expires: {ExpiresAt})", userId, expiresAt);

        return token;
    }

    /// <summary>
    /// Valide un token d'approbation
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <param name="email">Email de l'utilisateur</param>
    /// <param name="token">Token à valider</param>
    /// <param name="expiresTimestamp">Timestamp d'expiration Unix</param>
    /// <returns>True si le token est valide, False sinon</returns>
    public bool ValidateToken(string userId, string email, string token, long expiresTimestamp)
    {
        try
        {
            // Vérifier l'expiration
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresTimestamp).UtcDateTime;
            if (DateTime.UtcNow > expiresAt)
            {
                _logger.LogWarning("Token expired for user {UserId} (expired: {ExpiresAt})", userId, expiresAt);
                return false;
            }

            // Recalculer le token attendu
            var expectedToken = GenerateTokenInternal(userId, email, expiresTimestamp);

            // Comparer de manière sécurisée (protection contre timing attacks)
            var isValid = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedToken),
                Encoding.UTF8.GetBytes(token)
            );

            if (!isValid)
            {
                _logger.LogWarning("Invalid token for user {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("Valid token for user {UserId}", userId);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Calcule la date d'expiration par défaut (7 jours à partir de maintenant)
    /// </summary>
    /// <returns>Date d'expiration UTC</returns>
    public DateTime GetDefaultExpiration()
    {
        return DateTime.UtcNow.AddDays(7);
    }

    /// <summary>
    /// Génère un token en utilisant directement le timestamp (méthode interne pour validation)
    /// </summary>
    private string GenerateTokenInternal(string userId, string email, long expiresTimestamp)
    {
        var secret = GetSecret();
        var dataToSign = $"{userId}|{email}|{expiresTimestamp}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));

        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Récupère le secret depuis la configuration
    /// </summary>
    private string GetSecret()
    {
        var secret = _configuration["APPROVAL_TOKEN_SECRET"];

        if (string.IsNullOrWhiteSpace(secret))
        {
            _logger.LogError("APPROVAL_TOKEN_SECRET is not configured!");
            throw new InvalidOperationException(
                "APPROVAL_TOKEN_SECRET must be configured in application settings. " +
                "Generate a secure random string with: openssl rand -base64 32"
            );
        }

        if (secret.Length < 32)
        {
            _logger.LogWarning("APPROVAL_TOKEN_SECRET is too short (minimum 32 characters recommended)");
        }

        return secret;
    }
}
