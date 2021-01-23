#!/bin/bash

VAR1=$(cat modinfo.json | jq -r '.["version"]')
VAR2="1.0.0"

if [[ "$VAR1" == "$VAR2" ]]; then
    echo "Version string was not changed: $VAR1, $VAR2"
    exit 1
else
    echo "New Version: $VAR2"
fi
