# 1. Base image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything (including solution and projects)
COPY . .

# Restore dependencies
RUN dotnet restore "WebApi/WebApi.csproj"

# Build the app
RUN dotnet build "WebApi/WebApi.csproj" -c Release -o /app/build

# 3. Publish the app
FROM build AS publish
RUN dotnet publish "WebApi/WebApi.csproj" -c Release -o /app/publish

# 4. Runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]
