#!/bin/bash
# Script que se ejecuta diariamente
echo "Ejecutando parserdata a las $(date)" >> /var/log/parserdata.log
dotnet /app/ParserData.dll  # Ajusta la ruta según sea necesario