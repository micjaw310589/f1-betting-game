#!/usr/bin/env bash
set -euo pipefail
# Convenience wrapper to run the openf1 CLI inside docker-compose
docker compose run --rm openf1-cli "$@"