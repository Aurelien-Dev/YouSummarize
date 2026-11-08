# Reprise de YouSummarize apres separation de RecettesFamille

Date de la note : 2026-11-08.

## 1. Situation et perimetre

YouSummarize genere des resumes de videos YouTube pour preparer des voyages.
Le nom TravelPlanner reste utilise dans les dossiers, les routes et certains types.
Ce n'est plus un second site heberge dans le front RecettesFamille.

- Dossier de travail : `D:\01_dev\YouSummarize`.
- Solution autonome : [src/YouSummarize.sln](src/YouSummarize.sln).
- Ancien depot : `D:\01_dev\MudRecettesFamille\MudRecettesFamille`.
- Le split a ete realise sur la branche `evol/TryToSplit` de cet ancien depot.
- L'utilisateur confirme que RecettesFamille fonctionne et est deploye en production.
- Le prochain agent doit travailler uniquement dans YouSummarize.
- Aucune reference de projet ou de fichier vers l'ancien depot n'est necessaire.
- Aucun depot Git local n'est present dans YouSummarize au moment de cette note.
- Aucun depot GitHub ni deploiement YouSummarize n'a ete cree ou declenche par l'agent.

Ne pas modifier, redeployer ou arreter RecettesFamille pour poursuivre ce travail.
Une eventuelle intervention sur la base partagee exige une autorisation distincte.

Lire les conventions dans [.github/copilot-instructions.md](.github/copilot-instructions.md)
et [.github/instructions/travel-planner.instructions.md](.github/instructions/travel-planner.instructions.md).

## 2. Ce qui a ete fait

- Creation d'une solution et de projets propres a YouSummarize, avec namespaces adaptes.
- Extraction des pages TravelPlanner, layouts, dialogues, filtres et administration.
- Extraction des endpoints, services de resumes, repositories voyages/videos/categories,
  DTOs et outils d'extraction YouTube.
- Separation du gestionnaire IA : les methodes recettes ont ete retirees de YouSummarize,
  mais les prompts et les methodes de generation de resumes ont ete conserves.
- Duplication autonome de l'infrastructure utile : Identity, utilisateurs, roles,
  approbation, courriels, themes, erreurs et suivi de consommation IA.
- Conservation de la gestion des utilisateurs et des prompts avec le layout YouSummarize.
- Retrait du front recettes, de ses services/repositories/DTOs et du projet RAG.
- Retrait des dependances Cropper et ImageMagick propres au front recettes.
- Adaptation de la marque, des liens, des cookies et de Data Protection.
- Ajout de la route `/` au site ; `/travelplanner` et ses routes existantes restent valides.
- Preparation d'une image, d'un Compose et de workflows GitHub propres a YouSummarize.
- Exclusion des sorties bin/obj, sauvegardes, fichiers d'IDE et scripts temporaires du split.

Les interfaces et comportements metier devaient rester identiques : ce travail
n'etait ni un redesign, ni une evolution de fonctionnalites, ni une mise a jour NuGet.

## 3. Reperes dans le code

| Surface | Emplacement |
| --- | --- |
| Demarrage, DI, cookies, ports | [Program.cs](src/YouSummarize/Program.cs) |
| Front voyages et resumes | [Pages/TravelPlanner](src/YouSummarize/Components/Pages/TravelPlanner) |
| Connexion et profil | [Pages/Account](src/YouSummarize/Components/Pages/Account) |
| API metier | [Api/TravelPlanner](src/YouSummarize/Api/TravelPlanner) |
| Services de resumes et sources | [Services/TravelPlanner](src/YouSummarize/Services/TravelPlanner) |
| IA et prompts de resumes | [Managers/AiGenerators](src/YouSummarize/Managers/AiGenerators) |
| Supadata | [ServicesExternal](src/YouSummarize/ServicesExternal) |
| Extracteur et binaires yt-dlp | [Tools/yt-dlp](src/YouSummarize/Tools/yt-dlp) |
| Contexte EF | [ApplicationDbContext.cs](src/YouSummarize.Data/ApplicationDbContext.cs) |
| DTOs | [YouSummarize.Dto](src/YouSummarize.Dto) |
| Repositories et mappers | [YouSummarize.Data.Repository](src/YouSummarize.Data.Repository) |

Stack : .NET 10, Blazor Server interactif, MudBlazor, EF Core et PostgreSQL.
Interface en francais, noms de code en anglais ; suivre les composants MudBlazor existants.

