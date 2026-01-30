# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR "/src"
COPY ["./nuget.config", "."] 
COPY ["./src/GenericReportGenerator.Api/GenericReportGenerator.Api.csproj", "GenericReportGenerator.Api/"]
COPY ["./src/GenericReportGenerator.Core/GenericReportGenerator.Core.csproj", "GenericReportGenerator.Core/"]
COPY ["./src/GenericReportGenerator.Infrastructure/GenericReportGenerator.Infrastructure.csproj", "GenericReportGenerator.Infrastructure/"]
RUN dotnet restore "./GenericReportGenerator.Api/GenericReportGenerator.Api.csproj"
COPY "./src/" "."

WORKDIR "/src/GenericReportGenerator.Api"
RUN dotnet build "./GenericReportGenerator.Api.csproj" -c $BUILD_CONFIGURATION -o "/app/build"

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GenericReportGenerator.Api.csproj" -c $BUILD_CONFIGURATION -o "/app/publish" /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish "/app/publish" "."
ENTRYPOINT ["dotnet", "GenericReportGenerator.Api.dll"]