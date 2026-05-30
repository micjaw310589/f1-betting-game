#!/usr/bin/env python3
import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
MANIFEST = ROOT / "openf1" / "endpoints.json"

if not MANIFEST.exists():
    print(f"endpoints.json not found at {MANIFEST}", file=sys.stderr)
    sys.exit(1)

with MANIFEST.open("r", encoding="utf-8") as fh:
    endpoints = json.load(fh)

exit_code = 0

for ep in endpoints:
    print(f"Testing endpoint: {ep}")
    cmd = ["docker", "compose", "run", "--rm", "openf1-cli", ep, "--params", "session_key=latest"]
    try:
        proc = subprocess.run(cmd, capture_output=True, text=True, timeout=120)
    except Exception as e:
        print(f"Error running command for {ep}: {e}", file=sys.stderr)
        exit_code = 1
        continue
    if proc.returncode != 0:
        print(f"FAIL: {ep}", file=sys.stderr)
        print(proc.stderr, file=sys.stderr)
        exit_code = 1
    else:
        print(f"OK: {ep} (stdout truncated)")
        out = proc.stdout
        print(out[:1000])
print("Integration test run complete.")
sys.exit(exit_code)