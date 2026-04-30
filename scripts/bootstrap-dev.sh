#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

DB_HOST="${DATABASE_HOST:-db}"
DB_PORT="${DATABASE_PORT:-5432}"
DB_USER="${DATABASE_USER:-postgres}"
DB_PASS="${DATABASE_PASS:-postgres}"
S3_HOST="${S3_HOST:-s3}"
S3_PORT="${S3_PORT:-9000}"
S3_BUCKET="${S3_BUCKET:-saverwalter}"
S3_PROVIDER="${S3_PROVIDER:-http://${S3_HOST}:${S3_PORT}/${S3_BUCKET}}"
WALTER_DEV_TARGET_WOHNUNGEN="${WALTER_DEV_TARGET_WOHNUNGEN:-120}"
WALTER_DEV_FULL_TARGET_WOHNUNGEN="${WALTER_DEV_FULL_TARGET_WOHNUNGEN:-320}"
WALTER_DEV_RANDOM_SEED="${WALTER_DEV_RANDOM_SEED:-1337}"
WALTER_DEV_SKIP_FILE_SEED="${WALTER_DEV_SKIP_FILE_SEED:-0}"

wait_for_tcp() {
  local host="$1"
  local port="$2"
  local label="$3"
  local timeout_seconds="${4:-120}"

  for _ in $(seq 1 "$timeout_seconds"); do
    if (echo >"/dev/tcp/${host}/${port}") >/dev/null 2>&1; then
      return 0
    fi
    sleep 1
  done

  echo "${label} at ${host}:${port} did not become ready within ${timeout_seconds}s."
  return 1
}

echo "[1/5] Waiting for Postgres at ${DB_HOST}:${DB_PORT}"
if ! wait_for_tcp "$DB_HOST" "$DB_PORT" "Postgres"; then
  echo "Make sure the devcontainer services are running."
  exit 1
fi

echo "[2/5] Restoring .NET dependencies"
dotnet restore

echo "[3/5] Installing frontend dependencies"
(
  cd Deeplex.Saverwalter.WebAPI/svelte
  yarn install --immutable || yarn install
)

echo "[4/5] Seeding realistic development databases"
echo "Using DB connection: ${DB_HOST}:${DB_PORT} (user: ${DB_USER})"

# Probe MinIO; if it's reachable, hand the URL to the seeder so it can also
# upload realistic placeholder files. If not reachable (e.g. the user runs
# bootstrap outside the devcontainer), the seeder logs and skips.
seed_s3_provider=""
if [[ "$WALTER_DEV_SKIP_FILE_SEED" != "1" ]]; then
  if (echo >"/dev/tcp/${S3_HOST}/${S3_PORT}") >/dev/null 2>&1; then
    seed_s3_provider="$S3_PROVIDER"
    echo "MinIO reachable at ${S3_HOST}:${S3_PORT}; sample files will be uploaded to ${seed_s3_provider}"
  else
    echo "MinIO not reachable at ${S3_HOST}:${S3_PORT}; skipping file seeding."
    echo "Start the devcontainer s3 service or run 'docker compose up -d s3 s3-init' to enable file storage."
  fi
else
  echo "WALTER_DEV_SKIP_FILE_SEED=1 -> skipping file seeding."
fi

DATABASE_HOST="$DB_HOST" \
DATABASE_PORT="$DB_PORT" \
DATABASE_USER="$DB_USER" \
DATABASE_PASS="$DB_PASS" \
WALTER_DEV_TARGET_WOHNUNGEN="$WALTER_DEV_TARGET_WOHNUNGEN" \
WALTER_DEV_FULL_TARGET_WOHNUNGEN="$WALTER_DEV_FULL_TARGET_WOHNUNGEN" \
WALTER_DEV_RANDOM_SEED="$WALTER_DEV_RANDOM_SEED" \
WALTER_DEV_S3_PROVIDER="$seed_s3_provider" \
  dotnet run --project Deeplex.Saverwalter.InitiateTestDbs/Deeplex.Saverwalter.InitiateTestDbs.csproj --no-launch-profile

echo "[5/5] Development environment ready"
echo ""
echo "============================================================"
echo "DEV LOGIN CREDENTIALS (for UI + API tests)"
echo "Password for all seeded dev users: $DB_PASS"
echo "- $DB_USER"
echo "- admin.dev"
echo "- owner.dev"
echo "- manager.dev"
echo "- viewer.dev"
echo "- limited.dev"
echo "------------------------------------------------------------"
echo "FILE STORAGE (S3 / MinIO)"
echo "  S3_PROVIDER: ${S3_PROVIDER}"
echo "  MinIO console: http://localhost:9001 (login minioadmin / minioadmin)"
echo "============================================================"
echo "Suggested next steps:"
echo "  ./scripts/dev-watch-stack.sh    # backend + frontend in watch mode"
