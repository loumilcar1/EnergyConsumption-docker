# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DatabaseInitializer/DatabaseInitializer.csproj", "DatabaseInitializer/"]
RUN dotnet restore "./DatabaseInitializer/DatabaseInitializer.csproj"
COPY . .
WORKDIR "/src/DatabaseInitializer"
RUN dotnet build "./DatabaseInitializer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DatabaseInitializer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Copiar el archivo de configuración appsettings.json
COPY ["DatabaseInitializer/appsettings.json", "DatabaseInitializer/"]

# Copiar los scripts SQL necesarios al contenedor
COPY ["DatabaseInitializer/scripts/", "scripts/"] # Asegúrate de que la ruta sea correcta


# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DatabaseInitializer.dll"]