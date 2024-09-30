#!/bin/sh

# El primer parametro es el nombre del contenedor
CONTAINER=$1

# Comprueba si el nombre del contenedor está vacío
if [ -z "$CONTAINER" ]; then
    echo "No se ha especificado el servicio."
    exit 1
fi

# Levanta el contenedor especificado
echo "Iniciando $CONTAINER..."
docker start $CONTAINER