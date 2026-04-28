#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

DB_USER="${DATABASE_USER:-postgres}"
DB_PASS="${DATABASE_PASS:-postgres}"
DB_PORT="${DATABASE_PORT:-5432}"
DB_NAME="${DATABASE_NAME:-walter_dev_full_generic_db}"
PARITY_LOG_DIR="${WALTER_PARITY_LOG_DIR:-${ROOT_DIR}/artifacts/local-ci-logs}"
API_URL="${PLAYWRIGHT_API_BASE_URL:-http://localhost:15254}"
UI_URL="${PLAYWRIGHT_BASE_URL:-http://localhost:15173}"
PLAYWRIGHT_PARITY_INSTALL_DEPS="${PLAYWRIGHT_PARITY_INSTALL_DEPS:-1}"

mkdir -p "$PARITY_LOG_DIR"

echo "Running local CI parity for full-stack test workflow..."
echo "Log directory: $PARITY_LOG_DIR"
echo "API URL: $API_URL"
echo "UI URL: $UI_URL"

ensure_playwright_runtime() {
  local browser_bin

  browser_bin="$(compgen -G '/home/vscode/.cache/ms-playwright/chromium_headless_shell-*/chrome-headless-shell-linux64/chrome-headless-shell' | head -n 1 || true)"
  if [[ -n "$browser_bin" ]] && ! ldd "$browser_bin" 2>/dev/null | grep -q 'not found'; then
    echo "Playwright runtime already present; skipping dependency installation."
    return 0
  fi

  echo "Preparing Playwright runtime for parity run..."
  (
    cd Deeplex.Saverwalter.WebAPI/svelte
    yarn install --immutable

    if [[ "$PLAYWRIGHT_PARITY_INSTALL_DEPS" == "1" ]]; then
      if yarn playwright install --with-deps chromium; then
        return 0
      fi

      echo "Playwright '--with-deps' installation failed; retrying browser-only install."
      yarn playwright install chromium
    else
      yarn playwright install chromium
    fi
  )

  browser_bin="$(compgen -G '/home/vscode/.cache/ms-playwright/chromium_headless_shell-*/chrome-headless-shell-linux64/chrome-headless-shell' | head -n 1 || true)"
  if [[ -n "$browser_bin" ]] && ! ldd "$browser_bin" 2>/dev/null | grep -q 'not found'; then
    return 0
  fi

  echo "Playwright browser dependencies are still missing on this host."
  echo "Run with Docker-enabled host for full parity, or install missing libraries and retry."
  echo "Tip: if sudo is available, rerun with PLAYWRIGHT_PARITY_INSTALL_DEPS=1 (default)."
  exit 1
}

run_with_existing_db() {
  local host="${DATABASE_HOST:-localhost}"
  echo "Docker unavailable; using existing Postgres at ${host}:${DB_PORT}."

  DATABASE_HOST="$host" \
  DATABASE_PORT="$DB_PORT" \
  DATABASE_USER="$DB_USER" \
  DATABASE_PASS="$DB_PASS" \
  WALTER_E2E_PASSWORD="$DB_PASS" \
  PLAYWRIGHT_API_BASE_URL="$API_URL" \
  PLAYWRIGHT_BASE_URL="$UI_URL" \
  WALTER_LOG_DIR="$PARITY_LOG_DIR/full-stack" \
  WALTER_PRESERVE_LOGS_ON_FAILURE=1 \
    bash ./scripts/test-full-stack.sh
}

run_with_docker_db() {
  local container_name="walter-ci-parity-postgres"

  cleanup() {
    docker rm -f "$container_name" >/dev/null 2>&1 || true
  }
  trap cleanup EXIT INT TERM

  echo "Starting Postgres container ${container_name}..."
  docker rm -f "$container_name" >/dev/null 2>&1 || true
  docker run -d \
    --name "$container_name" \
    -e POSTGRES_DB="$DB_NAME" \
    -e POSTGRES_USER="$DB_USER" \
    -e POSTGRES_PASSWORD="$DB_PASS" \
    -p "$DB_PORT":5432 \
    postgres:16 >/dev/null

  echo "Waiting for Postgres health..."
  for _ in {1..60}; do
    status="$(docker inspect --format='{{.State.Health.Status}}' "$container_name" 2>/dev/null || true)"
    if [[ "$status" == "healthy" ]]; then
      break
    fi
    read -r -t 1 _ || true
  done

  status="$(docker inspect --format='{{.State.Health.Status}}' "$container_name")"
  if [[ "$status" != "healthy" ]]; then
    echo "Postgres container did not become healthy."
    docker logs "$container_name" || true
    exit 1
  fi

  DATABASE_HOST=localhost \
  DATABASE_PORT="$DB_PORT" \
  DATABASE_USER="$DB_USER" \
  DATABASE_PASS="$DB_PASS" \
  DATABASE_NAME="$DB_NAME" \
  WALTER_E2E_PASSWORD="$DB_PASS" \
  PLAYWRIGHT_API_BASE_URL="$API_URL" \
  PLAYWRIGHT_BASE_URL="$UI_URL" \
  WALTER_LOG_DIR="$PARITY_LOG_DIR/full-stack" \
  WALTER_PRESERVE_LOGS_ON_FAILURE=1 \
    bash ./scripts/test-full-stack.sh
}

if command -v docker >/dev/null 2>&1; then
  ensure_playwright_runtime
  run_with_docker_db
else
  ensure_playwright_runtime
  run_with_existing_db
fi

echo "Local CI parity run completed successfully."