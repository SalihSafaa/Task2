# The application is published on the host before Compose builds this image.
# Keeping NuGet restore outside BuildKit avoids network failures during image builds.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY publish/ .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductCatalogApi.dll"]