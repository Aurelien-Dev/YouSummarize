# Configuration des Secrets - YouSummarize

Les noms de variables restent compatibles avec RecettesFamille. Utiliser la base
existante ; ne pas creer de base ou de compte PostgreSQL pour ce split.
La configuration locale et les fichiers .env ne doivent jamais etre commites.
Les secrets et variables du workflow doivent etre configures dans le depot
YouSummarize ; ils ne sont pas automatiquement partages entre depots GitHub.

Ce document liste tous les secrets et variables d'environnement requis pour le fonctionnement de l'application.

## 🔐 Secrets Requis

### 1. APPROVAL_TOKEN_SECRET ⭐ NOUVEAU
**Description:** Secret utilisé pour générer et valider les tokens d'approbation des nouveaux comptes utilisateurs.

**Format:** Chaîne aléatoire sécurisée de 32+ caractères (base64 recommandé)

**Génération:**
```bash
# Linux/macOS
openssl rand -base64 32

# PowerShell (Windows)
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))

# Ou utiliser un générateur en ligne (https://randomkeygen.com/)
```

**Exemple:** `kJ8x3nP9mQ2vL5wR7tY1uZ4aB6cD8eF0gH2iJ4kL6mN8`

**Utilisation:** Signature HMAC-SHA256 des tokens d'approbation d'utilisateurs

---

