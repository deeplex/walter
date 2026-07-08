#!/usr/bin/env bash
# Run the GitHub Actions CI workflow locally via `act`.
#
# Examples:
#   ./scripts/ci.sh                  # run the whole reusable_ci workflow
#   ./scripts/ci.sh -j style-svelte  # run a single job
#   ./scripts/ci.sh -l               # list jobs
set -euo pipefail

if ! command -v act >/dev/null 2>&1; then
  echo "act is not installed. See https://nektosact.com/installation/ (or: brew install act)." >&2
  exit 1
fi

if ! command -v docker >/dev/null 2>&1; then
  echo "Docker is required by act but was not found on PATH." >&2
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

exec act -W .github/workflows/reusable_ci.yaml workflow_call "$@"
