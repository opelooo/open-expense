# Stage 1: Build
FROM ://mcr.microsoft.com AS build
WORKDIR /src

# Copy csproj dan restore
COPY ["AccountingApp.csproj", "./"]
RUN dotnet restore "AccountingApp.csproj"

# Copy semua file dan publish
COPY . .
RUN dotnet publish "AccountingApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM ://mcr.microsoft.com AS final
WORKDIR /app
COPY --from=build /app/publish .

# Install ICU untuk dukungan globalisasi di Alpine
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "AccountingApp.dll"]
