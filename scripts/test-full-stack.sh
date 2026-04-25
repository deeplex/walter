#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

DB_HOST="${DATABASE_HOST:-db}"
DB_PORT="${DATABASE_PORT:-5432}"
DB_USER="${DATABASE_USER:-postgres}"
DB_PASS="${DATABASE_PASS:-postgres}"
DB_NAME="${DATABASE_NAME:-walter_dev_generic_db}"
API_URL="${PLAYWRIGHT_API_BASE_URL:-http://localhost:5254}"
UI_URL="${PLAYWRIGHT_BASE_URL:-http://localhost:5173}"
WALTER_E2E_PASSWORD="${WALTER_E2E_PASSWORD:-$DB_PASS}"
PLAYWRIGHT_BROWSER="${PLAYWRIGHT_BROWSER:-chromium}"
STARTUP_TIMEOUT_SECONDS="${WALTER_STARTUP_TIMEOUT_SECONDS:-180}"

backend_pid=""
frontend_pid=""

cleanup() {
  if [[ -n "$backend_pid" ]] && kill -0 "$backend_pid" >/dev/null 2>&1; then
    kill "$backend_pid" >/dev/null 2>&1 || true
  fi

  if [[ -n "$frontend_pid" ]] && kill -0 "$frontend_pid" >/dev/null 2>&1; then
    kill "$frontend_pid" >/dev/null 2>&1 || true
  fi
}

trap cleanup EXIT INT TERM

wait_for_tcp() {
  local host="$1"
  local port="$2"

  for _ in {1..120}; do
    if (echo >"/dev/tcp/${host}/${port}") >/dev/null 2>&1; then
      return 0
    fi

    read -r -t 1 _ || true
  done

  return 1
}

wait_for_http() {
  local url="$1"
  local seconds="${2:-180}"

  for _ in $(seq 1 "$seconds"); do
    if curl --fail --silent "$url" >/dev/null 2>&1; then
      return 0
    fi

    read -r -t 1 _ || true
  done

  return 1
}

wait_for_http_connection() {
  local url="$1"
  local seconds="${2:-180}"

  for _ in $(seq 1 "$seconds"); do
    # A non-2xx status is still acceptable for readiness; we only care that
    # an HTTP server is reachable on the configured URL.
    if curl --silent --show-error --output /dev/null "$url" >/dev/null 2>&1; then
      return 0
    fi

    read -r -t 1 _ || true
  done

  return 1
}

url_host() {
  local url="$1"
  echo "$url" | sed -E 's#^[a-zA-Z]+://([^/:]+).*$#\1#'
}

url_port() {
  local url="$1"
  local port
  port="$(echo "$url" | sed -nE 's#^[a-zA-Z]+://[^/:]+:([0-9]+).*$#\1#p')"

  if [[ -n "$port" ]]; then
    echo "$port"
    return 0
  fi

  if [[ "$url" == https://* ]]; then
    echo "443"
  else
    echo "80"
  fi
}

ensure_local_port_is_free() {
  local host="$1"
  local port="$2"
  local label="$3"

  if (echo >"/dev/tcp/${host}/${port}") >/dev/null 2>&1; then
    echo "${label} port ${host}:${port} is already in use."
    echo "Stop the existing process or override ${label} URL via env vars before rerunning."
    exit 1
  fi
}

echo "[1/7] Waiting for Postgres at ${DB_HOST}:${DB_PORT}"
if ! wait_for_tcp "$DB_HOST" "$DB_PORT"; then
  echo "Postgres is not reachable at ${DB_HOST}:${DB_PORT}."
  exit 1
fi

echo "[2/7] Bootstrapping dev databases and frontend dependencies"
DATABASE_HOST="$DB_HOST" \
DATABASE_PORT="$DB_PORT" \
DATABASE_USER="$DB_USER" \
DATABASE_PASS="$DB_PASS" \
  ./scripts/bootstrap-dev.sh

echo "[3/7] Running .NET test suite"
dotnet test

echo "[4/7] Running frontend unit tests"
(
  cd Deeplex.Saverwalter.WebAPI/svelte
  yarn vitest run --coverage
  yarn playwright install "$PLAYWRIGHT_BROWSER"
)

API_HOST="$(url_host "$API_URL")"
API_PORT="$(url_port "$API_URL")"
UI_HOST="$(url_host "$UI_URL")"
UI_PORT="$(url_port "$UI_URL")"

ensure_local_port_is_free "$API_HOST" "$API_PORT" "Backend"
ensure_local_port_is_free "$UI_HOST" "$UI_PORT" "Frontend"

echo "[5/7] Starting backend"
(
  export DATABASE_HOST="$DB_HOST"
  export DATABASE_PORT="$DB_PORT"
  export DATABASE_NAME="$DB_NAME"
  export DATABASE_USER="$DB_USER"
  export DATABASE_PASS="$DB_PASS"
  export WALTER_PASSWORD="$DB_PASS"
  export ASPNETCORE_ENVIRONMENT="Development"
  export ASPNETCORE_URLS="http://0.0.0.0:${API_PORT}"
  dotnet run --project Deeplex.Saverwalter.WebAPI/Deeplex.Saverwalter.WebAPI.csproj --no-launch-profile
) >/tmp/walter-backend.log 2>&1 &
backend_pid="$!"

echo "[6/7] Starting frontend"
(
  cd Deeplex.Saverwalter.WebAPI/svelte
  yarn dev --host 0.0.0.0 --port "$UI_PORT" --strictPort
) >/tmp/walter-frontend.log 2>&1 &
frontend_pid="$!"

if ! wait_for_http_connection "$API_URL" "$STARTUP_TIMEOUT_SECONDS"; then
  echo "Backend did not become ready at $API_URL"
  tail -n 80 /tmp/walter-backend.log || true
  exit 1
fi

if ! wait_for_http "$UI_URL/login" "$STARTUP_TIMEOUT_SECONDS"; then
  echo "Frontend did not become ready at $UI_URL"
  tail -n 80 /tmp/walter-frontend.log || true
  exit 1
fi

echo "[7/7] Running Playwright end-to-end suite"
(
  cd Deeplex.Saverwalter.WebAPI/svelte
  PLAYWRIGHT_BASE_URL="$UI_URL" \
  PLAYWRIGHT_API_BASE_URL="$API_URL" \
  WALTER_E2E_PASSWORD="$WALTER_E2E_PASSWORD" \
    yarn test:e2e
)

echo "Full-stack test run completed successfully."