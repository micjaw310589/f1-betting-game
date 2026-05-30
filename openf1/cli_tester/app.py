from flask import Flask, render_template, request, redirect, url_for
import os
import subprocess
import shlex

app = Flask(__name__, template_folder="templates")
OPENF1_CLI_PATH = os.getenv("OPENF1_CLI_PATH", "/app/openf1_cli.py")
OPENF1_BASE_URL = os.getenv("OPENF1_BASE_URL", "https://api.openf1.org/v1")


@app.route("/", methods=["GET", "POST"])
def index():
    output = None
    error = None
    if request.method == "POST":
        endpoint = request.form.get("endpoint", "").strip()
        params = request.form.get("params", "").strip()
        fmt = request.form.get("format", "json")
        decode_drs = " --decode-drs" if request.form.get("decode_drs") else ""
        decode_segments = " --decode-segments" if request.form.get("decode_segments") else ""
        out = ""  # no file output by default

        cmd = f'python {shlex.quote(OPENF1_CLI_PATH)} {shlex.quote(endpoint)}'
        if params:
            # pass as single --params argument
            cmd += f' --params {shlex.quote(params)}'
        if fmt:
            cmd += f' --format {shlex.quote(fmt)}'
        if decode_drs:
            cmd += decode_drs
        if decode_segments:
            cmd += decode_segments

        try:
            proc = subprocess.run(cmd, shell=True, capture_output=True, text=True, timeout=60, env=dict(os.environ, OPENF1_BASE_URL=OPENF1_BASE_URL))
            output = proc.stdout
            error = proc.stderr
        except subprocess.TimeoutExpired:
            error = "Command timed out"
        except Exception as e:
            error = str(e)

    return render_template("index.html", output=output, error=error, base_url=OPENF1_BASE_URL)


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)