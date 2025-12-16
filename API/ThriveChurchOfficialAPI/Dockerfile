# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ThriveChurchOfficialAPI.Core/ThriveChurchOfficialAPI.Core.csproj ThriveChurchOfficialAPI.Core/
COPY ThriveChurchOfficialAPI.Repositories/ThriveChurchOfficialAPI.Repositories.csproj ThriveChurchOfficialAPI.Repositories/
COPY ThriveChurchOfficialAPI.Services/ThriveChurchOfficialAPI.Services.csproj ThriveChurchOfficialAPI.Services/
COPY ThriveChurchOfficialAPI/ThriveChurchOfficialAPI.csproj ThriveChurchOfficialAPI/

# Restore dependencies
RUN dotnet restore ThriveChurchOfficialAPI/ThriveChurchOfficialAPI.csproj

# Copy source code
COPY . .

# Build and publish
WORKDIR /src/ThriveChurchOfficialAPI
RUN dotnet publish -c Release -o /app/publish

# Debug stage (for Visual Studio debugging) - must come before runtime so it's not the default
FROM build AS debug
WORKDIR /src/ThriveChurchOfficialAPI
# Install debugger
RUN apt-get update && apt-get install -y unzip && \
    curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg
# Don't publish, keep source for debugging
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "run", "--no-launch-profile"]

# Runtime stage (LAST = default for production builds)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080

# Expose port
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "ThriveChurchOfficialAPI.dll"]

