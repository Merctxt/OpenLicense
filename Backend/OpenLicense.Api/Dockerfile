FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y curl --no-install-recommends && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY OpenLicense.sln .
COPY OpenLicense.Domain/OpenLicense.Domain.csproj OpenLicense.Domain/
COPY OpenLicense.Application/OpenLicense.Application.csproj OpenLicense.Application/
COPY OpenLicense.Infrastructure/OpenLicense.Infrastructure.csproj OpenLicense.Infrastructure/
COPY OpenLicense.Api/OpenLicense.Api.csproj OpenLicense.Api/
COPY OpenLicense.Tests/OpenLicense.Tests.csproj OpenLicense.Tests/
RUN dotnet restore OpenLicense.sln

COPY . .

RUN KEY="${JWT_SECRET_KEY:-$(head -c 32 /dev/urandom | base64)}" && \
    mkdir -p OpenLicense.Tests/bin/$BUILD_CONFIGURATION/net9.0 && \
    printf '{"Jwt":{"SecretKey":"%s","Issuer":"%s","Audience":"%s"},"RegistrationEnabled":%s}\n' \
      "$KEY" \
      "${JWT_ISSUER:-OpenLicenseApi}" \
      "${JWT_AUDIENCE:-OpenLicenseApiUsers}" \
      "${REGISTRATION_ENABLED:-true}" > OpenLicense.Tests/bin/$BUILD_CONFIGURATION/net9.0/appsettings.json && \
    dotnet test OpenLicense.Tests/OpenLicense.Tests.csproj -c $BUILD_CONFIGURATION --verbosity normal

RUN dotnet publish OpenLicense.Api/OpenLicense.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenLicense.Api.dll"]