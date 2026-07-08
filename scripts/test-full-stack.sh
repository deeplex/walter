#!/usr/bin/env bash
# Run the end-to-end Playwright suite against a running backend.
#
# The caller is responsible for having built the frontend bundle, the backend
# binaries (or a published binary), and installed Playwright browsers. This
# script only:
#   1. waits for Postgres,
#   2. seeds the dev databases via bootstrap-dev.sh,
#   3. starts the backend,
#   4. runs `yarn test:e2e`.
#
# Backend startup mode:
#   - if WALTER_APP_BINARY is set, that file is executed directly,
#   - otherwise `dotnet run --no-build` is used (the WebAPI must already be built).
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

DB_HOST="${DATABASE_HOST:-db}"
DB_PORT="${DATABASE_PORT:-5432}"
DB_USER="${DATABASE_USER:-postgres}"
DB_PASS="${DATABASE_PASS:-postgres}"
DB_NAME="${DATABASE_NAME:-walter_dev_generic_db}"
S3_HOST="${S3_HOST:-s3}"
S3_PORT="${S3_PORT:-9000}"
S3_BUCKET="${S3_BUCKET:-saverwalter}"
S3_PROVIDER="${S3_PROVIDER:-http://${S3_HOST}:${S3_PORT}/${S3_BUCKET}}"
APP_URL="${WALTER_APP_URL:-http://localhost:5254}"
WALTER_E2E_PASSWORD="${WALTER_E2E_PASSWORD:-$DB_PASS}"
APP_BINARY="${WALTER_APP_BINARY:-}"
STARTUP_TIMEOUT_SECONDS="${WALTER_STARTUP_TIMEOUT_SECONDS:-180}"
LOGS_DIR="${WALTER_LOG_DIR:-}"
PRESERVE_LOGS_ON_FAILURE="${WALTER_PRESERVE_LOGS_ON_FAILURE:-0}"

if [[ -z "$LOGS_DIR" ]]; then
  LOGS_DIR="$(mktemp -d "${TMPDIR:-/tmp}/walter-full-stack.XXXXXX")"
  LOGS_DIR_AUTO_CREATED=1
else
  mkdir -p "$LOGS_DIR"
  LOGS_DIR_AUTO_CREATED=0
fi

BACKEND_LOG_PATH="$LOGS_DIR/backend.log"

backend_pid=""

cleanup() {
  local status=$?

  if [[ -n "$backend_pid" ]] && kill -0 "$backend_pid" >/dev/null 2>&1; then
    kill "$backend_pid" >/dev/null 2>&1 || true
  fi

  if [[ "$LOGS_DIR_AUTO_CREATED" == "1" ]]; then
    if [[ "$status" -eq 0 || "$PRESERVE_LOGS_ON_FAILURE" != "1" ]]; then
      rm -rf "$LOGS_DIR"
    fi
  fi

  return "$status"
}

trap cleanup EXIT INT TERM

wait_for_tcp() {
  local host="$1"
  local port="$2"

  for _ in {1..120}; do
    if (echo >"/dev/tcp/${host}/${port}") >/dev/null 2>&1; then
      return 0
    fi

    sleep 1
  done

  return 1
}

process_state() {
  ps -o stat= -p "$1" 2>/dev/null | awk '{print $1}'
}

process_is_running() {
  local state
  state="$(process_state "$1")"

  [[ -n "$state" && "$state" != Z* ]]
}

url_host() {
  echo "$1" | sed -E 's#^[a-zA-Z]+://([^/:]+).*$#\1#'
}

url_port() {
  local port
  port="$(echo "$1" | sed -nE 's#^[a-zA-Z]+://[^/:]+:([0-9]+).*$#\1#p')"

  if [[ -n "$port" ]]; then
    echo "$port"
    return 0
  fi

  if [[ "$1" == https://* ]]; then
    echo "443"
  else
    echo "80"
  fi
}

bind_host() {
  local host
  host="$(url_host "$1")"

  case "$host" in
    localhost|127.0.0.1|::1)
      echo "127.0.0.1"
      ;;
    *)
      echo "0.0.0.0"
      ;;
  esac
}

# When the backend binds to 0.0.0.0 in CI, probing localhost can resolve to ::1
# first and miss an IPv4-only listener. Normalize the script-side readiness
# check to the IPv4 loopback while keeping external URLs unchanged.
probe_url() {
  echo "$1" | sed -E 's#^([a-zA-Z]+://)localhost([/:]|$)#\1127.0.0.1\2#'
}

