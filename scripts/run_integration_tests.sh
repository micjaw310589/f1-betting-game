#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
MANIFEST="$ROOT/openf1/endpoints.json"

if [ ! -f "$MANIFEST" ]; then
  echo "endpoints.json not found at $MANIFEST" >&2
  exit 1
fi

ENDPOINTS=$(jq -r '.[]' "$MANIFEST")
FAIL=0

for ep in $ENDPOINTS; do
  echo "Testing endpoint: $ep"
  if ! docker compose run --rm openf1-cli "$ep" --params session_key=latest >/tmp/openf1_${ep}.out 2>/tmp/openf1_${ep}.err; then
    echo "FAIL: $ep"
    echo "stderr:"
    cat /tmp/openf1_${ep}.err
    FAIL=1
  else
    echo "OK: $ep (output saved to /tmp/openf1_${ep}.out)"
  fi
done

exit $FAIL