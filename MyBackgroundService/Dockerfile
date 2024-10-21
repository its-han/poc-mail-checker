FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base 
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyBackgroundService.csproj", "."]
RUN dotnet restore "MyBackgroundService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "MyBackgroundService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MyBackgroundService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyBackgroundService.dll"]
