# Checklist d'accessibilité — pages Blazor

Référence : WCAG 2.2 niveau AA. À dérouler sur la page **rendue**, pas seulement sur le `.razor`.

## Structure

- [ ] Un seul `<h1>` par page ; titres dans l'ordre (`h1` → `h2` → `h3`).
- [ ] Les tableaux ont une `<caption>` (au besoin `visually-hidden`) et des `<th scope="col|row">`.
- [ ] Les zones sont nommées : `<section aria-labelledby="...">`, `role="search"` sur les filtres.

## Formulaires

- [ ] Chaque champ a un `<label for>` relié à son `id` (pas seulement un `placeholder`).
- [ ] Les erreurs de saisie sont annoncées (`role="alert"` ou `aria-live="polite"`).

## Actions

- [ ] Chaque bouton a un nom accessible **unique** : « Add Wireless Mouse to cart », pas dix « Add ».
- [ ] Un bouton inactif est réellement `disabled`, pas seulement grisé.
- [ ] Le focus clavier est visible et suit l'ordre de lecture.

## États

- [ ] Le chargement est annoncé une fois (`role="status"`), le skeleton est `aria-hidden`.
- [ ] Une erreur réseau se distingue d'un résultat vide.

## Couleur et contraste

- [ ] Aucun statut par la couleur seule (badge = couleur **+ texte**).
- [ ] Contraste du texte ≥ 4,5:1 (3:1 pour le texte large).

## Vérification outillée

- Navigateur : arbre d'accessibilité des devtools, navigation complète au clavier.
- Claude in Chrome : lire la console, redimensionner à 375 px, enregistrer un GIF du parcours.
