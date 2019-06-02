#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see http://aka.ms/containercompat 

FROM mcr.microsoft.com/dotnet/core/runtime:2.2 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["MQTT.OpenFaas.Trigger/MQTT.OpenFaas.Trigger.csproj", "MQTT.OpenFaas.Trigger/"]
RUN dotnet restore "MQTT.OpenFaas.Trigger/MQTT.OpenFaas.Trigger.csproj"
COPY . .
WORKDIR "/src/MQTT.OpenFaas.Trigger"
RUN dotnet build "MQTT.OpenFaas.Trigger.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MQTT.OpenFaas.Trigger.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MQTT.OpenFaas.Trigger.dll"]