### 2. OPENAI_SECRET
**Description:** Clé API pour OpenAI (utilisée pour les fonctionnalités d'IA)

**Format:** Clé API OpenAI commençant par `sk-`

**Obtention:** https://platform.openai.com/api-keys

**Exemple:** `sk-proj-xxxxxxxxxxxxxxxxxxxxxxxxxxxxx`

---

### 3. DEEPSEEK_SECRET
**Description:** Clé API pour DeepSeek (service d'IA alternatif)

**Format:** Clé API DeepSeek

**Obtention:** https://platform.deepseek.com/

---

### 4. EMAIL_BACKUP_FROM
**Description:** Adresse email utilisée pour l'envoi des emails (notifications, approbations, etc.)

**Format:** Adresse email valide Gmail

**Exemple:** `notifications@votredomaine.com`

**Note:** Doit correspondre au compte Gmail configuré avec SMTP_PASSWORD

---

### 5. EMAIL_BACKUP_DEST
**Description:** Adresse email de l'administrateur qui reçoit les notifications (nouvelles inscriptions, erreurs, etc.)

**Format:** Adresse email valide

**Exemple:** `admin@votredomaine.com`

---

### 6. SMTP_PASSWORD
**Description:** Mot de passe d'application Gmail pour l'envoi d'emails

**Format:** Mot de passe d'application Gmail (16 caractères sans espaces)

**Obtention:** 
1. Activer la validation en 2 étapes sur votre compte Google
2. Aller sur https://myaccount.google.com/apppasswords
3. Créer un mot de passe d'application pour "Mail"

**Exemple:** `abcdefghijklmnop`

**Configuration SMTP actuelle:**
- Serveur: `smtp.gmail.com`
- Port: `465` (SSL)

---

### 7. DB_HOST_URL
**Description:** URL/adresse IP du serveur PostgreSQL

**Format:** Nom d'hôte ou adresse IP

**Exemple:** `recettesfamille.data` (Docker), `localhost` (dev local), `db.example.com` (production)

---

### 8. DB_HOST_PORT
**Description:** Port du serveur PostgreSQL

**Format:** Nombre (port réseau)

**Par défaut:** `5432`

**Exemple:** `5442` (si mappé différemment dans Docker)

---

### 9. DB_DATABASE
**Description:** Nom de la base de données PostgreSQL

**Par défaut:** `recettesfamilledb`

---

### 10. DB_USERNAME
**Description:** Nom d'utilisateur PostgreSQL

**Par défaut:** `pguser`

---

### 11. DB_PASSWORD
**Description:** Mot de passe de l'utilisateur PostgreSQL

**Par défaut (dev):** `PGUserPwd`

**Production:** Utiliser un mot de passe fort et unique

**Génération (production):**
```bash
openssl rand -base64 24
```

---

### 12. SUPADATA_API_KEY
**Description:** Clé API pour Supadata (extraction de transcripts YouTube)

**Format:** Clé API Supadata

**Obtention:** https://supadata.ai/

**Endpoint:** `https://api.supadata.ai`

---

## 📋 Configuration par Environnement

### Développement Local

Créer un fichier `appsettings.Local.json` dans `src/RecettesFamille/` :

```json
{
  "OPENAI_SECRET": "sk-...",
  "DEEPSEEK_SECRET": "...",
  "EMAIL_BACKUP_FROM": "votre-email@gmail.com",
  "EMAIL_BACKUP_DEST": "admin@example.com",
  "SMTP_PASSWORD": "abcdefghijklmnop",
  "DB_HOST_URL": "localhost",
  "DB_HOST_PORT": "5432",
  "DB_DATABASE": "recettesfamilledb",
  "DB_USERNAME": "pguser",
  "DB_PASSWORD": "PGUserPwd",
  "SUPADATA_API_KEY": "...",
  "APPROVAL_TOKEN_SECRET": "kJ8x3nP9mQ2vL5wR7tY1uZ4aB6cD8eF0gH2iJ4kL6mN8"
}
```

**Note:** Le fichier `appsettings.Local.json` est dans `.gitignore` et ne doit jamais être commité.

---

### Docker Compose

Créer un fichier `.env` à la racine du projet (ou dans `src/RecettesFamille.Docker/`) :

```env
OPENAI_SECRET=sk-...
DEEPSEEK_SECRET=...
EMAIL_BACKUP_FROM=votre-email@gmail.com
EMAIL_BACKUP_DEST=admin@example.com
SMTP_PASSWORD=abcdefghijklmnop
DB_HOST_URL=recettesfamille.data
DB_HOST_PORT=5432
DB_DATABASE=recettesfamilledb
DB_USERNAME=pguser
DB_PASSWORD=PGUserPwd
SUPADATA_API_KEY=...
APPROVAL_TOKEN_SECRET=kJ8x3nP9mQ2vL5wR7tY1uZ4aB6cD8eF0gH2iJ4kL6mN8
```

**Note:** Le fichier `.env` doit être dans `.gitignore`.

---

### GitHub Actions / CI/CD

Configurer les secrets dans le repository GitHub :

1. Aller dans **Settings** → **Secrets and variables** → **Actions**
2. Ajouter chaque secret individuellement via **New repository secret**

**Secrets à configurer :**
- `APPROVAL_TOKEN_SECRET` ⭐ (nouveau)
- `OPENAI_SECRET`
- `DEEPSEEK_SECRET`
- `EMAIL_BACKUP_FROM`
- `EMAIL_BACKUP_DEST`
- `SMTP_PASSWORD`
- `DB_HOST_URL`
- `DB_HOST_PORT`
- `DB_DATABASE`
- `DB_USERNAME`
- `DB_PASSWORD`
- `SUPADATA_API_KEY`

---

## ⚠️ Sécurité

### Bonnes Pratiques

1. **Ne jamais commiter les secrets** dans le code source
2. **Utiliser des secrets différents** pour chaque environnement (dev, staging, production)
3. **Régénérer les secrets** en cas de suspicion de compromission
4. **Limiter l'accès** aux secrets aux personnes autorisées uniquement
5. **Auditer régulièrement** les accès aux secrets

### Fichiers à Ignorer (`.gitignore`)

Vérifier que ces fichiers sont bien dans `.gitignore` :
```
appsettings.Local.json
.env
*.secret
*.key
```

---

## 🔄 Rotation des Secrets

Il est recommandé de changer régulièrement les secrets sensibles :

- **APPROVAL_TOKEN_SECRET** : Tous les 6-12 mois
- **DB_PASSWORD** : Tous les 3-6 mois
- **API Keys** : Selon les recommandations du fournisseur
- **SMTP_PASSWORD** : Si compromis ou changement de compte

---

## 📞 Support

En cas de problème avec la configuration des secrets :
1. Vérifier les logs de l'application
2. Vérifier que tous les secrets sont définis
3. Vérifier le format des secrets (pas d'espaces, quotes, etc.)
4. Contacter l'administrateur système
