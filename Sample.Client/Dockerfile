FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Sample.Client/Sample.Client.csproj", "Sample.Client/"]
COPY ["Sample.Contracts/Sample.Contracts.csproj", "Sample.Contracts/"]
COPY ["SimpleCQRS.Contracts/SimpleCQRS.Contracts.csproj", "SimpleCQRS.Contracts/"]
COPY ["SimpleCQRS.Client/SimpleCQRS.Client.csproj", "SimpleCQRS.Client/"]
COPY ["SimpleCQRS.Serializers/SimpleCQRS.Serializers.csproj", "SimpleCQRS.Serializers/"]
COPY ["SimpleCQRS.Loggers/SimpleCQRS.Loggers.csproj", "SimpleCQRS.Loggers/"]
COPY ["SimpleCQRS.Serializers.Json/SimpleCQRS.Serializers.Json.csproj", "SimpleCQRS.Serializers.Json/"]
RUN dotnet restore "Sample.Client/Sample.Client.csproj"
COPY . .
WORKDIR "/src/Sample.Client"
RUN dotnet build "Sample.Client.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Sample.Client.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

# Add the wait-for-it script and use it for the process entrypoint
COPY wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh

ENTRYPOINT [ "/wait-for-it.sh", "sample.server:80", "--", "dotnet", "Sample.Client.dll" ]
