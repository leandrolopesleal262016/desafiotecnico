FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BookScraperWorker.csproj", "./"]
RUN dotnet restore "BookScraperWorker.csproj"
COPY . .
RUN dotnet build "BookScraperWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookScraperWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookScraperWorker.dll"]