## 4. Base partagee : contrainte essentielle

Les deux applications utilisent toujours la meme base PostgreSQL. Le split du code
n'a applique aucune migration et n'a transfere, nettoye ou modifie aucune donnee.
Les noms de tables, colonnes et relations existants ont ete preserves.

Le modele EF complet du schema partage est conserve dans YouSummarize, y compris
les entites recettes et leurs relations Identity. Ces entites sont des mappings
de compatibilite, pas une dependance sur les projets de l'autre site.
Ne pas les supprimer pour rendre le projet apparemment plus propre : cela ferait
diverger le modele du snapshot et pourrait conduire a une migration destructive.

Dans [Migrations](src/YouSummarize.Data/Migrations), seul
[ApplicationDbContextModelSnapshot.cs](src/YouSummarize.Data/Migrations/ApplicationDbContextModelSnapshot.cs)
reste. L'historique des migrations a ete retire de YouSummarize, pas de RecettesFamille.
YouSummarize n'appelle pas `Migrate`, `EnsureCreated` ou l'initialisation du RAG au demarrage.
Le snapshot ne doit pas servir a recreer la base existante.

Les comptes et roles sont communs. Une modification d'utilisateur, de role ou
d'approbation peut donc affecter RecettesFamille. Les tables communes de prompts
et de consommation IA ne sont pas devenues des donnees isolees non plus.
L'application fonctionne normalement en lecture/ecriture ; seule l'operation de split
s'est faite sans intervenir sur les donnees.

Cookie : `.YouSummarize.Identity`. Nom Data Protection : `YouSummarize`.
Les cles sont persistees dans la table existante `DataProtectionKeys`.
Une reconnexion est necessaire sur le nouveau site ; aucun SSO n'a ete ajoute.
Ne pas purger les cles communes pour corriger un probleme d'authentification.

## 5. Lancement local et correction HTTPS

Depuis la racine YouSummarize :

```powershell
dotnet run --project src/YouSummarize --launch-profile https
```

Profil HTTPS : `https://localhost:7225`, avec HTTP sur `http://localhost:5158`.
Profil HTTP : `http://localhost:5158`. Les profils actuels ne lancent pas automatiquement
le navigateur (`launchBrowser: false`). Verifier les lignes `Now listening on`.

Un bug apres extraction forcait Kestrel a ecouter uniquement en HTTP sur 8080,
ignorant le profil HTTPS et causant `ERR_CONNECTION_REFUSED` sur 7225.
Il a ete corrige dans Program.cs : respecter les URLs et ports ASP.NET configures,
utiliser 8080 uniquement en repli, ou appliquer une surcharge explicite `HTTP_PORT`.

```powershell
dotnet run --project src/YouSummarize -- --HTTP_PORT=8082
```

Cette surcharge force HTTP sur 8082, meme si un profil HTTPS est selectionne.
Ne pas reintroduire un `ListenAnyIP(8080)` inconditionnel.
Les certificats de developpement HTTPS sont distincts de la configuration TLS du VPS.

## 6. Docker : configuration actuelle

Fichiers a examiner avant toute modification :

- [Dockerfile](src/YouSummarize/Dockerfile).
- [docker-compose.yml](src/YouSummarize.Docker/docker-compose.yml).
- [docker-compose-light.yml](src/YouSummarize.Docker/docker-compose-light.yml).
- [.dockerignore](src/.dockerignore).
- [Workflow de deploiement](.github/workflows/docker-publish.yml).

| Element | Valeur actuelle |
| --- | --- |
| Projet Compose utilise par le workflow | `yousummarize` |
| Service | `yousummarize` |
| Image | `${DOCKER_REGISTRY-}yousummarize` |
| Conteneur | `yousummarize.client` |
| Repertoire VPS | `/home/yousummarize` |
| Contexte de build | `src/` |
| Port HTTP interne | `8080` |
| Port hote | `${YOUSUMMARIZE_HTTP_PORT:-8082}` |
| Volume applicatif hote | `${YOUSUMMARIZE_DATA_PATH:-/home/data/yousummarize}` |
| Montage dans le conteneur | `/app/data` |
| Reseau externe existant | `testnet` |
| Hote PostgreSQL par defaut | `recettesfamille.data` |
| Port PostgreSQL interne par defaut | `5432` |
| Base par defaut | `recettesfamilledb` |

