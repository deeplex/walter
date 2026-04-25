# Walter

## Development bootstrap with realistic demo data

Use the bootstrap script from inside the devcontainer to prepare the development data.
It assumes the devcontainer services are already up, then installs dependencies and seeds two realistic development databases:

- `walter_dev_generic_db` (default 120 apartments)
- `walter_dev_full_generic_db` (default 320 apartments)

Run:

```bash
./scripts/bootstrap-dev.sh
```

Optional tuning:

```bash
WALTER_DEV_TARGET_WOHNUNGEN=90 \
WALTER_DEV_FULL_TARGET_WOHNUNGEN=250 \
WALTER_DEV_RANDOM_SEED=4242 \
./scripts/bootstrap-dev.sh
```

The script prints the exact `dotnet watch` and frontend start commands afterwards.
It also prints an access overview for seeded users and their apartment visibility.
It now prints a high-visibility credential block with all seeded usernames and the shared password.

Seeded development users (default password = `DATABASE_PASS`):

- primary DB user (`DATABASE_USER`)
- `admin.dev`
- `owner.dev`
- `manager.dev`
- `viewer.dev`
- `limited.dev`

## Run Full Dev Watch Stack (Inside Dev Container)

Use this script to start both backend and frontend in watch mode, connected to your seeded development database:

```bash
./scripts/dev-watch-stack.sh
```

Defaults:

- Backend DB: `walter_dev_generic_db`
- Frontend URL: `http://localhost:5173`

Useful options:

```bash
DATABASE_NAME=walter_dev_full_generic_db ./scripts/dev-watch-stack.sh
```

```bash
WALTER_BOOTSTRAP=1 ./scripts/dev-watch-stack.sh
```

`WALTER_BOOTSTRAP=1` runs the full bootstrap script first before launching watch mode.
On every run, the watch script also ensures dev users/roles exist in the selected DB and prints a user access overview.
The watch script also prints a dedicated credential block (username list + shared password) for quick copy/paste testing.

## Playwright E2E (UI + API authorization)

From the frontend folder:

```bash
cd Deeplex.Saverwalter.WebAPI/svelte
yarn test:e2e
```

The suite contains both browser and API tests and checks:

- role-based sign-in for all seeded test users
- admin-only access for `/api/accounts`
- owner-only policy enforcement for `POST /api/wohnungen`
- apartment list visibility differences for manager/viewer/limited roles
- UI access to `/admin` for admin and error rendering for non-admin
- UI access to dynamic detail/new routes such as `/wohnungen/:id`, `/wohnungen/new`, `/accounts/:id`, and `/accounts/new`
- entity-level update restrictions based on backend permission flags

Required runtime setup before running tests:

- backend reachable at `PLAYWRIGHT_API_BASE_URL` (default `http://localhost:5254`)
- frontend reachable at `PLAYWRIGHT_BASE_URL` (default `http://localhost:5173`)
- shared test password in `WALTER_E2E_PASSWORD` (defaults to `postgres`)

## Full Stack Test Script

Run the entire stack locally from inside the dev container with:

```bash
./scripts/test-full-stack.sh
```

The script:

- waits for Postgres
- bootstraps and seeds the development databases
- runs all `.NET` tests
- runs frontend unit tests
- starts backend and frontend locally
- runs the Playwright API + UI authorization suite against the running stack

CI now includes a full-stack test job that runs this script on every main and feature branch test workflow.

VS Code tasks are available as well:

- `dev_watch_stack` starts backend + frontend watch directly
- `dev_watch_stack_with_bootstrap` runs bootstrap first, then starts the watch stack