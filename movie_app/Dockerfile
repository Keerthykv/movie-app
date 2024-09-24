# Base image for Azure Functions .NET 6 runtime
FROM mcr.microsoft.com/azure-functions/dotnet:4 AS base
WORKDIR /home/site/wwwroot

# Build image using .NET SDK to build the function app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY ["movie_app.csproj", "./"]
RUN dotnet restore "./movie_app.csproj"

# Copy the rest of the source code and build the project
COPY . .
RUN dotnet publish "./movie_app.csproj" -c Release -o /app/publish

# Final stage, using the Azure Functions runtime image as base
FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=build /app/publish .

# Azure Functions uses 'func host start' to start the function app, no need for an entrypoint
