FROM python:3.11-slim
WORKDIR /app
COPY openf1_cli.py requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt
ENTRYPOINT ["python", "openf1_cli.py"]