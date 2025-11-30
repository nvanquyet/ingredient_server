# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file and restore dependencies
COPY ["IngredientServer/IngredientServer.csproj", "./"]
RUN dotnet restore "IngredientServer.csproj"

# Copy everything else and build
COPY IngredientServer/ .
WORKDIR "/src"
RUN dotnet build "IngredientServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 2: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "IngredientServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

# Install bash and curl for healthcheck (if wait-for-it.sh is needed)
RUN apt-get update && apt-get install -y --no-install-recommends \
    bash \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN adduser --disabled-password --gecos '' --uid 1000 dotnetuser \
    && chown -R dotnetuser:dotnetuser /app

# Copy published files
COPY --from=publish /app/publish .

# Set ownership and switch to non-root user
USER dotnetuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "IngredientServer.dll"]