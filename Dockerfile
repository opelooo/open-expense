# 1. Build Stage (Native AMD64)
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG TARGETARCH
WORKDIR /src

COPY ["OpenExpenseApp.csproj", "./"]
RUN dotnet restore "OpenExpenseApp.csproj" -a $TARGETARCH

COPY . .
RUN dotnet publish "OpenExpenseApp.csproj" -c Release -o /app/publish -a $TARGETARCH --self-contained false

# 2. Final Stage (Target ARM64)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .

# Kita gunakan user root sementara agar bisa install library saat startup
USER root

# Script untuk install ICU hanya jika belum ada, lalu jalankan App
RUN echo '#!/bin/sh' > /entrypoint.sh && \
    echo 'if ! apk info -e icu-libs; then apk add --no-cache icu-libs icu-data-full; fi' >> /entrypoint.sh && \
    echo 'exec dotnet OpenExpenseApp.dll "$@"' >> /entrypoint.sh && \
    chmod +x /entrypoint.sh

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["/entrypoint.sh"]
