#!/usr/bin/env bash
# Hook SessionStart — rappelle le contexte de travail au démarrage de la session.
# Ce que ce script écrit sur stdout est ajouté au contexte de l'agent.
# Purement informatif : il n'échoue jamais.
set -uo pipefail

cd "${CLAUDE_PROJECT_DIR:-.}" 2>/dev/null || true

branch="$(git branch --show-current 2>/dev/null || echo '-')"
dirty="$(git status --porcelain 2>/dev/null | wc -l | tr -d ' ')"
sdk="$(dotnet --version 2>/dev/null || echo 'introuvable (export PATH="/usr/local/share/dotnet:$PATH")')"
docker_state="$(docker info >/dev/null 2>&1 && echo 'démarré' || echo 'arrêté')"
kctx="$(kubectl config current-context 2>/dev/null || echo 'aucun')"

cat <<TXT
Contexte de travail (oddo-dev-toolkit)
  dépôt      : $(basename "$PWD")  — branche $branch, $dirty fichier(s) modifié(s)
  .NET SDK   : $sdk
  Docker     : $docker_state
  kubectl    : contexte $kctx
  garde-fous : anti-secret (commit + écriture), blocage destructif, migrations immuables, audit.
TXT
[ "$branch" = "main" ] && echo "  ATTENTION  : vous êtes sur main. Créez une branche feat/, fix/ ou chore/ avant de modifier."
exit 0
