#!/bin/bash

REPO=$(cat modinfo.json | jq -r '.["website"]')
echo "Extracted repo url: $REPO"
VAR1=$(cat modinfo.json | jq -r '.["version"]')
echo "Version in this commit: $VAR1" 
VAR2=$(curl ${REPO}/raw/main/modinfo.json | jq -r '.["version"]')
echo "Version on main in repo: $VAR2" 

if [[ "$VAR1" == "$VAR2" ]]; then
    echo "Version string was not changed: $VAR1, $VAR2"
    exit 1
else
    echo "New Version: $VAR2"
fi
