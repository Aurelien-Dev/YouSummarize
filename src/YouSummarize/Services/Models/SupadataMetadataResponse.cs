namespace YouSummarize.Services.Models;

/// <summary>
/// Représente la réponse de l'API Supadata pour la récupération des métadonnées d'un contenu.
/// </summary>
public class SupadataMetadataResponse
{
    /// <summary>La plateforme source (youtube, tiktok, instagram, twitter, facebook).</summary>
    public string? Platform { get; set; }

    /// <summary>Le type de contenu (video, image, carousel, post).</summary>
    public string? Type { get; set; }

    /// <summary>L'identifiant de la ressource sur la plateforme.</summary>
    public string? Id { get; set; }

    /// <summary>L'URL canonique du contenu.</summary>
    public string? Url { get; set; }

    /// <summary>Le titre du contenu.</summary>
    public string? Title { get; set; }

    /// <summary>La description du contenu.</summary>
    public string? Description { get; set; }

    /// <summary>Les informations sur l'auteur.</summary>
    public SupadataAuthor? Author { get; set; }

    /// <summary>Les statistiques d'engagement.</summary>
    public SupadataStats? Stats { get; set; }

    /// <summary>Les informations sur le média.</summary>
    public SupadataMedia? Media { get; set; }

    /// <summary>Les tags associés au contenu.</summary>
    public List<string>? Tags { get; set; }

    /// <summary>La date de création au format ISO 8601.</summary>
    public string? CreatedAt { get; set; }
}

/// <summary>Informations sur l'auteur du contenu.</summary>
public class SupadataAuthor
{
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool? Verified { get; set; }
}

/// <summary>Statistiques d'engagement du contenu.</summary>
public class SupadataStats
{
    public long? Views { get; set; }
    public long? Likes { get; set; }
    public long? Comments { get; set; }
    public long? Shares { get; set; }
}

/// <summary>Informations sur le média associé au contenu.</summary>
public class SupadataMedia
{
    public string? Type { get; set; }
    public int? Duration { get; set; }
    public string? ThumbnailUrl { get; set; }
}
