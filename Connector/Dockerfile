# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

# Instalar tzdata para gestionar la zona horaria
RUN apt-get update && apt-get install -y tzdata

# Establecer la zona horaria
ENV TZ=Europe/Madrid

USER app
WORKDIR /app

# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Connector/Connector.csproj", "Connector/"]
RUN dotnet restore "./Connector/Connector.csproj"
COPY . .
WORKDIR "/src/Connector"
RUN dotnet build "./Connector.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Connector.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Copiar el archivo de configuración appsettings.json
COPY ["Connector/appsettings.json", "Connector/"]

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Connector.dll"]
