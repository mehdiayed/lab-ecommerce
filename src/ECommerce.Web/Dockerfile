# Image Web:Web : build multi-étapes, runtime chiselé non-root.
# À construire depuis la racine du dépôt :
#   docker build -t ecommerce-web:dev -f src/ECommerce.Web/Dockerfile .

# --- Étape build : SDK complet (jamais livré) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Les .csproj d'abord : la couche de restore reste en cache tant que les dépendances ne changent pas.
COPY src/ECommerce.ServiceDefaults/ECommerce.ServiceDefaults.csproj src/ECommerce.ServiceDefaults/
COPY src/ECommerce.Web/ECommerce.Web.csproj src/ECommerce.Web/
RUN dotnet restore src/ECommerce.Web/ECommerce.Web.csproj

COPY src/ECommerce.ServiceDefaults/ src/ECommerce.ServiceDefaults/
COPY src/ECommerce.Web/ src/ECommerce.Web/
RUN dotnet publish src/ECommerce.Web/ECommerce.Web.csproj -c Release --no-restore -o /app

# --- Étape finale : runtime uniquement, sans shell ni gestionnaire de paquets ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app .

# Non-root (UID 1654) ; port 8080 par défaut. Aucun secret : la configuration est injectée au démarrage.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ECommerce.Web.dll"]
