# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["CricLive.sln", "."]
COPY ["CricLive/CricLive.csproj", "CricLive/"]
RUN dotnet restore "CricLive/CricLive.csproj"

COPY . .
WORKDIR /src/CricLive
RUN dotnet publish "CricLive.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user (Debian syntax)
ARG APP_USER=app
RUN useradd -m -u 5000 $APP_USER
USER $APP_USER

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "CricLive.dll"]
