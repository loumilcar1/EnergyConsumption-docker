FROM alpine:latest AS base

# Instala paquetes necesarios
RUN apk add --no-cache bash curl openrc docker-cli

# Copia el script a la ruta del contenedor
COPY ["Scheduler/scheduler_init.sh", "/app/scheduler_init.sh"]

# Da permisos de ejecución al script
RUN chmod +x /app/scheduler_init.sh

# Copia el archivo de crontab al contenedor
COPY ["Scheduler/crontab", "/etc/crontabs/root"]

# Comando para iniciar cron en primer plano
CMD ["crond", "-f"]