# Stage 1: Restore dependencies and build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all files and build the app
COPY . ./
RUN dotnet build -c Release -o /src/build

# Stage 2: Publish the app
FROM build AS publish
RUN dotnet publish -c Release -o /src/publish

# Stage 3: Production image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published app
COPY --from=publish /src/publish .

# Create Logs folder with proper permissions
RUN mkdir -p /app/Logs \
    && chown -R www-data:www-data /app/Logs \
    && chmod -R 755 /app/Logs

# Set environment variable for logging folder (optional, for consistency)
ENV DOTNET_LOG_DIR=/app/Logs

# Expose port 8080
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "FakeHubApi.dll"]
