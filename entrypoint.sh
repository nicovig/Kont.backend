#!/bin/sh
#

set -e

if [ -z "$1" ]; then
    exec dotnet Kont.backend.dll
fi

exec "$@"
