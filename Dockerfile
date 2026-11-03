# Stage 1: build with the full SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file first, so the (slow) restore layer is cached
# and only re-runs when the dependencies change, not on every code edit
COPY ProductCatalogApi.csproj .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore ProductCatalogApi.csproj

COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish ProductCatalogApi.csproj -c Release -o /app/publish --no-restore

# Stage 2: run on the small ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductCatalogApi.dll"]