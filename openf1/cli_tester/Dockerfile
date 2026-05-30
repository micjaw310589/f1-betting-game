FROM python:3.11-slim
WORKDIR /app
# Copy the CLI and tester app from repository root
COPY openf1/openf1_cli.py /app/openf1_cli.py
COPY openf1/requirements.txt /app/openf1_requirements.txt
COPY openf1/cli_tester/requirements.txt /app/requirements.txt
COPY openf1/cli_tester /app/cli_tester
# Install tester requirements and CLI dependencies
RUN pip install --no-cache-dir -r requirements.txt && pip install --no-cache-dir -r openf1_requirements.txt
EXPOSE 5000
ENV OPENF1_CLI_PATH=/app/openf1_cli.py
ENTRYPOINT ["python", "cli_tester/app.py"]
