#!/bin/bash

export RMS_RECORDING_PATH="../web_service/data/recordings"

dotnet run --urls=http://0.0.0.0:16503/
