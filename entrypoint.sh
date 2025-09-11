#!/bin/sh
#

set -e

if [ -z "$1" ]; then
    exec dotnet exec Galarne.Template.AspNet.dll
fi

exec "$@"
