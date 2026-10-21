# Projets de tests — corrigé du lab 02

Deux projets xUnit de tests **d'intégration**, ajoutés à `ECommerce.slnx` :

| Projet | Ce qu'il teste | Technique | Tests |
|---|---|---|:---:|
| `ECommerce.Catalog.Tests` | Les endpoints du catalogue | `WebApplicationFactory<Program>`, base in-memory (avant le lab 04) | 5 |
| `ECommerce.Ordering.Tests` | La création de commande | `WebApplicationFactory<Program>` + **faux Catalog** (`FakeCatalogHandler`) | 4 |

## Mettre en place

**1. Rendre `Program` visible aux tests**, à la fin de `src/ECommerce.Catalog.Api/Program.cs`
**et** de `src/ECommerce.Ordering.Api/Program.cs` :

```csharp
// Makes the Program class visible to the integration tests (WebApplicationFactory<Program>).
public partial class Program { }
```

**2. Copier et référencer les projets.**

```bash
cp -r "$FORMATION/labs/lab-02-tests/solution/tests" ~/lab-ecommerce/
cd ~/lab-ecommerce
dotnet sln ECommerce.slnx add tests/ECommerce.Catalog.Tests/ECommerce.Catalog.Tests.csproj \
                              tests/ECommerce.Ordering.Tests/ECommerce.Ordering.Tests.csproj
dotnet test ECommerce.slnx
```

Résultat attendu (vérifié) : **5 + 4 tests, tous verts** — à condition d'avoir appliqué le
correctif de quantité du [lab 01](../../../lab-01-git-github/).

## Ce que couvrent les tests, et pourquoi

| Test | Comportement | Pourquoi il compte |
|---|---|---|
| `GetProductById_IdInexistant_Retourne404` | 404, pas 200 vide ni 500 | C'est le contrat que Gateway et Ordering consomment |
| `GetProductById_IdNonEntier_…` | La contrainte de route rejette sans exception | Une entrée invalide ne doit jamais produire de 500 |
| `CreateProduct_PuisLecture_…` | 201 **et** `Location` exploitable | Vérifie le cycle complet, pas seulement le code HTTP |
| `CreateOrder_QuantitesMultiples_TotalEgalPrixFoisQuantite` | 2 × 329,00 + 119,99 | Un calcul **métier** : de l'argent |
| `CreateOrder_QuantiteNulleOuNegative_Retourne400` | Le correctif du lab 01 | Un bug corrigé sans test revient |
| `CreateOrder_ProduitInconnu_Retourne400` | Validation via Catalog | Le chemin d'erreur entre services |

> **Le point pédagogique.** La plupart des tests portent sur des **cas d'erreur ou limites**. Le
> chemin nominal est celui qu'on teste à la main tous les jours ; les régressions se cachent
> dans ceux que personne ne rejoue.

## Le faux Catalog

`OrderingApiFactory` remplace le handler HTTP du client typé `CatalogServiceClient` par un
catalogue figé. Deux détails, découverts en écrivant ce corrigé :

- le service discovery doit pouvoir résoudre `catalog` : on fournit
  `services:catalog:http:0` par `UseSetting`, sinon la résolution échoue **avant** d'atteindre
  le faux handler ;
- les tests ne dépendent d'aucun autre service : rapides, déterministes, exécutables en CI.
