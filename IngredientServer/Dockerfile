# ============================================
# Dockerfile - IngredientServer/Dockerfile
# ============================================

# Stage 1: Base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["IngredientServer/IngredientServer.csproj", "IngredientServer/"]
RUN dotnet restore "IngredientServer/IngredientServer.csproj"

# Copy all source code
COPY . .
WORKDIR "/src/IngredientServer"

# Build the application
RUN dotnet build "IngredientServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 2: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "IngredientServer.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Copy wait-for-it script
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

# Install bash and curl for healthcheck
RUN apt-get update && \
    apt-get install -y bash curl && \
    rm -rf /var/lib/apt/lists/*

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads

# Create non-root user
RUN adduser --disabled-password --gecos '' dotnetuser && \
    chown -R dotnetuser:dotnetuser /app

USER dotnetuser

# Wait for MySQL then start application
ENTRYPOINT ["/bin/bash", "-c", "./wait-for-it.sh mysql:3306 -t 60 -- dotnet IngredientServer.dll"]

