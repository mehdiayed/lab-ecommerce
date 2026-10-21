---
name: ui-modernize
description: Améliore une page Blazor de l'ecommerce-app (ECommerce.Web) — responsive, états de chargement / vide / erreur, accessibilité (a11y), filtres — en gardant la logique métier intacte. À utiliser pour moderniser, rendre accessible, rendre responsive ou ajouter un état à une page ou un composant .razor. Valide visuellement dans le navigateur avant de conclure.
---

# Skill : ui-modernize

Objectif : une interface **utilisable par tous** et **honnête sur son état** (chargement,
vide, erreur), sans toucher aux règles métier.

## Procédure

1. **État des lieux** — lire la page cible et lister les manques sur quatre axes :
   responsive, états, accessibilité, cohérence visuelle. Présenter la liste **avant** de coder.
2. **Direction visuelle** — si le plugin officiel `frontend-design` est installé, s'appuyer
   dessus pour la proposition ; sinon, rester dans Bootstrap (déjà présent) sans nouvelle
   dépendance. Pour un changement important, produire d'abord une maquette HTML statique.
3. **Appliquer** :
   - **Responsive** : `table-responsive`, grille `row` / `col-*`, mobile d'abord.
   - **États** : skeleton (`placeholder-glow`) au chargement, message explicite si vide,
     alerte avec bouton *Retry* en cas d'erreur réseau, spinner sur les actions longues.
   - **a11y** : voir [a11y-checklist.md](a11y-checklist.md).
4. **Ne pas casser Blazor Server** — conserver `@rendermode InteractiveServer`, les `@bind`,
   les `@onclick` et le bloc `@code` métier (panier, commande).
5. **Compiler** — `dotnet build src/ECommerce.Web` : une erreur Razor ne se voit qu'à la
   compilation.
6. **Vérifier dans le navigateur** — si Claude in Chrome est connecté (`/chrome`) : ouvrir la
   page, tester en largeur 375 px, lire la console, vérifier la navigation au clavier.
   Sinon, donner à l'utilisateur la liste exacte des vérifications à faire.

## Garde-fous

- **Le comportement métier ne change pas** : on améliore la forme, pas les règles.
- Jamais d'information portée **par la couleur seule** : un statut a toujours un texte.
- Tout contrôle interactif a un **nom accessible** unique.
- Aucune dépendance front (CDN, package npm) ajoutée sans accord explicite.
- Aucune donnée client réelle dans les captures d'écran ou les maquettes.
