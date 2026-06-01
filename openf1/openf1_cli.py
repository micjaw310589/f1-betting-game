#!/usr/bin/env python3
import os
import sys
import argparse
import requests
import json
import csv
import time
from typing import Any, Dict, List

BASE_URL = os.getenv("OPENF1_BASE_URL", "https://openf1.org/api")
DEFAULT_TIMEOUT = 10
MAX_RETRIES = 4
BACKOFF_FACTOR = 1.5

# Simple DRS and segment decoders (extendable)
DRS_MAP = {
    0: "Unknown",
    1: "Enabled",
    2: "Disabled",
    8: "Not Available",
    10: "Open",
    12: "Closed",
    14: "Penalty",
}

SEGMENT_MAP = {
    "s1": "Sector 1",
    "s2": "Sector 2",
    "s3": "Sector 3",
    "best": "Best Lap",
    "last": "Last Lap",
}


def retry_request(session: requests.Session, url: str, params: Dict[str, Any]) -> requests.Response:
    attempt = 0
    while True:
        try:
            resp = session.get(url, params=params, timeout=DEFAULT_TIMEOUT)
            if resp.status_code == 429:
                # Rate limited; respect Retry-After if present
                ra = resp.headers.get("Retry-After")
                wait = int(ra) if ra and ra.isdigit() else (2 ** attempt)
                print(f"Rate limited, sleeping for {wait}s...", file=sys.stderr)
                time.sleep(wait)
                attempt += 1
                if attempt >= MAX_RETRIES:
                    resp.raise_for_status()
                continue
            resp.raise_for_status()
            return resp
        except (requests.RequestException, ValueError) as e:
            attempt += 1
            if attempt > MAX_RETRIES:
                raise
            backoff = (BACKOFF_FACTOR ** attempt)
            time.sleep(backoff)


def parse_params(param_list: List[str]) -> Dict[str, str]:
    params: Dict[str, str] = {}
    if not param_list:
        return params
    for item in param_list:
        # allow comma separated pairs in a single --params
        parts = item.split(",")
        for p in parts:
            if "=" in p:
                k, v = p.split("=", 1)
                params[k.strip()] = v.strip()
            elif p.strip():
                # flag-like param -> set to true
                params[p.strip()] = "true"
    return params


def decode_drs_in_obj(obj: Any):
    if isinstance(obj, dict):
        for k, v in obj.items():
            if k.lower().endswith("drs") or k.lower() == "drs":
                try:
                    code = int(v)
                    obj[k] = {"code": code, "decoded": DRS_MAP.get(code, "Unknown")}
                except Exception:
                    obj[k] = v
            else:
                decode_drs_in_obj(v)
    elif isinstance(obj, list):
        for i in obj:
            decode_drs_in_obj(i)


def decode_segments_in_obj(obj: Any):
    if isinstance(obj, dict):
        for k, v in obj.items():
            if isinstance(v, dict) and any(x in v for x in SEGMENT_MAP.keys()):
                # decode known segment keys
                for sk in list(v.keys()):
                    if sk in SEGMENT_MAP:
                        v[f"{SEGMENT_MAP[sk]}"] = v.pop(sk)
                obj[k] = v
            else:
                decode_segments_in_obj(v)
    elif isinstance(obj, list):
        for i in obj:
            decode_segments_in_obj(i)


def extract_list_from_response(data: Any) -> List[Dict[str, Any]]:
    # Try common keys that hold lists
    if isinstance(data, list):
        return data
    if not isinstance(data, dict):
        return [ {"value": data} ]
    for key in ("data", "items", "results", "drivers", "races", "sessions"):
        if key in data and isinstance(data[key], list):
            return data[key]
    # Fallback: find first list in dict values
    for v in data.values():
        if isinstance(v, list):
            return v
    return [data]


