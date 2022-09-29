#!/bin/bash

#docker run -it --rm radar_service /bin/bash
#docker run  -it --rm --network host --name radar_service radar_service

# to clear the storage:
# docker volume rm radar_roe_service_radar_service_storage

# save-load docker
#docker save --output image.tar image-name
#docker save --output radar_service.tar radar_service:latest
#docker load < image.tar


docker compose build
docker compose up
#docker compose rm


