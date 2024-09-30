#!/bin/bash

docker build -t models-service-image .
docker run -it --rm --network=host --name models-service models-service-image
