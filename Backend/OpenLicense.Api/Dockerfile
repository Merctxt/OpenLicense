# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y curl --no-install-recommends && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 5000

# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY OpenLicense.sln .
COPY OpenLicense.Domain/OpenLicense.Domain.csproj OpenLicense.Domain/
COPY OpenLicense.Application/OpenLicense.Application.csproj OpenLicense.Application/
COPY OpenLicense.Infrastructure/OpenLicense.Infrastructure.csproj OpenLicense.Infrastructure/
COPY OpenLicense.Api/OpenLicense.Api.csproj OpenLicense.Api/
RUN dotnet restore "OpenLicense.Api/OpenLicense.Api.csproj"
COPY . .
WORKDIR "/src/OpenLicense.Api"
RUN dotnet build "OpenLicense.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OpenLicense.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenLicense.Api.dll"]
