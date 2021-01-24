#!/bin/bash

source ci/vercomp.script

REPO=$(cat modinfo.json | jq -r '.["website"]')
VAR1=$(cat modinfo.json | jq -r '.["version"]')
VAR2=$(curl -LsS "${REPO}/raw/main/modinfo.json" | jq -r '.["version"]')

vercomp $VAR1 $VAR2
vercomp_status=$?
if [[ vercomp_status -eq 2 ]]; then
  echo "Version string was not changed: $VAR2 -> $VAR1"
  exit 1
else
  echo "New Version will be: $VAR1 (prev: $VAR2)"
fi
