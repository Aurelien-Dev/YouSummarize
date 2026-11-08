# YouSummarize

YouSummarize est une application Blazor .NET 10 et MudBlazor pour generer des
resumes de videos et preparer des voyages. Cette solution est autonome : aucune
reference de projet ou de fichier ne pointe vers RecettesFamille.

## Reprise du travail

Lire [la note de reprise HANDOFF.md](HANDOFF.md) avant de poursuivre uniquement
sur YouSummarize. Elle retrace le split, la correction HTTPS et surtout le
deploiement Docker, la base partagee et les precautions pour le site recettes
desormais en production.

## Fonctionnalités

- Inscription et connexion des utilisateurs
- Approbation des inscriptions et gestion des utilisateurs
- Extraction des transcripts YouTube avec Supadata et des outils yt-dlp existants
- Generation IA de resumes, conseils, lieux, pays et categories
- Gestion des voyages, des categories, des favoris et des statuts des resumes
- Administration des voyages, categories, utilisateurs et prompts

## Structure du projet

- `src/YouSummarize.sln` : solution autonome
- `src/YouSummarize` : front, endpoints et services
- `src/YouSummarize.Data` : modele EF du schema partage et snapshot de reference
- `src/YouSummarize.Dto` : DTOs voyages, resumes, categories et infrastructure IA
- `src/YouSummarize.Data.Repository` : repositories et mappers propres a l'application
- `src/YouSummarize.Docker` : Docker Compose sans service de base de donnees

## Base de donnees existante

Les deux applications utilisent la meme base PostgreSQL, les memes comptes et
les memes roles. Aucune donnee n'a ete transferee, supprimee ou modifiee par le split.

Le modele EF complet et les relations Identity sont conserves pour correspondre
au schema existant. Les entites recettes sont uniquement des mappings de
compatibilite : il n'y a ni front, ni repository, ni service recettes dans cette
solution. Le RAG recettes et ses dependances ont ete retires.

Seul ApplicationDbContextModelSnapshot est conserve dans les migrations.
YouSummarize n'appelle ni Migrate, ni EnsureCreated, ni l'initialisation du RAG.
Ne pas lancer database update ou creer une nouvelle base pour ce split.

Les cookies et le nom Data Protection sont distincts : une reconnexion a
YouSummarize est necessaire. Les comptes et roles restent partages, donc leur
modification dans une application affecte egalement l'autre.

## Prérequis

- .NET 10 SDK
- Acces a la base PostgreSQL existante
- Docker (optionnel)

## Installation

1. Configurer les parametres DB_*, OPENAI_SECRET, DEEPSEEK_SECRET, SUPADATA_API_KEY,
   les courriels et APPROVAL_TOKEN_SECRET selon SECRETS.md. La configuration locale
   existante a ete copiee, mais reste exclue de Git, de Docker et de la publication.
2. Compiler :
   ```sh
   dotnet build src/YouSummarize/YouSummarize.csproj
   ```
3. Lancer avec un port distinct du site recettes :
   ```sh
   dotnet run --project src/YouSummarize -- --HTTP_PORT=8082
   ```

## Utilisation

- Acces local : http://localhost:8082 avec la commande ci-dessus.
- La racine et /travelplanner affichent le meme site ; les routes existantes sont conservees.

## Déploiement avec Docker

1. Fournir un fichier src/.env prive avec les variables de SECRETS.md.
2. Utiliser le reseau externe testnet existant et les memes parametres de base
    que RecettesFamille. Ne pas recreer le serveur PostgreSQL ni son volume.
3. Construire et lancer le conteneur applicatif independant :
    ```sh
    docker compose --project-name yousummarize -f src/YouSummarize.Docker/docker-compose-light.yml --env-file src/.env up --build -d
    ```

Image : yousummarize. Conteneur : yousummarize.client. Port hote par defaut : 8082.
Volume applicatif : /home/data/yousummarize. YOUSUMMARIZE_HTTP_PORT et
YOUSUMMARIZE_DATA_PATH permettent de les adapter. Le proxy inverse doit cibler ce
nouveau conteneur ou ce port, pas l'adresse fixe du conteneur recettes.

Le workflow manuel .github/workflows/docker-publish.yml deploie par SSH dans
/home/yousummarize avec les memes noms de secrets et variables que le depot
recettes. Les configurer dans le futur depot YouSummarize. Il ne deploie aucune
base et n'arrete pas les conteneurs recettes. Aucun depot Git distant ni
deploiement de production n'a ete cree ou declenche pendant le split.

## Verification sans base

```sh
dotnet build src/YouSummarize/YouSummarize.csproj
dotnet publish src/YouSummarize/YouSummarize.csproj -c Release
dotnet ef migrations has-pending-model-changes --project src/YouSummarize.Data --startup-project src/YouSummarize --no-build
```

Le controle EF utilise la factory de conception et compare le modele au snapshot
sans ouvrir de connexion. Les deux images Docker ont ete construites. Le
demarrage YouSummarize et ses ressources statiques ont ete verifies dans un
conteneur sans reseau avec une base fictive, puis ce conteneur a ete supprime.
L'indisponibilite de cette base fictive produit des erreurs Data Protection
attendues ; ce test ne valide pas l'authentification.
Les parcours authentifies et la generation de resumes
restent a verifier avec les services habituels : ils lisent ou ecrivent dans la
base partagee et n'ont donc pas ete executes pendant le split.


## Tips

# Modify the server-side reconnection handler

https://learn.microsoft.com/fr-fr/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-8.0#modify-the-server-side-reconnection-handler

# ThemeManager :

Editeur de thème : https://themes.arctechonline.tech/