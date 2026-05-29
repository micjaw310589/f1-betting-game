# OpenF1 CLI & Tester Integration

Summary:
- CLI: openf1/openf1_cli.py
- Tester: openf1/cli_tester (Flask)
- Docker services: openf1-cli, cli-tester
- Env: .env (OPENF1_BASE_URL, OPENF1_CLI_PATH)

Usage examples:
- Run CLI directly:
  docker compose run --rm openf1-cli drivers --params session_key=latest
- Run tester:
  docker compose up -d cli-tester
  Open: http://localhost:5000
- Convenience wrapper:
  ./scripts/openf1.sh drivers --params session_key=latest

Notes:
- The CLI validates endpoints against openf1/endpoints.json (use --list-endpoints).
- The tester calls the CLI via subprocess using OPENF1_CLI_PATH.
- Ensure Docker daemon is running and .env exists before building.

Recommended next steps:
- Add integration test script to exercise documented endpoints.
- Add CI workflow to build images and run smoke tests.