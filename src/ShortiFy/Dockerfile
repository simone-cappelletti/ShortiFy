# =============================================================================
# ShortiFy Dockerfile
# Multi-stage build for .NET 10 Minimal API
# =============================================================================

# -----------------------------------------------------------------------------
# Stage 1: Build
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/ShortiFy/ShortiFy.csproj", "src/ShortiFy/"]
RUN dotnet restore "src/ShortiFy/ShortiFy.csproj"

# Copy source code and build
COPY ["src/ShortiFy/", "src/ShortiFy/"]
WORKDIR /src/src/ShortiFy
RUN dotnet build "ShortiFy.csproj" -c Release -o /app/build

# -----------------------------------------------------------------------------
# Stage 2: Publish
# -----------------------------------------------------------------------------
FROM build AS publish
RUN dotnet publish "ShortiFy.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -----------------------------------------------------------------------------
# Stage 3: Runtime
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

# Create non-root user for security
# Copy published application
COPY --from=publish /app/publish .

# NOTE: some base images don't include `adduser`/`useradd` utilities.
# For local development we'll run as root inside the container. Adjust
# this step for production images by creating a non-root user if needed.

# Expose HTTP port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "ShortiFy.dll"]
