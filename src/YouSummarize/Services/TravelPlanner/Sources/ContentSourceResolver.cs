using Microsoft.Extensions.Logging;

namespace YouSummarize.Services.TravelPlanner.Sources;

/// <summary>
/// Résout et route les appels vers le service source le plus approprié pour une URL donnée.
/// Parcourt les services spécialisés enregistrés par ordre de priorité, puis tombe en fallback
/// sur le service générique. En cas d'erreur du service sélectionné, essaie le suivant.
/// </summary>
public class ContentSourceResolver : IContentSourceService
{
    private readonly IEnumerable<IContentSourceService> _specializedServices;
    private readonly GenericSourceService _genericService;
    private readonly ILogger<ContentSourceResolver> _logger;

    /// <inheritdoc/>
    public string SourceType => "Resolver";

    /// <inheritdoc/>
    public bool CanHandle(string url) => true;

    /// <summary>
    /// Initialise une nouvelle instance du resolver.
    /// </summary>
    /// <param name="specializedServices">Services spécialisés (YoutubeSourceService, etc.) évalués en priorité.</param>
    /// <param name="genericService">Service générique utilisé en fallback final.</param>
    /// <param name="logger">Logger pour tracer les routing et fallbacks.</param>
    public ContentSourceResolver(IEnumerable<IContentSourceService> specializedServices, 
        GenericSourceService genericService, ILogger<ContentSourceResolver> logger)
    {
        _specializedServices = specializedServices;
        _genericService = genericService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ContentMetadata> ExtractMetadata(string sourceUrl, CancellationToken cancellationToken = default)
    {
        foreach (var service in GetCandidates(sourceUrl))
        {
            try
            {
                _logger.LogDebug("ExtractMetadata : tentative avec {ServiceType} pour {Url}", service.GetType().Name, sourceUrl);
                return await service.ExtractMetadata(sourceUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ExtractMetadata : échec avec {ServiceType} pour {Url}, passage au suivant", service.GetType().Name, sourceUrl);
            }
        }

        throw new InvalidOperationException($"Aucun service n'a pu extraire les métadonnées pour l'URL : {sourceUrl}");
    }

    /// <inheritdoc/>
    public async Task<string> GetContent(string sourceUrl, CancellationToken cancellationToken = default)
    {
        foreach (var service in GetCandidates(sourceUrl))
        {
            try
            {
                _logger.LogDebug("GetContent : tentative avec {ServiceType} pour {Url}", service.GetType().Name, sourceUrl);
                return await service.GetContent(sourceUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetContent : échec avec {ServiceType} pour {Url}, passage au suivant", service.GetType().Name, sourceUrl);
            }
        }

        throw new InvalidOperationException($"Aucun service n'a pu récupérer le contenu pour l'URL : {sourceUrl}");
    }

    /// <summary>
    /// Retourne les services candidats dans l'ordre : spécialisés correspondants, puis générique.
    /// </summary>
    private IEnumerable<IContentSourceService> GetCandidates(string url)
    {
        foreach (var service in _specializedServices)
        {
            if (service.CanHandle(url))
                yield return service;
        }

        yield return _genericService; //Fallback sur le générique
    }
}
