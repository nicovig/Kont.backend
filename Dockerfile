# Multi-stage build: build & publish, then run

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore first for better layer caching
COPY ./src/Kont.backend/Kont.backend.csproj ./src/Kont.backend/
RUN dotnet restore ./src/Kont.backend/Kont.backend.csproj

# copy the rest of the source and publish
COPY . .
RUN dotnet publish ./src/Kont.backend/Kont.backend.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
RUN apk add --no-cache tzdata icu-libs wget

COPY --chmod=755 ./entrypoint.sh /entrypoint.sh
COPY --from=build /app/publish /app

WORKDIR /app
EXPOSE 5000

ARG app_version
ENV APP_VERSION_BACK=$app_version
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT [ "/entrypoint.sh" ]

HEALTHCHECK --interval=5m --timeout=3s \
    CMD wget -q -O /dev/null http://localhost:5000/ || exit 1