# Build context is the repo's backend/ directory (see docker-compose.yml's `context: ./backend`).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy just the solution + csproj files first so `dotnet restore` is cached across builds unless
# a dependency actually changes — copying the whole source tree before restore would invalidate
# this layer on every code edit.
COPY OnlineShop.sln Directory.Build.props ./
COPY src/BuildingBlocks/*.csproj src/BuildingBlocks/
COPY src/Api/*.csproj src/Api/
COPY src/TelegramBot/*.csproj src/TelegramBot/
COPY src/Modules/Identity/Domain/*.csproj src/Modules/Identity/Domain/
COPY src/Modules/Identity/Application/*.csproj src/Modules/Identity/Application/
COPY src/Modules/Identity/Infrastructure/*.csproj src/Modules/Identity/Infrastructure/
COPY src/Modules/Catalog/Domain/*.csproj src/Modules/Catalog/Domain/
COPY src/Modules/Catalog/Application/*.csproj src/Modules/Catalog/Application/
COPY src/Modules/Catalog/Infrastructure/*.csproj src/Modules/Catalog/Infrastructure/
COPY src/Modules/Inventory/Domain/*.csproj src/Modules/Inventory/Domain/
COPY src/Modules/Inventory/Application/*.csproj src/Modules/Inventory/Application/
COPY src/Modules/Inventory/Infrastructure/*.csproj src/Modules/Inventory/Infrastructure/
COPY src/Modules/Ordering/Domain/*.csproj src/Modules/Ordering/Domain/
COPY src/Modules/Ordering/Application/*.csproj src/Modules/Ordering/Application/
COPY src/Modules/Ordering/Infrastructure/*.csproj src/Modules/Ordering/Infrastructure/
COPY src/Modules/AiAdvisory/Domain/*.csproj src/Modules/AiAdvisory/Domain/
COPY src/Modules/AiAdvisory/Application/*.csproj src/Modules/AiAdvisory/Application/
COPY src/Modules/AiAdvisory/Infrastructure/*.csproj src/Modules/AiAdvisory/Infrastructure/
COPY src/Modules/Notification/Domain/*.csproj src/Modules/Notification/Domain/
COPY src/Modules/Notification/Application/*.csproj src/Modules/Notification/Application/
COPY src/Modules/Notification/Infrastructure/*.csproj src/Modules/Notification/Infrastructure/
COPY tests/OnlineShop.UnitTests/*.csproj tests/OnlineShop.UnitTests/
COPY tests/OnlineShop.IntegrationTests/*.csproj tests/OnlineShop.IntegrationTests/

RUN dotnet restore src/Api/OnlineShop.Api.csproj

COPY . .
RUN dotnet publish src/Api/OnlineShop.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Npgsql probes for GSSAPI (Kerberos) support on every connection attempt even though this app
# never uses Kerberos auth — without this lib present it still works, but logs a scary-looking
# "Cannot load library libgssapi_krb5.so.2" error on startup. Silences that noise.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libkrb5-3 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OnlineShop.Api.dll"]
