# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and all project files first (for layer caching)
COPY WebApiShop.sln .
COPY WebApiShop/WebApiShop.csproj ./WebApiShop/
COPY Services/Services.csproj ./Services/
COPY Repositories/Repositories.csproj ./Repositories/
COPY Entities/Entities.csproj ./Entities/
COPY DTOs/DTOs.csproj ./DTOs/

# Restore dependencies
RUN dotnet restore WebApiShop/WebApiShop.csproj

# Copy all source code
COPY . .

# Publish the API project
RUN dotnet publish WebApiShop/WebApiShop.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApiShop.dll"]
