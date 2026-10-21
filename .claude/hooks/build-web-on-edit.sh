#!/usr/bin/env bash
# Hook PostToolUse (Write|Edit|MultiEdit) — compile le frontend après chaque édition dans
# src/ECommerce.Web, et renvoie les erreurs à l'agent.
#
# Pourquoi : le serveur de langage C# (plugin csharp-lsp) ne couvre que les .cs. Une erreur dans
# un composant .razor (ex. CS0542 : un membre injecté qui porte le nom du composant) ne se voit
# qu'à la compilation. Ce hook la fait remonter tout de suite, au lieu d'en fin de tâche.
# PostToolUse ne peut pas annuler l'édition : exit 2 renvoie le message à l'agent, qui corrige.
set -uo pipefail

file="$(jq -r '.tool_input.file_path // empty' 2>/dev/null)"
case "$file" in
  */src/ECommerce.Web/*.razor|*/src/ECommerce.Web/*.cs|*/src/ECommerce.Web/*.razor.css) ;;
  *) exit 0 ;;
esac

cd "${CLAUDE_PROJECT_DIR:-.}" 2>/dev/null || exit 0

if ! out="$(dotnet build src/ECommerce.Web --nologo -v q 2>&1)"; then
  {
    echo "Le frontend ne compile plus après cette édition :"
    printf '%s\n' "$out" | grep -E " error " | sed -E 's# \[[^]]*\]$##; s#^.*/(src/ECommerce\.Web/)#\1#' | sort -u | head -10
    echo "Corrige avant de continuer."
  } >&2
  exit 2
fi
exit 0
