# Stage 1: Restore dependencies and build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all files and build the app
COPY . ./
RUN dotnet build -c Release -o /src/build


# Stage 3: Publish the app
FROM build AS publish
RUN dotnet publish -c Release -o /src/publish

# Stage 4: Production image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /src
COPY --from=publish /src/publish .

# Expose port 80 for the application
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "FakeHubApi.dll"]
