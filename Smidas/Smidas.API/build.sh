#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

docker build -f ./Dockerfile -t dekamik/smidas:latest ..
