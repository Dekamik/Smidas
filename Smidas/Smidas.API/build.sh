#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

TAG=${1}

docker build -f ./Dockerfile -t dekamik/smidas:${TAG} ..
