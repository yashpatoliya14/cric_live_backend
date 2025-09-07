# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["CricLive.sln", "."]
COPY ["CricLive/CricLive.csproj", "CricLive/"]
RUN dotnet restore "CricLive/CricLive.csproj"

# Copy everything else and build the project
COPY . .
WORKDIR /src/CricLive
RUN dotnet publish "CricLive.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Create a non-root user for security
ARG APP_USER=app
RUN adduser -u 5000 -D -s /bin/sh $APP_USER
USER $APP_USER

WORKDIR /app
COPY --from=build /app/publish .

# Set the listening port and expose it
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "CricLive.dll"]
