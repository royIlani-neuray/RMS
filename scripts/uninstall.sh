#!/bin/bash

echo "Removing Radar Management Service (RMS) installation..."

docker compose down
docker compose rm

docker rmi webservice
docker rmi web_app
docker volume rm rms_webservice_storage

