# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY WebApiShop.sln .
COPY OrderConsumer/OrderConsumer.csproj ./OrderConsumer/

# Restore dependencies
RUN dotnet restore OrderConsumer/OrderConsumer.csproj

# Copy all source code
COPY . .

# Publish
RUN dotnet publish OrderConsumer/OrderConsumer.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

ENV DOTNET_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "OrderConsumer.dll"]