ensure_local_port_is_free() {
  local host="$1"
  local port="$2"
  local label="$3"

  if (echo >"/dev/tcp/${host}/${port}") >/dev/null 2>&1; then
    echo "${label} port ${host}:${port} is already in use."
    echo "Stop the existing process or override the URL via WALTER_APP_URL before rerunning."
    exit 1
  fi
}

print_startup_diagnostics() {
  local label="$1"
  local backend_state="<none>"

  if [[ -n "${backend_pid:-}" ]]; then
    backend_state="$(process_state "$backend_pid")"
    if [[ -z "$backend_state" ]]; then
      backend_state="<exited>"
    fi
  fi

  echo "${label} diagnostics:"
  echo "  Backend PID: ${backend_pid:-<none>}"
  echo "  Backend state: ${backend_state}"
  echo "  Backend probe URL: ${APP_PROBE_URL:-<unset>}"
  echo "  Log directory: ${LOGS_DIR}"
  echo "  Process table entries:"
  if [[ -n "${backend_pid:-}" ]]; then
    ps -fp "$backend_pid" 2>/dev/null || true
  fi
  echo "  Listening sockets:"
  ss -lntp 2>/dev/null || true
  echo "  Backend log tail:"
  tail -n 80 "$BACKEND_LOG_PATH" 2>/dev/null || true
}

echo "[1/4] Waiting for Postgres at ${DB_HOST}:${DB_PORT}"
if ! wait_for_tcp "$DB_HOST" "$DB_PORT"; then
  echo "Postgres is not reachable at ${DB_HOST}:${DB_PORT}."
  exit 1
fi

echo "[2/4] Seeding dev databases"
DATABASE_HOST="$DB_HOST" \
DATABASE_PORT="$DB_PORT" \
DATABASE_USER="$DB_USER" \
DATABASE_PASS="$DB_PASS" \
S3_HOST="$S3_HOST" \
S3_PORT="$S3_PORT" \
S3_BUCKET="$S3_BUCKET" \
S3_PROVIDER="$S3_PROVIDER" \
  ./scripts/bootstrap-dev.sh

APP_HOST="$(url_host "$APP_URL")"
APP_PORT="$(url_port "$APP_URL")"
APP_BIND_HOST="$(bind_host "$APP_URL")"
APP_PROBE_URL="$(probe_url "$APP_URL")"

ensure_local_port_is_free "$APP_HOST" "$APP_PORT" "Backend"

echo "Backend bind host: ${APP_BIND_HOST}"
echo "Backend probe URL: ${APP_PROBE_URL}"
echo "Startup logs directory: ${LOGS_DIR}"

echo "[3/4] Starting backend"
(
  export DATABASE_HOST="$DB_HOST"
  export DATABASE_PORT="$DB_PORT"
  export DATABASE_NAME="$DB_NAME"
  export DATABASE_USER="$DB_USER"
  export DATABASE_PASS="$DB_PASS"
  export WALTER_PASSWORD="$DB_PASS"
  export S3_PROVIDER="$S3_PROVIDER"
  export ASPNETCORE_ENVIRONMENT="Development"
  export ASPNETCORE_URLS="http://${APP_BIND_HOST}:${APP_PORT}"

  if [[ -n "$APP_BINARY" ]]; then
    cd "$(dirname "$APP_BINARY")"
    exec "./$(basename "$APP_BINARY")"
  else
    exec dotnet run --no-build --project Deeplex.Saverwalter.WebAPI/Deeplex.Saverwalter.WebAPI.csproj --no-launch-profile
  fi
) >"$BACKEND_LOG_PATH" 2>&1 &
backend_pid="$!"

backend_ready=false
for _ in $(seq 1 "$STARTUP_TIMEOUT_SECONDS"); do
  if ! process_is_running "$backend_pid"; then
    echo "Backend process exited unexpectedly."
    print_startup_diagnostics "Backend failure"
    exit 1
  fi
  if curl --fail --silent "$APP_PROBE_URL/login" >/dev/null 2>&1; then
    backend_ready=true
    break
  fi
  sleep 1
done

if [[ "$backend_ready" != "true" ]]; then
  echo "Backend did not become ready at $APP_URL"
  print_startup_diagnostics "Backend timeout"
  exit 1
fi

echo "[4/4] Running Playwright end-to-end suite"
(
  cd Deeplex.Saverwalter.WebAPI/svelte
  PLAYWRIGHT_BASE_URL="$APP_URL" \
  WALTER_E2E_PASSWORD="$WALTER_E2E_PASSWORD" \
    yarn test:e2e
)

echo "Full-stack test run completed successfully."
