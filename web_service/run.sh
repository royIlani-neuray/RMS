#!/bin/bash
set -a  # pushes the following variables to current environment
source ../s3-secrets.env
set +a
dotnet watch run --urls=http://0.0.0.0:16500/
