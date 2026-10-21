#!/usr/bin/env bash
# Hook PreToolUse (Write|Edit|MultiEdit) — refuse d'ÉCRIRE un secret dans un fichier.
#
# Complément du hook secret-scan (qui contrôle le commit) : ici on arrête le secret avant
# même qu'il touche le disque — appsettings.json, docker-compose.yml, manifeste, test...
# Contrat : JSON de l'appel sur stdin ; exit 2 = écriture bloquée, stderr renvoyé à l'agent.

set -uo pipefail

input="$(cat)"

file_path="$(jq -r '.tool_input.file_path // .tool_input.notebook_path // ""' <<<"$input")"

# Le texte sur le point d'être écrit, quelle que soit la forme de l'outil.
content="$(jq -r '
  .tool_input
  | [ .content?, .new_string?, .new_source?, ((.edits? // [])[] | .new_string?) ]
  | map(select(. != null))
  | join("\n")
' <<<"$input")"

[ -z "$content" ] && exit 0

# Fichiers qui contiennent légitimement des motifs ressemblant à des secrets (les hooks eux-mêmes).
case "$file_path" in
  */hooks/scan-secrets-on-write.sh|*/hooks/secret-scan.sh) exit 0 ;;
esac

findings=""

check() {
  local label="$1" regex="$2" flags="${3:-}"
  local match
  match="$(grep -En $flags -e "$regex" <<<"$content" | head -n 3)"
  if [ -n "$match" ]; then
    # On affiche le numéro de ligne, jamais le secret lui-même.
    local lines
    lines="$(cut -d: -f1 <<<"$match" | paste -sd, -)"
    findings+="  - ${label} (ligne(s) ${lines} du nouveau contenu)"$'\n'
  fi
}

# Formats de jetons connus
check "Bloc de clé privée"             '-----BEGIN ([A-Z]+ )?PRIVATE KEY-----'
check "AWS access key ID"             '(AKIA|ASIA)[0-9A-Z]{16}'
check "GitHub token"                  '(ghp|gho|ghu|ghs|ghr)_[A-Za-z0-9]{36}|github_pat_[A-Za-z0-9_]{60,}'
check "Slack token"                   'xox[abprs]-[A-Za-z0-9-]{10,}'
check "Stripe live key"               '(sk|rk)_live_[A-Za-z0-9]{20,}'
check "Anthropic API key"             'sk-ant-[A-Za-z0-9_-]{20,}'
check "OpenAI API key"                'sk-(proj-)?[A-Za-z0-9]{32,}'
check "Google API key"                'AIza[0-9A-Za-z_-]{35}'
check "Azure storage account key"     'AccountKey=[A-Za-z0-9+/=]{40,}'
check "JWT"                           'eyJ[A-Za-z0-9_-]{10,}\.eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}'

# Chaînes de connexion avec mot de passe en clair (appsettings.json, compose, manifeste...)
check "Chaîne de connexion avec mot de passe" '(password|pwd)=[^;"'"'"'[:space:]<$]{6,}' -i

# Affectations génériques « secret = littéral » (C#, JSON, YAML, .env).
# Ignore les gabarits : "", "<...>", "${VAR}", "changeme", "xxx".
generic="$(grep -Eni -e '(api[_-]?key|secret|token|password|passwd|client[_-]?secret)["'"'"']?[[:space:]]*[:=][[:space:]]*["'"'"'][^"'"'"']{8,}["'"'"']' <<<"$content" \
  | grep -Evi -e '["'"'"'](<[^>]*>|\$\{[^}]*\}|changeme|change-me|your[_-]|example|placeholder|dummy|x{6,}|\*{6,})' \
  | head -n 3)"
if [ -n "$generic" ]; then
  lines="$(cut -d: -f1 <<<"$generic" | paste -sd, -)"
  findings+="  - Affectation de secret en dur (ligne(s) ${lines} du nouveau contenu)"$'\n'
fi

if [ -n "$findings" ]; then
  {
    echo "ÉCRITURE BLOQUÉE par le hook scan-secrets-on-write : secret probable dans ${file_path:-<fichier inconnu>}"
    printf '%s' "$findings"
    echo "Ne codez jamais un secret en dur : dotnet user-secrets, variable d'environnement,"
    echo "paramètre Aspire (builder.AddParameter(..., secret: true)) ou Secret Kubernetes."
  } >&2
  exit 2
fi

exit 0
