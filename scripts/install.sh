#!/bin/bash

echo "Installing neuRay Radar Management Service (RMS)..."

docker load < rms-web_service.tar
docker load < rms-inference_service.tar
docker load < rms-web_app.tar
docker load < rms-device_emulator.tar

docker compose up -d

echo "Install script done."