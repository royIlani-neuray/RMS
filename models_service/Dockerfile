FROM python:3

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY ./src ./src
COPY default_models /app/default_models
COPY ./config.json . 

RUN mkdir /app/data

CMD ["python", "src/main.py", "-config", "config.json" ]

