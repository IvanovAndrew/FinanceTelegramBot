# --- Runtime image ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
WORKDIR /app

# Locales + ScottPlot runtime dependencies (single layer)
RUN apt-get update && apt-get install -y --no-install-recommends \
    locales \
    libgdiplus \
    libx11-6 \
    fontconfig \
    libfreetype6 \
    fonts-dejavu-core \
    && sed -i 's/# en_US.UTF-8 UTF-8/en_US.UTF-8 UTF-8/' /etc/locale.gen \
    && sed -i 's/# ru_RU.UTF-8 UTF-8/ru_RU.UTF-8 UTF-8/' /etc/locale.gen \
    && locale-gen \
    && ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll \
    && fc-cache -f -v \
    && rm -rf /var/lib/apt/lists/*

# Set environment variables for proper locale handling in .NET
ENV LANG=en_US.UTF-8
ENV LC_ALL=en_US.UTF-8
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
ENV ASPNETCORE_ENVIRONMENT=Production

# Azure Container Apps provides HTTP internally; TLS is already terminated externally
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# --- Build image ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only .csproj to cache restore
COPY */*.csproj ./
RUN dotnet restore "BotController.csproj" -r linux-x64

# Copy the rest of the source code
COPY . .

# --- Publish ---
FROM build AS publish
WORKDIR "/src/BotController"
RUN dotnet publish "BotController.csproj" -c Release -o /app/publish \
    -r linux-x64 --self-contained false \
    -p:PublishReadyToRun=true -p:UseAppHost=false

# --- Final lite image ---
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "BotController.dll"]