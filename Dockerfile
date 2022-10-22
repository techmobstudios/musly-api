# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

# copy everything else and build app
COPY . ./
RUN dotnet publish musly-api -c release -o app

# final stage/image
# FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
FROM rlshaw/musly-dotnet6
ENV LD_LIBRARY_PATH=/usr/local/lib
WORKDIR /app
COPY --from=build /source/app .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "musly-api.dll"]

