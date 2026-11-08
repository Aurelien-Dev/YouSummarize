using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using OpenAI.Images;
using YouSummarize.Api;
using YouSummarize.Data.Repository.IRepositories;
using YouSummarize.Dto.Models;
using YouSummarize.Managers.AiGenerators.Models;

namespace YouSummarize.Managers.AiGenerators;

/// <summary>
/// Manages AI operations for video summaries and travel metadata.
/// </summary>
public class AiManager(IServiceProvider serviceProvider, IAiRepository aiRepository, ICategoryRepository categoryRepository) : IAiManager
{
    // V3.1 - AI Metadata Detection Thresholds
    /// <summary>
    /// Seuil de confiance minimum pour accepter un pays détecté (0.70 = 70%).
    /// </summary>
    private const double COUNTRY_CONFIDENCE_THRESHOLD = 0.70;

    /// <summary>
    /// Seuil de confiance minimum pour accepter une catégorie détectée (0.70 = 70%).
    /// </summary>
    private const double CATEGORY_CONFIDENCE_THRESHOLD = 0.70;

    /// <summary>
    /// Prompt système V3 pour la génération de résumés YouTube avec métadonnées structurées.
    /// Le placeholder {CATEGORIES_LIST} doit être remplacé dynamiquement par la liste des catégories existantes.
    /// </summary>
    private const string YOUTUBE_RESUME_V3_PROMPT = @"Tu es un assistant spécialisé dans l'analyse de vidéos de voyage YouTube.

Ta mission :
Analyse le transcript d'une vidéo YouTube et génère un résumé structuré en JSON.

## Instructions pour le champ ""summaryIntro"" :
Rédige un paragraphe d'introduction de 3 à 5 lignes résumant la vidéo.
Commence directement par le texte, sans aucun titre Markdown (pas de # ou ##).
Sois factuel et synthétique. N'invente rien, utilise uniquement les informations du transcript.

## Instructions pour le champ ""tips"" :
Liste à puces des conseils et astuces pratiques mentionnés dans la vidéo :
- **Transport** : comment se déplacer, coûts, cartes de transport, conseils
- **Logement** : où dormir, quartiers recommandés, prix, réservations
- **Budget** : prix des repas, entrées, activités, coût de la vie
- **Erreurs à éviter** : pièges à touristes, arnaques, choses à ne pas faire
- **Astuces pratiques** : meilleurs moments pour visiter, applications utiles, conseils locaux

Si AUCUN conseil n'est mentionné dans la vidéo, écris exactement :
Aucun conseil pratique spécifique mentionné dans cette vidéo.

## Instructions pour le champ ""places"" :
Liste structurée des lieux cités dans la vidéo avec leurs noms exacts et adresses si disponibles :
- **Restaurants, cafés, bars** : nom complet + adresse si mentionnée
- **Hôtels, auberges, logements** : nom + quartier
- **Attractions touristiques** : monuments, musées, parcs, jardins
- **Quartiers visités** : noms des zones explorées
- **Magasins, centres commerciaux** : boutiques mentionnées

Format : ""**Nom du lieu** (Adresse) - Type/description""

Si AUCUN lieu n'est cité, écris exactement :
Aucun lieu spécifique mentionné dans cette vidéo.

**Règles absolues :**
- Sois factuel et synthétique
- N'invente RIEN, utilise uniquement les informations présentes dans la vidéo
- Les 3 champs sont OBLIGATOIRES, même si leur contenu est vide

**Exemple de valeurs attendues :**

""summaryIntro"" : ""La vidéo présente un voyage à Tokyo pendant la saison des cerisiers en fleurs, avec un focus sur la gastronomie locale et les quartiers traditionnels.""

""tips"" : ""- **Transport** : Acheter une Suica Card dès l'arrivée à l'aéroport pour tous les transports en commun\n- **Budget** : Prévoir 15-25€ par repas dans les restaurants locaux\n- **Réservations** : Réserver les restaurants populaires 2-3 jours à l'avance via Tabelog""

""places"" : ""- **Yakiniku Jumbo Hanare** (3-14-9 Roppongi, Minato-ku) - Restaurant de viande grillée primé\n- **Jardin Shinjuku Gyoen** - Parc célèbre pour observer les cerisiers en fleurs\n- **Tsuta Ramen** (1-14-1 Sugamo, Toshima-ku) - Premier ramen étoilé Michelin""

## Instructions pour le pays principal (champ ""mainCountry"") :
- Identifie le pays qui constitue le SUJET PRINCIPAL de la vidéo
- Ignore les simples mentions secondaires ou les pays de transit
- Si le pays est clairement identifiable, fournis :
  - name : le nom du pays en français
  - isoCode : le code ISO 3166-1 alpha-2 (ex: ""FR"", ""JP"", ""US"") UNIQUEMENT si tu es certain
  - confidence : un score entre 0 et 1 représentant ta certitude
- Si aucun pays ne peut être déterminé avec certitude, retourne null

## Instructions pour les catégories (champ ""categories"") :
Tu dois sélectionner UNIQUEMENT parmi les catégories suivantes :
{CATEGORIES_LIST}

- Sélectionne uniquement les catégories réellement pertinentes pour cette vidéo
- N'invente AUCUNE nouvelle catégorie
- Pour chaque catégorie sélectionnée, fournis :
  - name : le nom exact de la catégorie (respecte la casse)
  - confidence : un score entre 0 et 1 représentant ta certitude
- Si aucune catégorie ne correspond, retourne une liste vide

## Format de réponse attendu :
Réponds UNIQUEMENT avec un objet JSON valide, sans texte supplémentaire, sans balises Markdown et sans explication.

Schéma JSON :
{
  ""summaryIntro"": ""string (paragraphe d'introduction, sans titre Markdown)"",
  ""tips"": ""string (conseils et astuces en Markdown, sans titre)"",
  ""places"": ""string (lieux mentionnés en Markdown, sans titre)"",
  ""mainCountry"": {
    ""name"": ""string"",
    ""isoCode"": ""string (2 lettres) ou null"",
    ""confidence"": number (0-1)
  } ou null,
  ""categories"": [
    {
      ""name"": ""string (nom exact de la catégorie)"",
      ""confidence"": number (0-1)
    }
  ]
}";

    public async Task<YoutubeResumeDto> GetYoutubeResume(string transcript, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Charger les catégories existantes
            var categories = await categoryRepository.GetAll(cancellationToken);

            // 2. Récupérer le prompt (BDD avec fallback hardcodé)
            string systemPrompt = await GetYoutubeResumePrompt(cancellationToken);

            // 3. Injecter la liste des catégories dans le prompt
            var categoriesList = string.Join("\n", categories.Select(c => $"- {c.Name}"));
            systemPrompt = systemPrompt.Replace("{CATEGORIES_LIST}", categoriesList);

            // 4. Construire le message utilisateur avec le transcript
            var userMessage = $@"
=== Début du transcript ===
{transcript}
=== Fin du transcript ===";

            var messages = new ChatMessage[]
            {
                new (ChatRole.System, systemPrompt),
                new (ChatRole.User, userMessage)
            };

            // 5. Appeler l'IA avec JSON structuré
            var result = await GetChatResponse<AiSummaryGenerationResult>(messages, AiClientType.OpenAi, cancellationToken);

            // 6. Reconstruire le Markdown final à partir des 3 champs distincts
            var fullMarkdown = $"{result.SummaryIntro}\n\n###### Conseils et astuces pratiques\n{result.Tips}\n\n###### Lieux mentionnés\n{result.Places}";

            // 7. Valider le pays détecté
            var validatedCountry = ValidateCountry(result.MainCountry);

            // 8. Valider et filtrer les catégories détectées
            var validatedCategories = ValidateAndFilterCategories(result.Categories, categories);

            // 9. Construire le DTO avec toutes les métadonnées
            var summaryDto = new YoutubeResumeDto
            {
                Resume = fullMarkdown,
                CreatedDate = DateTime.UtcNow,

                // Métadonnées IA - Pays
                MainCountryName = validatedCountry?.Name,
                MainCountryIsoCode = validatedCountry?.IsoCode,
                MainCountryConfidence = validatedCountry?.Confidence,

                // Métadonnées IA - État de l'analyse
                AiMetadataStatus = AiMetadataStatus.Completed,
                AiMetadataAnalyzedAt = DateTime.UtcNow,

                // Catégories détectées
                CategoryIds = validatedCategories.Select(c => c.Id).ToList()
            };

            return summaryDto;
        }
        catch (JsonException ex)
        {
            // Erreur de désérialisation JSON - sauvegarder quand même avec statut Failed
            var errorMessage = "Erreur de désérialisation du résultat IA";
            Console.WriteLine($"[ERROR] {errorMessage}: {ex.Message}");

            throw new InvalidOperationException(errorMessage, ex);
        }
        catch (Exception ex)
        {
            // Erreur générale - sauvegarder quand même avec statut Failed
            var errorMessage = "Erreur lors de l'analyse IA des métadonnées";
            Console.WriteLine($"[ERROR] {errorMessage}: {ex.Message}");

            throw;
        }
    }


    public async Task<T> GetChatResponse<T>(ChatMessage[] chatMessages, AiClientType aiClientTypeEnum, CancellationToken cancellationToken = default)
    {
        var result = await GetChatResponse(chatMessages, aiClientTypeEnum, ChatResponseFormat.Json, cancellationToken);
        var serialized = JsonConvert.DeserializeObject<T>(result) ?? throw new InvalidOperationException("Deserialization failed");

        return serialized;
    }

    public async Task<string> GetChatResponse(ChatMessage[] chatMessages, AiClientType aiClientTypeEnum, ChatResponseFormat chatResponseFormat, CancellationToken cancellationToken = default)
    {
        IChatClient client = GetChatClient(aiClientTypeEnum);

        ChatResponse completion = await client.GetResponseAsync(chatMessages, new ChatOptions() { ResponseFormat = chatResponseFormat }, cancellationToken: cancellationToken);

        var resultText = completion.Text;

        ArgumentException.ThrowIfNullOrEmpty(resultText);

        await ReportChatConsumption(completion, aiClientTypeEnum);
        return resultText;
    }


    private IChatClient GetChatClient(AiClientType aiClientTypeEnum)
    {
        IChatClient client;

        switch (aiClientTypeEnum)
        {
            case AiClientType.OpenAi:
                client = serviceProvider.GetRequiredKeyedService<IChatClient>("OpenAi");
                break;
            case AiClientType.DeepSeek:
                client = serviceProvider.GetRequiredKeyedService<IChatClient>("DeepSeek");
                break;
            default:
                throw new ArgumentException("Invalid AI client type", nameof(aiClientTypeEnum));
        }

        return client;
    }

    /// <summary>
    /// Reports the consumption of chat operations to the AI repository.
    /// </summary>
    /// <param name="completion">The chat response completion details.</param>
    /// <exception cref="InvalidOperationException">Thrown when token counts are null.</exception>
    private async Task ReportChatConsumption(ChatResponse completion, AiClientType aiClientTypeEnum)
    {
        if (completion?.Usage?.InputTokenCount == null || completion.Usage.OutputTokenCount == null)
        {
            throw new InvalidOperationException("Token counts are null");
        }

        var (inputPrice, outputPrice, chatModel) = GetPrices(aiClientTypeEnum);

        await aiRepository.ReportConsumption(new AiConsumptionDto()
        {
            Date = DateTime.UtcNow,
            InputToken = (int)completion.Usage.InputTokenCount,
            OutputToken = (int)completion.Usage.OutputTokenCount,
            InputPrice = inputPrice,
            OutputPrice = outputPrice,
            UseCase = "RecipeConverter",
            AiModelName = $"openai-{chatModel}"
        });
    }

    private static (decimal inputPrice, decimal outputPrice, string chatModel) GetPrices(AiClientType aiClientTypeEnum)
    {
        return aiClientTypeEnum switch
        {
            AiClientType.OpenAi => (2.50m, 10.00m, "gpt-4o"),
            AiClientType.DeepSeek => (0.27m, 1.10m, "deepseek-chat"),
            _ => throw new ArgumentException("Invalid AI client type", nameof(aiClientTypeEnum))
        };
    }

    // V3.1 - AI Metadata Detection Methods

    /// <summary>
    /// Récupère le prompt de génération YouTube V3 depuis la BDD ou utilise le fallback hardcodé.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le contenu du prompt système.</returns>
    private async Task<string> GetYoutubeResumePrompt(CancellationToken cancellationToken)
    {
        try
        {
            var promptDto = await aiRepository.GetPrompt("YoutubeResumeV3", cancellationToken);
            return promptDto.Prompt;
        }
        catch
        {
            // Fallback sur le prompt hardcodé si non trouvé en BDD
            return YOUTUBE_RESUME_V3_PROMPT;
        }
    }

    /// <summary>
    /// Valide un pays détecté par l'IA en vérifiant le score de confiance.
    /// </summary>
    /// <param name="country">Le pays détecté à valider.</param>
    /// <returns>Le pays validé ou null si invalide ou en dessous du seuil.</returns>
    private DetectedCountryResult? ValidateCountry(DetectedCountryResult? country)
    {
        if (country == null)
            return null;

        // Vérifier que le score de confiance est entre 0 et 1
        if (country.Confidence < 0 || country.Confidence > 1)
            return null;

        // Appliquer le seuil de confiance minimum
        if (country.Confidence < COUNTRY_CONFIDENCE_THRESHOLD)
            return null;

        return country;
    }

    /// <summary>
    /// Valide et filtre les catégories détectées par l'IA en les comparant aux catégories existantes.
    /// </summary>
    /// <param name="detectedCategories">Liste des catégories détectées par l'IA.</param>
    /// <param name="existingCategories">Liste des catégories existantes en base de données.</param>
    /// <returns>Liste des CategoryDto valides et au-dessus du seuil de confiance.</returns>
    private List<CategoryDto> ValidateAndFilterCategories(
        List<DetectedCategoryResult> detectedCategories,
        List<CategoryDto> existingCategories)
    {
        var validCategories = new List<CategoryDto>();

        foreach (var detected in detectedCategories)
        {
            // Vérifier que le score de confiance est entre 0 et 1
            if (detected.Confidence < 0 || detected.Confidence > 1)
                continue;

            // Appliquer le seuil de confiance minimum
            if (detected.Confidence < CATEGORY_CONFIDENCE_THRESHOLD)
                continue;

            // Trouver la catégorie existante (lookup insensible à la casse)
            var existingCat = existingCategories.FirstOrDefault(
                c => c.Name.Equals(detected.Name, StringComparison.OrdinalIgnoreCase));

            if (existingCat != null)
            {
                validCategories.Add(existingCat);
            }
            else
            {
                // Logger un warning si la catégorie retournée par l'IA n'existe pas
                Console.WriteLine($"[WARNING] AI returned unknown category: {detected.Name}");
            }
        }

        return validCategories;
    }
}
