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
- `src/YouSummarize.Data` : contexte Identity partage, contexte metier et snapshots EF
- `src/YouSummarize.Dto` : DTOs voyages, resumes, categories et infrastructure IA
- `src/YouSummarize.Data.Repository` : repositories et mappers propres a l'application
- `src/YouSummarize.Docker` : Docker Compose sans service de base de donnees

## Bases de donnees

Les comptes, roles, profils et approbations restent dans la base PostgreSQL
existante, via `ApplicationDbContext`. Son snapshot historique conserve le
modele de reference complet, mais le modele EF actuel ne contient plus les
entites recettes : il ne decrit donc pas toute la base partagee.

Un garde-fou refuse l'application de migrations EF, la generation de scripts de
migration et le controle des changements en attente pour `ApplicationDbContext`,
en execution comme avec les outils EF. Cela ne protege pas contre du SQL manuel,
`EnsureCreated`, un autre contexte ou un autre outil ; la protection durable
exige aussi des droits PostgreSQL adaptes. Ne pas modifier le schema partage
depuis YouSummarize et ne pas appliquer de migration a cette base.

`YouSummarizeDbContext` contient uniquement voyages, resumes, categories, prompts,
consommations IA et cles Data Protection. Tous les repositories metier et Data
Protection utilisent ce contexte. Identity continue a utiliser `ApplicationDbContext`.

Configuration des connexions :

- `DB_*` continue de designer la base existante des comptes.
- `YOUSUMMARIZE_DB_DATABASE=yousummarizedb` designe la base metier. Seul le nom
  de base change : serveur, port et identifiants PostgreSQL restent ceux de `DB_*`.
- Si le parametre est absent, l'application utilise `yousummarizedb` par defaut.
  S'il est explicitement vide, les deux contextes utilisent encore `DB_DATABASE`
  et un avertissement est journalise. Compose transmet une valeur vide si la
  variable n'est pas definie : il faut donc la renseigner pour la base metier.
- Un nom cible identique a `DB_DATABASE` est refuse lorsqu'il est explicitement configure.

La migration initiale dans `Migrations/YouSummarize` concerne uniquement la
nouvelle base. L'application n'appelle ni `Migrate`, ni `EnsureCreated` au demarrage.

Les cookies et le nom Data Protection sont distincts : une reconnexion a
YouSummarize est necessaire. Les comptes et roles restent partages, donc leur
modification dans une application affecte egalement l'autre.

La copie initiale et la bascule locale ont ete realisees ; l'utilisateur a
confirme une connexion et une modification enregistree dans la nouvelle base.
Le prompt `YoutubeResumeV3` faisait partie du transfert ; la consommation IA et
les cles Data Protection sont reparties vides. Les tables et donnees source
n'ont pas ete supprimees. Les scripts ponctuels de transfert ont ete retires
apres cette operation ; la migration EF et son snapshot restent necessaires.

Retour arriere : arreter uniquement YouSummarize et definir explicitement
`YOUSUMMARIZE_DB_DATABASE` a une chaine vide pour retrouver l'ancienne base et ses
cles. Le retrait du parametre ne suffit pas : la valeur par defaut est
`yousummarizedb`. Le retour ne recopie pas les nouvelles ecritures de la cible :
les sauvegarder et les reconcilier au prealable. Conserver les sauvegardes et
CSV prives selon la politique de retention choisie ; ne pas les commiter.

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
2. Utiliser le reseau externe testnet existant. Garder DB_* pour les comptes ;
    definir YOUSUMMARIZE_DB_DATABASE sur la base metier deja preparee.
    Ne pas recreer le serveur PostgreSQL ni son volume.
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
dotnet ef migrations has-pending-model-changes --context YouSummarizeDbContext --project src/YouSummarize.Data --startup-project src/YouSummarize --no-build
dotnet run --project tests/YouSummarize.Data.Checks
```

Le controle EF compare le modele metier a son snapshot sans ouvrir de connexion.
Le programme de checks verifie aussi le blocage des migrations Identity, la
compatibilite des colonnes/cles/index et l'isolation de la migration initiale.
Lors du split initial, les deux images Docker ont ete construites. Le
demarrage YouSummarize et ses ressources statiques ont ete verifies dans un
conteneur sans reseau avec une base fictive, puis ce conteneur a ete supprime.
L'indisponibilite de cette base fictive produit des erreurs Data Protection
attendues ; ce test ne valide pas l'authentification.
La connexion et une modification dans la nouvelle base ont ete confirmees
localement par l'utilisateur. Les autres parcours, notamment la generation
de resumes, restent a verifier avec les services habituels et les autorisations requises.


## Tips

# Modify the server-side reconnection handler

https://learn.microsoft.com/fr-fr/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-8.0#modify-the-server-side-reconnection-handler

# ThemeManager :

Editeur de thème : https://themes.arctechonline.tech/