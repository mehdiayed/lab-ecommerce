# Image Catalog.Api:Catalog : build multi-étapes, runtime chiselé non-root.
# À construire depuis la racine du dépôt :
#   docker build -t ecommerce-catalog:dev -f src/ECommerce.Catalog.Api/Dockerfile .

# --- Étape build : SDK complet (jamais livré) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Les .csproj d'abord : la couche de restore reste en cache tant que les dépendances ne changent pas.
COPY src/ECommerce.ServiceDefaults/ECommerce.ServiceDefaults.csproj src/ECommerce.ServiceDefaults/
COPY src/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj src/ECommerce.Catalog.Api/
RUN dotnet restore src/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj

COPY src/ECommerce.ServiceDefaults/ src/ECommerce.ServiceDefaults/
COPY src/ECommerce.Catalog.Api/ src/ECommerce.Catalog.Api/
RUN dotnet publish src/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj -c Release --no-restore -o /app

# --- Étape finale : runtime uniquement, sans shell ni gestionnaire de paquets ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app .

# Non-root (UID 1654) ; port 8080 par défaut. Aucun secret : la configuration est injectée au démarrage.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ECommerce.Catalog.Api.dll"]
