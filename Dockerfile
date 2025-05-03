FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_URLS=http://*:443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY */*.csproj ./

RUN dotnet restore "BotController.csproj"
COPY . .
WORKDIR "/src/BotController"
RUN dotnet build "BotController.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BotController.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BotController.dll"]
