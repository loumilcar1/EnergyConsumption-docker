#!/bin/sh

CONTAINER=$1  # El primer argumento es el nombre del servicio

if [ -z "$CONTAINER" ]; then
    echo "No se ha especificado el servicio."
    exit 1
fi

# Levanta el contenedor especificado
echo "Iniciando $CONTAINER..."
docker start $CONTAINER