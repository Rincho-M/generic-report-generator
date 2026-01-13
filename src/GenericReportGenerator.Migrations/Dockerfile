# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR "/src"
COPY ["nuget.config", "."]
COPY ["./src/GenericReportGenerator.Migrations/GenericReportGenerator.Migrations.csproj", "GenericReportGenerator.Migrations/"]
RUN dotnet restore "./GenericReportGenerator.Migrations/GenericReportGenerator.Migrations.csproj"
COPY "./src/" "."

WORKDIR "/src/GenericReportGenerator.Migrations"
RUN dotnet build "./GenericReportGenerator.Migrations.csproj" -c $BUILD_CONFIGURATION -o "/app/build"

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GenericReportGenerator.Migrations.csproj" -c $BUILD_CONFIGURATION -o "/app/publish" /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish "/app/publish" "."
ENTRYPOINT ["dotnet", "GenericReportGenerator.Migrations.dll"]