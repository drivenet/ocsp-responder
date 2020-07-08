FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY shared shared

WORKDIR /src/OcspResponder
COPY OcspResponder/OcspResponder.csproj .
RUN dotnet restore -r linux-x64 -p:MinimalBuild=true

WORKDIR /src/OcspResponder
COPY OcspResponder .
RUN dotnet publish -c Release -r linux-x64 --self-contained false --no-restore -p:MinimalBuild=true -o /app && \
    rm /app/web.config /app/*.deps.json

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ./ocsp-responder --urls http://+:80 --databasePath /db
