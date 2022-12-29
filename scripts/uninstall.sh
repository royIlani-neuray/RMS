#!/bin/bash

echo "Removing Radar Management Service (RMS) installation..."

docker compose down
docker compose rm

docker rmi web_app
docker rmi web_service
docker rmi inference_service
docker rmi device_emulator

while true; do

read -p "Do you want to delete stored data (docker volumes)? (y/n) " yn

case $yn in 
	[yY] ) echo deleting RMS data volumes...;
        docker volume rm rms_webservice_storage
        docker volume rm rms_webservice_recordings
		docker volume rm rms_inference_service_storage
		break;;
	[nN] ) echo keeping RMS data volumes.;
		exit;;
	* ) echo invalid response;;
esac

echo "Uninstall script done."
done