Le Compose light reutilise le service du Compose principal via `extends`.
Les deux variantes ne contiennent aucun serveur PostgreSQL, aucun volume de base
et aucun `depends_on` vers un service de base local. La base doit deja etre disponible.
Pas d'adresse IP fixe attribuee a YouSummarize : ne pas reutiliser celle des recettes.

Si le serveur PostgreSQL est sur le meme reseau Docker, utiliser son nom DNS et
son port interne 5432, pas automatiquement le port publie sur l'hote. Pour une
connexion distante, conserver les parametres DB_* correspondant a cette connexion.
Ne pas recreer le reseau ou le volume de la base pour ce deploiement.

Le Dockerfile utilise les images SDK/runtime .NET 10 Alpine, copie tous les projets
avant le restore, puis compile et publie en Release. Le runtime contient Node.js
pour yt-dlp, ainsi que les clients PostgreSQL et SSH herites. Le binaire Linux
yt-dlp musllinux est rendu executable. Conserver ce binaire et sa copie a la publication.
`ENTRYPOINT` execute `YouSummarize.dll`.

Attention : `EXPOSE 8081` ne configure ni certificat ni HTTPS a lui seul.
Le Compose actuel publie uniquement HTTP 8080 vers 8082. Le profil IDE
`Container (Dockerfile)` avec `useSSL: true` est un autre mecanisme, non valide ici.

## 7. Parametres et secrets Docker

Referentiel : [SECRETS.md](SECRETS.md). Ne jamais ajouter de valeurs secretes a cette note.

Secrets GitHub utilises par le workflow :
`DEPLOY_HOST`, `DEPLOY_USER`, `DEPLOY_PASSWORD`, `OPENAI_SECRET`, `DEEPSEEK_SECRET`,
`DB_PASSWORD`, `SMTP_PASSWORD`, `SUPADATA_API_KEY`, `APPROVAL_TOKEN_SECRET`, `YTDLP_COOKIES`.

Variables GitHub utilisees : `EMAIL_BACKUP_DEST`, `EMAIL_BACKUP_FROM`, `DB_HOST_URL`,
`DB_HOST_PORT`, `DB_DATABASE`, `DB_USERNAME`.

Ces valeurs doivent etre configurees dans le depot YouSummarize : elles ne sont
pas automatiquement heritees du depot recettes. DEEPSEEK_SECRET est encore exige
par la validation de la DI existante, meme si les resumes utilisent OpenAI.
YTDLP_COOKIES est optionnel pour Compose ; verifier l'extraction selon les videos.

Le fichier prive attendu sur le VPS est `/home/yousummarize/src/.env`.
Le Compose peut egalement recevoir YOUSUMMARIZE_HTTP_PORT, YOUSUMMARIZE_DATA_PATH
et DOCKER_REGISTRY. Le workflow actuel n'ecrit pas ces trois options dans .env ;
les adapter explicitement dans un futur changement si necessaire.

`appsettings.Local.json` a ete copie pour les tests locaux. Il est exclu de Git,
du contexte Docker et de la publication. Les fichiers .env sont aussi exclus de Git.
Program.cs ajoute appsettings.json puis appsettings.Local.json apres la configuration
standard : des valeurs JSON peuvent prendre le dessus sur les variables ou arguments.
Avant de modifier les parametres, verifier cette priorite ; ne pas embarquer le
fichier local pour faire fonctionner le conteneur.

## 8. Procedure de deploiement et securite du site recettes

Le workflow est manuel (`workflow_dispatch`) et conserve le mecanisme SSH/SCP/Compose
de l'ancien site, avec ses propres chemins. Il :

1. Cree `/home/yousummarize` et supprime uniquement `/home/yousummarize/src`.
2. Copie le depot dans `/home/yousummarize/`.
3. Ecrit `src/.env` avec `umask 077`.
4. Execute `up --build -d` avec le projet Compose `yousummarize`.

Il ne lance ni les migrations, ni le Compose recettes, ni une base supplementaire.
Il n'effectue pas de `down` sur les recettes ni de nettoyage global Docker.
La suppression de src peut compliquer un retour arriere si la copie echoue :
la preparation d'un deploiement atomique/rollback est un travail futur, pas realise.

Depuis la racine du depot, pour une intervention de deploiement autorisee :

