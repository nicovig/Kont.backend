# Before publish the application
# dotnet publish -c Release -o dist -r linux-musl-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

RUN apk add --no-cache tzdata icu-libs

COPY --chmod=755 ./entrypoint.sh /entrypoint.sh

COPY ./dist /app

WORKDIR /app
EXPOSE 8080

ARG app_version
ENV APP_VERSION_BACK=$app_version
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT [ "/entrypoint.sh" ]

HEALTHCHECK --interval=5m --timeout=3s \
    CMD wget http://localhost:8080/healthz -q -O /dev/null || exit