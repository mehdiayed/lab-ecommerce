-- Rôle PostgreSQL en LECTURE SEULE pour le serveur MCP (lab 04).
--
-- C'est le contrôle qui compte : même si l'outil MCP était mal configuré, ce rôle ne
-- peut physiquement ni écrire, ni modifier le schéma. Le mot de passe n'est PAS dans ce
-- fichier : il est passé par psql au moment de l'exécution (variable :ro_password).
--
-- Exécution (par l'humain, hors session Claude Code) :
--   docker exec -i <conteneur-postgres> psql -U postgres -d catalogdb \
--     -v ro_password="$CATALOG_RO_PASSWORD" -f - < sql/create-readonly-role.sql

\set ON_ERROR_STOP on

CREATE ROLE catalog_ro LOGIN PASSWORD :'ro_password'
    NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT
    CONNECTION LIMIT 5;

-- Garde-fous côté serveur : une requête de l'agent ne peut ni bloquer, ni durer.
ALTER ROLE catalog_ro SET default_transaction_read_only = on;
ALTER ROLE catalog_ro SET statement_timeout = '5s';

GRANT CONNECT ON DATABASE catalogdb TO catalog_ro;
GRANT USAGE ON SCHEMA public TO catalog_ro;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO catalog_ro;

-- Les tables créées par les PROCHAINES migrations seront lisibles, sans rien de plus.
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO catalog_ro;
