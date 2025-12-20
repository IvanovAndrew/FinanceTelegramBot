# --- Runtime image ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS base
WORKDIR /app

# Install locales
RUN apt-get update && apt-get install -y --no-install-recommends locales \
    && sed -i 's/# en_US.UTF-8 UTF-8/en_US.UTF-8 UTF-8/' /etc/locale.gen \
    && sed -i 's/# ru_RU.UTF-8 UTF-8/ru_RU.UTF-8 UTF-8/' /etc/locale.gen \
    && locale-gen

# Set environment variables for proper locale handling in .NET
ENV LANG=en_US.UTF-8
ENV LC_ALL=en_US.UTF-8
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

# Azure Container Apps provides HTTP internally; TLS is already terminated externally
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# --- Build image ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only .csproj to cache restore
COPY */*.csproj ./
RUN dotnet restore "BotController.csproj"

# Copy the rest of the source code
COPY . .

WORKDIR "/src/BotController"
RUN dotnet build "BotController.csproj" -c Release -o /app/build

# --- Publish ---
FROM build AS publish
RUN dotnet publish "BotController.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Final lite image ---
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "BotController.dll"]
