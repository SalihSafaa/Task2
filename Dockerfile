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

# Stage 3: build the EF Core migration bundle (one self-contained executable)
FROM build AS bundle
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet tool install --global dotnet-ef --version 10.*
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet ef migrations bundle --project ProductCatalogApi.csproj -o /app/efbundle --force

# Stage 4: tiny image that only runs the bundle
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrator
WORKDIR /app
COPY --from=bundle /app/efbundle .
ENTRYPOINT ["./efbundle"]