def to_csv(data: Any, out_path: str):
    records = extract_list_from_response(data)
    if not records:
        raise ValueError("No records to write as CSV")
    # compute fieldnames as union of keys
    fieldnames = set()
    for r in records:
        if isinstance(r, dict):
            fieldnames.update(r.keys())
    fieldnames = list(fieldnames)
    if out_path:
        f = open(out_path, "w", newline="", encoding="utf-8")
    else:
        f = sys.stdout
    writer = csv.DictWriter(f, fieldnames=fieldnames)
    writer.writeheader()
    for r in records:
        if isinstance(r, dict):
            writer.writerow({k: (json.dumps(v, ensure_ascii=False) if isinstance(v, (dict, list)) else v) for k, v in r.items()})
        else:
            writer.writerow({"value": r})
    if out_path:
        f.close()


def main():
    parser = argparse.ArgumentParser(prog="openf1-cli", description="OpenF1 CLI tool")
    parser.add_argument("endpoint", nargs="?", help="API endpoint to call (e.g. drivers, races, sessions)")
    parser.add_argument("--list-endpoints", action="store_true", help="List supported endpoints and exit")
    parser.add_argument("--params", "-p", action="append", help="Query params as key=value or comma separated list", metavar="key=value")
    parser.add_argument("--format", "-f", choices=("json", "csv"), default="json", help="Output format")
    parser.add_argument("--out", "-o", help="Write output to file")
    parser.add_argument("--decode-drs", action="store_true", help="Decode DRS values")
    parser.add_argument("--decode-segments", action="store_true", help="Decode segments")
    parser.add_argument("--raw", action="store_true", help="Print raw response and exit")
    args = parser.parse_args()

    # load endpoints manifest if available
    endpoints = []
    try:
        manifest_path = os.path.join(os.path.dirname(__file__), "endpoints.json")
        if os.path.exists(manifest_path):
            with open(manifest_path, "r", encoding="utf-8") as fh:
                endpoints = json.load(fh)
    except Exception:
        endpoints = []

    if args.list_endpoints:
        if endpoints:
            print(json.dumps(endpoints, ensure_ascii=False, indent=2))
        else:
            print("No endpoints manifest found.")
        return

    if not args.endpoint:
        print("No endpoint specified. Use --list-endpoints to see available endpoints.", file=sys.stderr)
        sys.exit(1)

    params = parse_params(args.params)
    endpoint = args.endpoint.strip().lstrip("/")

    # validate endpoint name if manifest present
    if endpoints and endpoint not in endpoints:
        print(f"Unknown endpoint '{endpoint}'. Use --list-endpoints to see supported endpoints.", file=sys.stderr)
        sys.exit(2)

    url = BASE_URL.rstrip("/") + "/" + endpoint

    session = requests.Session()
    try:
        resp = retry_request(session, url, params)
    except Exception as e:
        print(f"Request failed: {e}", file=sys.stderr)
        sys.exit(2)

    try:
        data = resp.json()
    except ValueError:
        text = resp.text
        if args.out:
            with open(args.out, "w", encoding="utf-8") as fh:
                fh.write(text)
            print(f"Wrote raw response to {args.out}")
            return
        else:
            print(text)
            return

    if args.raw:
        print(json.dumps(data, ensure_ascii=False))
        return

    if args.decode_drs:
        decode_drs_in_obj(data)
    if args.decode_segments:
        decode_segments_in_obj(data)

    if args.format == "json":
        out_text = json.dumps(data, ensure_ascii=False, indent=2)
        if args.out:
            with open(args.out, "w", encoding="utf-8") as fh:
                fh.write(out_text)
            print(f"Wrote JSON to {args.out}")
        else:
            print(out_text)
    else:
        try:
            to_csv(data, args.out)
            if args.out:
                print(f"Wrote CSV to {args.out}")
        except Exception as e:
            print(f"CSV output failed: {e}", file=sys.stderr)
            sys.exit(3)


if __name__ == "__main__":
    main()