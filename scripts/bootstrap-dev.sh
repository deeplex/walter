#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

DB_HOST="${DATABASE_HOST:-db}"
DB_PORT="${DATABASE_PORT:-5432}"
DB_USER="${DATABASE_USER:-postgres}"
DB_PASS="${DATABASE_PASS:-postgres}"
WALTER_DEV_TARGET_WOHNUNGEN="${WALTER_DEV_TARGET_WOHNUNGEN:-120}"
WALTER_DEV_FULL_TARGET_WOHNUNGEN="${WALTER_DEV_FULL_TARGET_WOHNUNGEN:-320}"
WALTER_DEV_RANDOM_SEED="${WALTER_DEV_RANDOM_SEED:-1337}"

echo "[1/4] Waiting for Postgres at ${DB_HOST}:${DB_PORT}"
ready=0
for _ in {1..120}; do
  if (echo >"/dev/tcp/${DB_HOST}/${DB_PORT}") >/dev/null 2>&1; then
    ready=1
    break
  fi

  sleep 1
done

if [[ "$ready" -ne 1 ]]; then
  echo "Postgres at ${DB_HOST}:${DB_PORT} did not become ready in time."
  echo "Make sure the devcontainer services are running."
  exit 1
fi

echo "[2/4] Restoring .NET dependencies"
dotnet restore

echo "[3/4] Installing frontend dependencies"
(
  cd Deeplex.Saverwalter.WebAPI/svelte
  yarn install --immutable || yarn install
)

echo "[4/4] Seeding realistic development databases"
echo "Using DB connection: ${DB_HOST}:${DB_PORT} (user: ${DB_USER})"
DATABASE_HOST="$DB_HOST" \
DATABASE_PORT="$DB_PORT" \
DATABASE_USER="$DB_USER" \
DATABASE_PASS="$DB_PASS" \
WALTER_DEV_TARGET_WOHNUNGEN="$WALTER_DEV_TARGET_WOHNUNGEN" \
WALTER_DEV_FULL_TARGET_WOHNUNGEN="$WALTER_DEV_FULL_TARGET_WOHNUNGEN" \
WALTER_DEV_RANDOM_SEED="$WALTER_DEV_RANDOM_SEED" \
  dotnet run --project Deeplex.Saverwalter.InitiateTestDbs/Deeplex.Saverwalter.InitiateTestDbs.csproj --no-launch-profile

echo ""
echo "Development environment is prepared."
echo "============================================================"
echo "DEV LOGIN CREDENTIALS (for UI + API tests)"
echo "Password for all seeded dev users: $DB_PASS"
echo "- $DB_USER"
echo "- admin.dev"
echo "- owner.dev"
echo "- manager.dev"
echo "- viewer.dev"
echo "- limited.dev"
echo "============================================================"
echo "Suggested next steps:"
echo "  ASPNETCORE_URLS=http://localhost:5254 DATABASE_HOST=$DB_HOST DATABASE_PORT=$DB_PORT DATABASE_NAME=walter_dev_generic_db DATABASE_USER=$DB_USER DATABASE_PASS=$DB_PASS WALTER_PASSWORD=$DB_PASS dotnet watch run --project Deeplex.Saverwalter.WebAPI/Deeplex.Saverwalter.WebAPI.csproj --no-launch-profile"
echo "  (cd Deeplex.Saverwalter.WebAPI/svelte && yarn dev)"
