#!/usr/bin/env bash
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
WALTER_PASSWORD="${WALTER_PASSWORD:-$DB_PASS}"
ASPNETCORE_URLS="${ASPNETCORE_URLS:-http://localhost:5254}"
FRONTEND_HOST="${FRONTEND_HOST:-0.0.0.0}"
FRONTEND_PORT="${FRONTEND_PORT:-5173}"
RUN_BOOTSTRAP="${WALTER_BOOTSTRAP:-0}"

if [[ "$RUN_BOOTSTRAP" == "1" ]]; then
  echo "Running bootstrap before starting watch stack..."
  DATABASE_HOST="$DB_HOST" \
  DATABASE_PORT="$DB_PORT" \
  DATABASE_USER="$DB_USER" \
  DATABASE_PASS="$DB_PASS" \
  S3_HOST="$S3_HOST" \
  S3_PORT="$S3_PORT" \
  S3_BUCKET="$S3_BUCKET" \
  S3_PROVIDER="$S3_PROVIDER" \
    ./scripts/bootstrap-dev.sh
fi

echo "Ensuring dev users/roles and printing access overview for ${DB_NAME}"
DATABASE_HOST="$DB_HOST" \
DATABASE_PORT="$DB_PORT" \
DATABASE_NAME="$DB_NAME" \
DATABASE_USER="$DB_USER" \
DATABASE_PASS="$DB_PASS" \
  dotnet run --project Deeplex.Saverwalter.InitiateTestDbs/Deeplex.Saverwalter.InitiateTestDbs.csproj --no-launch-profile -- --ensure-dev-users --print-access

s3_status="not reachable"
if (echo >"/dev/tcp/${S3_HOST}/${S3_PORT}") >/dev/null 2>&1; then
  s3_status="reachable at ${S3_HOST}:${S3_PORT}"
fi

echo "============================================================"
echo "DEV LOGIN CREDENTIALS (for UI + API tests)"
echo "Password for all seeded dev users: $WALTER_PASSWORD"
echo "- $DB_USER"
echo "- admin.dev"
echo "- owner.dev"
echo "- manager.dev"
echo "- viewer.dev"
echo "- limited.dev"
echo "------------------------------------------------------------"
echo "FILE STORAGE (S3 / MinIO)"
echo "  S3_PROVIDER: ${S3_PROVIDER}"
echo "  MinIO status: ${s3_status}"
echo "  MinIO console: http://localhost:9001 (login minioadmin / minioadmin)"
echo "============================================================"

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

echo "Starting backend watch with DATABASE_NAME=${DB_NAME} and S3_PROVIDER=${S3_PROVIDER}"
(
  export DATABASE_HOST="$DB_HOST"
  export DATABASE_PORT="$DB_PORT"
  export DATABASE_NAME="$DB_NAME"
  export DATABASE_USER="$DB_USER"
  export DATABASE_PASS="$DB_PASS"
  export WALTER_PASSWORD="$WALTER_PASSWORD"
  export S3_PROVIDER="$S3_PROVIDER"
  export ASPNETCORE_URLS="$ASPNETCORE_URLS"
  dotnet watch run --project Deeplex.Saverwalter.WebAPI/Deeplex.Saverwalter.WebAPI.csproj --no-launch-profile
) &
backend_pid="$!"

echo "Starting frontend watch on ${FRONTEND_HOST}:${FRONTEND_PORT}"
(
  cd Deeplex.Saverwalter.WebAPI/svelte
  yarn dev --host "$FRONTEND_HOST" --port "$FRONTEND_PORT"
) &
frontend_pid="$!"

echo ""
echo "Watch stack is running."
echo "Backend uses database: ${DB_NAME}"
echo "Backend URL: ${ASPNETCORE_URLS}"
echo "Frontend URL: http://localhost:${FRONTEND_PORT}"
echo "S3 provider: ${S3_PROVIDER}"
echo "Password for seeded dev accounts: ${WALTER_PASSWORD}"
if [[ "$RUN_BOOTSTRAP" != "1" ]]; then
  echo "Bootstrap was skipped (set WALTER_BOOTSTRAP=1 to run it automatically)."
fi
echo ""
echo "Press Ctrl+C to stop both processes."

wait -n "$backend_pid" "$frontend_pid"
