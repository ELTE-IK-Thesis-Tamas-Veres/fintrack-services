# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Stage 2: Build image with .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copy only the .csproj first and restore
COPY ["src/services/FinTrack-api/fintrack-api/fintrack-api.csproj", "FinTrack-api/fintrack-api/"]
RUN dotnet restore "FinTrack-api/fintrack-api/fintrack-api.csproj"

# 2. Copy the remaining source files and build
COPY src . 
WORKDIR "/src/services/FinTrack-api/fintrack-api"
RUN dotnet build "fintrack-api.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
WORKDIR "/src/services/FinTrack-api/fintrack-api"
RUN dotnet publish "fintrack-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final image
FROM base AS final
WORKDIR /app

# Copy the published build from the previous stage
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# Run the application
ENTRYPOINT ["dotnet", "fintrack-api.dll"]
