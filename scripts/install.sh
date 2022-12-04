#!/bin/bash

echo "Installing neuRay Radar Management Service (RMS)..."

docker load < rms-webservice.tar
docker load < rms-web_app.tar

docker-compose up -d

echo "Install script done."