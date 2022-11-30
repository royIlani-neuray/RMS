#!/bin/bash

echo "Removing Radar Management Service (RMS) installation..."

docker compose down
docker compose rm

docker rmi webservice
docker rmi web_app

while true; do

read -p "Do you want to delete stored data (docker volumes)? (y/n) " yn

case $yn in 
	[yY] ) echo deleting RMS data volumes...;
        docker volume rm rms_webservice_storage
        docker volume rm rms_webservice_recordings
		break;;
	[nN] ) echo keeping RMS data volumes.;
		exit;;
	* ) echo invalid response;;
esac

echo "Uninstall script done."
done