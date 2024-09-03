# Use the official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:6.0.420-alpine3.19-amd64 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build the app
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.28-alpine3.19-amd64
WORKDIR /app
COPY --from=build-env /app/out .

# Make sure the app binds to port 8080
ENV ASPNETCORE_URLS http://*:8080

# Expose the port and start the app
ENTRYPOINT ["dotnet", "ChatHub.dll"]