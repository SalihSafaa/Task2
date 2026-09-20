# --- Stage 1: build ---
# The SDK image has the compiler; we only need it during this stage.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy just the .csproj first and restore before copying the rest of the
# source. This is a caching trick: Docker only re-runs a step if the files
# it depends on changed. If you copied everything at once, changing a
# single .cs file would force NuGet to re-restore every package, every
# time. Copying the .csproj alone means restore only reruns when you
# actually change a dependency.
COPY ProductCatalogApi.csproj .
RUN dotnet restore ProductCatalogApi.csproj

# Now copy the real source and publish. --no-restore skips redoing the
# restore we just cached above.
COPY . .
RUN dotnet publish ProductCatalogApi.csproj -c Release -o /app/publish --no-restore

# --- Stage 2: run ---
# Runtime-only image: no SDK, no compiler, just what's needed to execute
# an already-built app. This is what actually ships.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductCatalogApi.dll"]