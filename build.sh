#!/bin/bash

docker compose rm

docker rmi webservice
docker rmi web_app

docker volume rm rms_webservice_storage

rm -rf bin/rms
rm -rf bin/neuRay_rms.tar.gz

docker compose build

mkdir bin
mkdir bin/rms

docker save --output ./bin/rms/rms-webservice.tar webservice
docker save --output ./bin/rms/rms-web_app.tar web_app

cp ./scripts/install.sh ./bin/rms
cp ./scripts/uninstall.sh ./bin/rms
cp ./scripts/start.sh ./bin/rms
cp ./scripts/stop.sh ./bin/rms
cp ./docker-compose.yml ./bin/rms
cp ./rms-variables.env ./bin/rms

cd bin
tar -czvf neuRay_rms.tar.gz rms
cd ..