```sh
docker compose --project-name yousummarize -f src/YouSummarize.Docker/docker-compose-light.yml --env-file src/.env config --quiet
docker compose --project-name yousummarize -f src/YouSummarize.Docker/docker-compose-light.yml --env-file src/.env up --build -d
docker compose --project-name yousummarize -f src/YouSummarize.Docker/docker-compose-light.yml --env-file src/.env ps
docker logs --tail 100 yousummarize.client
```

Ne pas afficher `config` sans `--quiet` dans les journaux : les secrets seraient resolus.
Ne pas lancer `database update`, `down -v`, des commandes sur le volume PostgreSQL
ou un `docker system prune` pour terminer le split.

Pour le proxy inverse, choisir soit `yousummarize.client:8080` depuis le reseau
testnet, soit le port hote 8082 selon l'architecture du proxy. Configurer un nom
de domaine/certificat distinct, les WebSockets et le maintien du circuit Blazor.
Program.cs utilise `UseHttpsRedirection` sans appel explicite a `UseForwardedHeaders` :
verifier le schema HTTPS transmis par le proxy avant un deploiement public.
Le proxy, le DNS et le certificat YouSummarize n'ont pas ete configures par l'agent.

Le Compose conserve pour l'instant `ASPNETCORE_ENVIRONMENT=Development` et `user: root`.
Ce sont des points a revoir avant la production, pas des choix de securite valides
par le split. Ne pas changer l'image de base ou les droits sans discuter les impacts.
Les versions d'actions GitHub sont heritees ; le workflow n'a pas ete execute a distance.

## 9. Verifications faites et limites

Lors du split, les controles suivants ont reussi :

- Compilation des deux fronts et publication Release.
- References YouSummarize internes et existantes, sans DLL RecettesFamille publiee.
- Modele EF coherent avec le snapshot dans les deux solutions.
- Configurations Compose principales et light valides dans les deux solutions.
- Construction des images locales `recettesfamille:split-check` et `yousummarize:split-check`.
- Demarrage YouSummarize dans un conteneur `--network none` avec une base fictive.
- Ressources site.css, Blazor, MudBlazor et MudBlazor.Markdown servies dans ce test.
- Conteneur de test supprime apres verification.
- Correction HTTPS compilee dans un dossier de sortie separe, sans demarrer sur la base.

Le test sans reseau produisait des erreurs Data Protection attendues sur la base
fictive. Il ne prouve pas que l'authentification ou les appels IA fonctionnent.
La construction des images precedait la correction HTTPS ; reconstruire pour
livrer le code actuel. Les tags split-check ne sont pas des livraisons de production.

L'utilisateur a depuis confirme la bonne marche et le deploiement recettes.
Il n'a pas encore confirme ici un deploiement YouSummarize ou la reussite de tous
ses parcours authentifies. Aucun resultat de test fonctionnel ne doit etre invente.
Des avertissements NuGet/MudBlazor existaient avant le split ; ils n'ont pas ete
corriges par une mise a jour de dependances hors perimetre.

Controles sans application ni connexion a la base :

```powershell
dotnet build src/YouSummarize/YouSummarize.csproj
dotnet publish src/YouSummarize/YouSummarize.csproj -c Release
dotnet ef migrations has-pending-model-changes --project src/YouSummarize.Data --startup-project src/YouSummarize --no-build
docker build -t yousummarize:verification -f src/YouSummarize/Dockerfile src
```

Le controle EF utilise la factory de conception pour comparer le modele au snapshot
en memoire. Ne pas le remplacer par une commande d'application de migrations.
Compiler le projet web est le controle CLI le plus simple : la solution inclut
un projet Docker SDK dont le support peut dependre de l'IDE.

## 10. Prochaine reprise conseillee

1. Ouvrir uniquement le dossier YouSummarize et lire cette note et ses instructions.
2. Verifier l'etat actuel des fichiers et initialiser/configurer Git seulement sur demande.
3. Confirmer les tests locaux HTTPS, connexion et affichage des voyages existants.
4. Tester les resumes, statuts, categories et approbations avec l'autorisation requise
   pour les ecritures sur la base partagee et les appels IA payants.
5. Preparer les secrets du depot, le reseau et le proxy avant le premier deploiement.
6. Faire valider les changements de mode Production, de droits et de gestion du proxy.
7. Deployer uniquement YouSummarize et verifier que le site recettes reste intact.

Cette note decrit l'etat au 2026-11-08 ; relire les fichiers de configuration avant
d'executer une commande, notamment apres de nouveaux changements utilisateur.