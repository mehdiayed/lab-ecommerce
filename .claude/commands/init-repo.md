---
description: Initialise le dépôt git, vérifie le .gitignore, puis crée le dépôt GitHub privé après confirmation
allowed-tools: Bash(git:*), Bash(gh auth status:*), Bash(gh repo create:*), Read, Edit, Write
---

Initialise ce projet comme dépôt git et publie-le sur GitHub. Suis les étapes dans l'ordre, sans en sauter aucune.

## 1. Garde-fou : remote existant

Exécute `git remote -v` (si le dossier est déjà un dépôt git).

- Si un remote existe, **arrête-toi immédiatement** : affiche les remotes, explique que cette commande refuse de toucher un dépôt qui a déjà un remote, et ne modifie rien (ni `git init`, ni `.gitignore`, ni commit).
- Sinon, continue.

## 2. Initialisation

- Si `.git` n'existe pas : `git init -b main`.
- Si `.git` existe déjà (sans remote) : ne le réinitialise pas, continue.

## 3. Vérification du .gitignore

Vérifie que `.gitignore` existe et contient chacune de ces entrées :

- `.env`
- `*.pem`
- `*.key`
- `appsettings.*.local.json`
- `bin/`
- `obj/`

Crée le fichier s'il manque, et ajoute uniquement les entrées absentes (ne supprime ni ne réordonne rien). Indique ce que tu as ajouté. Vérifie ensuite avec `git status --short --ignored` qu'aucun fichier sensible (`.env`, `*.pem`, `*.key`) n'est suivi ou en attente d'ajout ; si c'en est un, signale-le clairement.

## 4. Revue : STOP

Lance `git status` et affiche le résultat complet. Puis **attends ma validation explicite** avant tout `git add` ou commit. Ne continue pas tant que je n'ai pas répondu.

## 5. Premier commit

Après validation seulement : `git add -A`, puis commit sur `main` (c'est le commit initial, seule exception à la règle « pas de travail direct sur main ») avec le message :

```
chore: initial commit
```

Suivi de la ligne d'attribution Co-Authored-By demandée par l'environnement.

## 6. Création du dépôt GitHub : confirmation obligatoire

Vérifie `gh auth status`. Si `gh` n'est pas authentifié, indique-le et arrête-toi.

Demande ensuite explicitement : « Créer le dépôt GitHub **privé** `<nom-du-dossier>` et pousser `main` ? (oui/non) ». Annonce la commande exacte :

```
gh repo create <nom-du-dossier> --private --source=. --remote=origin --push
```

N'exécute `gh repo create` **qu'après un « oui » explicite**. Toute autre réponse, ou une absence de réponse : n'exécute rien et rappelle que le dépôt local est prêt.

Ne passe jamais `--public`. Termine en affichant l'URL du dépôt créé.
