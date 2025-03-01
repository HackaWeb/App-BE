FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY src/App.sln .
COPY src/App.Api/App.Api.csproj App.Api/
COPY src/App.Application/App.Application.csproj App.Application/
COPY src/App.DataContext/App.DataContext.csproj App.DataContext/
COPY src/App.Domain/App.Domain.csproj App.Domain/
COPY src/App.Infrastructure/App.Infrastructure.csproj App.Infrastructure/
COPY src/contracts/App.RestContracts/App.RestContracts.csproj contracts/App.RestContracts/

RUN dotnet restore "App.sln"

COPY src/ ./

WORKDIR "/src/App.Api"
RUN dotnet build "./App.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./App.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App.Api.dll"]