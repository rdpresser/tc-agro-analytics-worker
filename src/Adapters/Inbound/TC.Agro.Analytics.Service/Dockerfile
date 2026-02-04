# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Kerberos/GSSAPI dependency for providers (e.g., SQL client integrated auth)
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy Directory.Build.props, Directory.Build.targets and Directory.Packages.props first (build optimization)
# From common
COPY common/Directory.Build.props ./common/
COPY common/Directory.Build.targets ./common/
COPY common/Directory.Packages.props ./common/

# From analytics-worker
COPY services/analytics-worker/Directory.Build.props ./services/analytics-worker/
COPY services/analytics-worker/Directory.Build.targets ./services/analytics-worker/
COPY services/analytics-worker/Directory.Packages.props ./services/analytics-worker/

# Copy shared common projects (required for ProjectReferences)
COPY ["common/src/TC.Agro.SharedKernel/TC.Agro.SharedKernel.csproj", "common/src/TC.Agro.SharedKernel/"]
COPY ["common/src/TC.Agro.Contracts/TC.Agro.Contracts.csproj", "common/src/TC.Agro.Contracts/"]

# Copy analytics-worker projects maintaining directory structure (required for ProjectReferences)
COPY ["services/analytics-worker/src/Adapters/Inbound/TC.Agro.Analytics.Service/TC.Agro.Analytics.Service.csproj", "services/analytics-worker/src/Adapters/Inbound/TC.Agro.Analytics.Service/"]
COPY ["services/analytics-worker/src/Core/TC.Agro.Analytics.Application/TC.Agro.Analytics.Application.csproj", "services/analytics-worker/src/Core/TC.Agro.Analytics.Application/"]
COPY ["services/analytics-worker/src/Core/TC.Agro.Analytics.Domain/TC.Agro.Analytics.Domain.csproj", "services/analytics-worker/src/Core/TC.Agro.Analytics.Domain/"]
COPY ["services/analytics-worker/src/Adapters/Outbound/TC.Agro.Analytics.Infrastructure/TC.Agro.Analytics.Infrastructure.csproj", "services/analytics-worker/src/Adapters/Outbound/TC.Agro.Analytics.Infrastructure/"]

# Restore packages
RUN dotnet restore "services/analytics-worker/src/Adapters/Inbound/TC.Agro.Analytics.Service/TC.Agro.Analytics.Service.csproj"

# Copy source code maintaining structure
COPY . .

# Build the application
WORKDIR "/src/services/analytics-worker/src/Adapters/Inbound/TC.Agro.Analytics.Service"
RUN dotnet build "./TC.Agro.Analytics.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TC.Agro.Analytics.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TC.Agro.Analytics.Service.dll"]
