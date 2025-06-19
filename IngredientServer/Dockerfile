# Stage 1: Base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IngredientServer/IngredientServer.csproj", "IngredientServer/"]
RUN dotnet restore "IngredientServer/IngredientServer.csproj"
COPY . .
WORKDIR "/src/IngredientServer"
RUN dotnet build "IngredientServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "IngredientServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh
# Cài đặt bash nếu cần
RUN apt-get update && apt-get install -y bash && rm -rf /var/lib/apt/lists/*
# Giữ user non-root
RUN adduser --disabled-password --gecos '' dotnetuser
RUN chown -R dotnetuser:dotnetuser /app
USER dotnetuser
ENTRYPOINT ["/bin/bash", "-c", "./wait-for-it.sh sqlserver:1433 -t 60 -- dotnet IngredientServer.dll"]