#!/usr/bin/env bash
set -u

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Find the actual script in Packages or Library/PackageCache
SCRIPT_PATH=""

# 1. Check in Library/PackageCache
# Sort to pick the latest version if multiple exist
candidates=("$PROJECT_ROOT"/Library/PackageCache/com.pereviader.unityclirunner@*/CLI~/unitycli.sh)
if [ ${#candidates[@]} -gt 0 ] && [ -f "${candidates[0]}" ]; then
  SCRIPT_PATH="${candidates[${#candidates[@]}-1]}"
# 2. Check in Packages (local/development package)
elif [ -f "$PROJECT_ROOT/Packages/com.pereviader.unityclirunner/CLI~/unitycli.sh" ]; then
  SCRIPT_PATH="$PROJECT_ROOT/Packages/com.pereviader.unityclirunner/CLI~/unitycli.sh"
fi

if [ -z "$SCRIPT_PATH" ] || [ ! -f "$SCRIPT_PATH" ]; then
  echo "Error: Could not find unitycli.sh in Library/PackageCache/com.pereviader.unityclirunner@* or Packages/com.pereviader.unityclirunner/" >&2
  echo "Please ensure com.pereviader.unityclirunner is installed in your project." >&2
  exit 1
fi

chmod +x "$SCRIPT_PATH" 2>/dev/null || true
export UNITY_CLI_PROJECT_ROOT="$PROJECT_ROOT"
exec "$SCRIPT_PATH" "$@"
