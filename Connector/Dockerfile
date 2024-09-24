# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia el archivo del proyecto y restaura las dependencias
COPY ["Connector/Connector.csproj", "Connector/"]
RUN dotnet restore "Connector/Connector.csproj"

# Copia el resto de los archivos del proyecto
COPY ["Connector/", "Connector/"]

# Copia el archivo de configuración al contenedor
COPY ["Connector/appsettings.json", "Connector/"]

# Compila el proyecto
WORKDIR "/src/Connector"
RUN dotnet build "Connector.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Connector.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Copia el archivo de configuración a la ubicación correcta en la fase final
COPY ["Connector/appsettings.json", "./"]  # Copia el appsettings.json a /app

ENTRYPOINT ["dotnet", "Connector.dll"]