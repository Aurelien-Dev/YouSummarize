# YouSummarize

## Architecture
- Solution autonome : `src/YouSummarize.sln`.
- Front Blazor : `src/YouSummarize/Components/Pages/TravelPlanner/`.
- Services : `src/YouSummarize/Services/TravelPlanner/`.
- Entites EF : `src/YouSummarize.Data/EntityModel/`.
- DTOs : `src/YouSummarize.Dto/Models/`.
- Repositories : `src/YouSummarize.Data.Repository/Repositories/`.
- Ne jamais ajouter de reference vers les projets ou fichiers RecettesFamille.

## Base partagee
- Utiliser la base PostgreSQL existante, les comptes et roles existants.
- Le modele EF complet du schema partage est conserve pour rester coherent avec
  le snapshot et les relations Identity. Les entites recettes sont des mappings
  de compatibilite, sans services ou front recettes.
- L'historique des migrations est retire ; seul le snapshot de reference reste.
- Ne pas appliquer de migrations, initialiser un schema, supprimer des tables
  ou modifier des donnees dans le cadre du split.
- Les cookies et le nom d'application Data Protection sont propres a YouSummarize.

## Conventions
- Blazor .NET 10 Server-side et MudBlazor pour tous les composants UI.
- Interface en francais ; noms de code et commentaires en anglais.
- Layout : MudStack, MudGrid, MudItem. Pas de layout HTML ou CSS custom si un
  equivalent MudBlazor existe.
- Boutons : MudButton et MudIconButton. Texte : MudText. Icones : MudIcon et
  Icons.Material.Filled.* / Icons.Material.Outlined.*.
- Formulaires : MudForm et composants MudBlazor ; dialogues via IDialogService.
- Notifications : ISnackbar. Tableaux : MudTable ou MudDataGrid.
- Navigation : MudNavMenu et MudNavLink. Tooltips : MudTooltip.
- Sous-composants dans un dossier <PageName>PageComponents ; EventCallback pour
  la communication enfant-parent. Les services injectes portent la logique metier.
- Toujours utiliser des DTOs entre couches.
- Garder les interfaces, comportements, prompts et routes existants.
- Ne pas publier appsettings.Local.json ou les fichiers .env.
- Docker : conteneur yousummarize.client, base externe, reseau testnet partage,
  volume applicatif distinct. Ne jamais intervenir sur le conteneur recettes.