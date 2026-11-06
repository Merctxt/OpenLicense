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

RUN dotnet test OpenLicense.Tests/OpenLicense.Tests.csproj -c $BUILD_CONFIGURATION --verbosity normal

RUN dotnet publish OpenLicense.Api/OpenLicense.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenLicense.Api.dll"]