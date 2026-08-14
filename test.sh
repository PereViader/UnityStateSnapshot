#!/usr/bin/env bash
set -euo pipefail

# Ensure script is run from repository root
cd "$(dirname "$0")"

echo "=== Running UnityStateSnapshot EditMode Test Suite ==="

# Run editmode tests via unitycli.sh
bash ./unitycli.sh test --editmode

echo "=== All tests passed successfully! ==="
