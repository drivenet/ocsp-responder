FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY shared shared

WORKDIR /src/OcspResponder
COPY OcspResponder/OcspResponder.csproj .
RUN dotnet restore

WORKDIR /src/OcspResponder
COPY OcspResponder .
RUN dotnet publish -c Release --no-restore -o /app && \
    find /app -type f -executable -not -wholename /app/ocsp-responder -exec chmod a-x -- {} +

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENV OCSPR_HOST_URLS=http://+:80
ENTRYPOINT ./ocsp-responder
