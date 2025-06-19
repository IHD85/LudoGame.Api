# ----------- Build Stage -----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopier alle filer og build .NET-API
COPY . ./
RUN dotnet publish -c Release -o out


# ----------- Runtime Stage -----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

# Kopier kun det nødvendige output fra build-stage
COPY --from=build /app/out ./

# Opret ikke-root bruger og giv rettigheder
RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser /app

# Skift til ikke-root bruger
USER appuser

# Start applikationen
ENTRYPOINT ["dotnet", "LudoGame.Api.dll"]
