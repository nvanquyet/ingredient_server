# Stage 1: Base image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

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
USER $APP_UID
ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "-t", "60", "--", "dotnet", "IngredientServer.dll"]