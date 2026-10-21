#!/usr/bin/env bash
# Hook PreToolUse (Bash) — refuse un `git commit` contenant un secret potentiel.
#
# Deux niveaux : gitleaks s'il est installé (recommandé), sinon une détection par motifs.
# Si gitleaks est installé et détecte quelque chose, on BLOQUE : un contrôle de sécurité
# ne doit jamais se désactiver silencieusement.
# Contrat : l'appel d'outil arrive en JSON sur stdin ; exit 2 = commande bloquée,
# stderr est renvoyé à l'agent.
set -uo pipefail

cmd="$(jq -r '.tool_input.command // empty' 2>/dev/null)" || exit 0
printf '%s' "$cmd" | grep -qi 'git commit' || exit 0

if command -v gitleaks >/dev/null 2>&1; then
  if ! out="$(gitleaks protect --staged --redact --no-banner 2>&1)"; then
    {
      echo "COMMIT BLOQUÉ : gitleaks a détecté un secret dans les fichiers indexés."
      printf '%s\n' "$out" | head -20
      echo ""
      echo "Procédure : retirer la valeur du code (dotnet user-secrets, variable d'environnement"
      echo "ou coffre), puis considérer le secret comme COMPROMIS et le faire révoquer."
    } >&2
    exit 2
  fi
  exit 0
fi

PATTERNS='(password|passwd|pwd|secret|api[_-]?key|token|BEGIN (RSA|OPENSSH|EC) PRIVATE KEY|connectionstring|aws_secret_access_key)'
hits="$(git diff --cached -U0 2>/dev/null | grep -iE "^\+.*$PATTERNS" | grep -viE '\$\{[A-Z_]+\}|<[a-z-]+>|changeme|example' || true)"

if [ -n "$hits" ]; then
  {
    echo "COMMIT BLOQUÉ par le hook secret-scan : secret potentiel détecté."
    echo "Lignes suspectes :"
    printf '%s\n' "$hits" | head -5
    echo ""
    echo "Retirez la valeur du code (dotnet user-secrets, variable d'environnement ou coffre),"
    echo "puis recommencez. Cette détection par motifs est un filet : installez gitleaks."
  } >&2
  exit 2
fi
exit 0
