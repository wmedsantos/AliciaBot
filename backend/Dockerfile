# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AlicIA.sln ./
COPY src/AlicIA.Api/AlicIA.Api.csproj src/AlicIA.Api/
COPY src/AlicIA.Domain/AlicIA.Domain.csproj src/AlicIA.Domain/
COPY src/AlicIA.Infrastructure/AlicIA.Infrastructure.csproj src/AlicIA.Infrastructure/

RUN dotnet restore AlicIA.sln

COPY . .
RUN dotnet publish src/AlicIA.Api/AlicIA.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet AlicIA.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
