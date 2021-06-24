FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY shared shared
COPY .editorconfig global.json *.props ./

WORKDIR /src/OcspResponder
COPY OcspResponder/OcspResponder.csproj .
RUN dotnet restore -r linux-x64 -p:MinimalBuild=true

COPY OcspResponder .
RUN dotnet publish -c Release -r linux-x64 --self-contained false --no-restore -p:MinimalBuild=true -o /app && \
    rm /app/*.deps.json

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app .
COPY /db db

EXPOSE 80
ENTRYPOINT ./ocsp-responder
