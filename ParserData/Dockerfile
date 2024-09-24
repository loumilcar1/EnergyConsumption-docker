# Dockerfile para ParserData

# Usa la imagen base de .NET
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build

# Establece el directorio de trabajo
WORKDIR /src

# Copia el archivo de proyecto y restaura las dependencias
COPY ParserData/ParserData.csproj ParserData/
RUN dotnet restore "ParserData/ParserData.csproj"

# Copia el resto del código
COPY . .

# Compila y publica la aplicación
WORKDIR /src/ParserData
RUN dotnet build "ParserData.csproj" -c Release -o /app/build

RUN dotnet publish "ParserData.csproj" -c Release -o /app/publish

# Fase final
FROM mcr.microsoft.com/dotnet/runtime:3.1 AS final
WORKDIR /app

# Copia el archivo de configuración
COPY ParserData/appsettings.json .  # Asegúrate de que el archivo se copie a la ruta correcta

COPY --from=build /app/publish .  # Copia los archivos publicados

ENTRYPOINT ["dotnet", "ParserData.dll"]