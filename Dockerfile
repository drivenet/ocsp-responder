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
RUN dotnet publish -c Release -r linux-x64 --self-contained false --no-restore -p:MinimalBuild=true -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENV OCSPR_HOST_URLS=http://+:80
ENV OCSPR_APP_DATABASEPATH=/db
ENTRYPOINT ./ocsp-responder
