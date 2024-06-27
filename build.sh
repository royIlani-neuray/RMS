#!/bin/bash

docker compose rm

docker rmi web_service
docker rmi web_app
docker rmi device_emulator
# docker rmi inference_service

docker volume rm rms_webservice_storage
docker volume rm rms_webservice_recordings
# docker volume rm inference_service_storage

rm -rf bin/rms
rm -rf bin/neuRay_rms.tar.gz

docker compose build

mkdir bin
mkdir bin/rms

docker save --output ./bin/rms/rms-web_service.tar web_service
docker save --output ./bin/rms/rms-web_app.tar web_app
docker save --output ./bin/rms/rms-device_emulator.tar device_emulator
# docker save --output ./bin/rms/rms-inference_service.tar inference_service

cp ./scripts/install.sh ./bin/rms
cp ./scripts/uninstall.sh ./bin/rms
cp ./scripts/start.sh ./bin/rms
cp ./scripts/stop.sh ./bin/rms
cp ./docker-compose.yml ./bin/rms
cp ./rms-variables.env ./bin/rms
cp ./s3-secrets-template.env ./bin/rms/s3-secrets.env

cd bin
tar -czvf neuRay_rms.tar.gz rms
cd ..

