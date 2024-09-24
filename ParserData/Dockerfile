# Dockerfile para ParserData

# Usa la imagen base de .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build

# Establece el directorio de trabajo
WORKDIR /src

# Copia el archivo de proyecto y restaura las dependencias
COPY ParserData/ParserData.csproj ParserData/
RUN dotnet restore "ParserData/ParserData.csproj"

# Copia el resto del código fuente
COPY ParserData/ ./ParserData/

# Compila y publica la aplicación
WORKDIR /src/ParserData
RUN dotnet build "ParserData.csproj" -c Release -o /app/build
RUN dotnet publish "ParserData.csproj" -c Release -o /app/publish

# Fase final: usa la imagen base del runtime de .NET
FROM mcr.microsoft.com/dotnet/runtime:3.1 AS final
WORKDIR /app

# Asegúrate de que el directorio de destino existe
RUN mkdir -p /app

# Copia el archivo de configuración (appsettings.json)
COPY ParserData/appsettings.json /app/appsettings.json

# Copia los archivos publicados desde la etapa de construcción
COPY --from=build /app/publish .

# Comando de entrada
ENTRYPOINT ["dotnet", "ParserData.dll"]