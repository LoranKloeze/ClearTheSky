#!/bin/bash
git pull
docker compose build --build-arg GIT_BRANCH=$(git rev-parse --abbrev-ref HEAD) --build-arg GIT_COMMIT=$(git rev-parse HEAD) --build-arg BUILD_DATETIME=$(date -u +"%Y-%m-%dT%H:%M:%SZ") --build-arg GIT_COMMIT_DATE=$(git log -1 --format=%cd --date=iso-strict-local | xargs -I{} date -u -d "{}" +"%Y-%m-%dT%H:%M:%SZ")

docker compose up --force-recreate --no-deps web -d
