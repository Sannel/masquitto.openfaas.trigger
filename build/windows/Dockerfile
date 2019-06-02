ARG tag=2.2
FROM mcr.microsoft.com/dotnet/core/runtime:${tag} AS base
WORKDIR /app

COPY MQTT.OpenFaas.Trigger/ .
USER ContainerUser
ENTRYPOINT ["dotnet", "MQTT.OpenFaas.Trigger.dll"]