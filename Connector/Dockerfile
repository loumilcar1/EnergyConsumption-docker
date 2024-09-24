# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

# Fase de compilación
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia el archivo del proyecto y restaura las dependencias
COPY Connector/Connector.csproj Connector/
RUN dotnet restore "Connector/Connector.csproj"

# Copia el resto de los archivos del proyecto
COPY Connector/ Connector/

# Compila el proyecto
WORKDIR "/src/Connector"
RUN dotnet build "Connector.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Fase de publicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Connector.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase final (runtime)
FROM base AS final
WORKDIR /app

# Copia los archivos publicados desde la fase de build
COPY --from=publish /app/publish .

# Copia el archivo de configuración a la ubicación correcta en la fase final
COPY Connector/appsettings.json ./  # Copia el appsettings.json a /app

# Comando de inicio
ENTRYPOINT ["dotnet", "Connector.dll"]