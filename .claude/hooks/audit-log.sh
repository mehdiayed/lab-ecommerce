#!/usr/bin/env bash
# Hook PostToolUse (Bash|Write|Edit|MultiEdit|mcp__*) — journalise chaque action de l'agent.
#
# Traçabilité exigée par DORA : qui, quand, quoi, dans quel dépôt, avec quel outil.
# Le journal est écrit HORS du dépôt de travail (ni committé, ni effacé par `git clean`).
# Le dossier vient de l'option du plugin `audit_dir` (userConfig), sinon ~/.claude/audit.
# Ce hook ne fait JAMAIS échouer une action (exit 0 systématique).
set -uo pipefail

LOG_DIR="${CLAUDE_PLUGIN_OPTION_AUDIT_DIR:-${CLAUDE_AUDIT_DIR:-$HOME/.claude/audit}}"
LOG_DIR="${LOG_DIR/#\~/$HOME}"     # la valeur par défaut "~/.claude/audit" arrive sans expansion
LOG="$LOG_DIR/$(date -u +%F).jsonl"
mkdir -p "$LOG_DIR" 2>/dev/null || exit 0

payload="$(cat)"
line="$(jq -c \
  --arg ts   "$(date -u +%FT%TZ)" \
  --arg user "${USER:-unknown}" \
  --arg repo "$(basename "${CLAUDE_PROJECT_DIR:-$PWD}")" \
  '{ts:$ts, session:(.session_id // "unknown"), user:$user, repo:$repo,
    tool:(.tool_name // "unknown"),
    command:(.tool_input.command // null),
    file:(.tool_input.file_path // null)}' \
  <<<"$payload" 2>/dev/null)" || exit 0

# Une ligne complète ou rien (pas de JSON tronqué en cas d'arrêt brutal).
[ -n "$line" ] && printf '%s\n' "$line" >> "$LOG" 2>/dev/null
exit 0
