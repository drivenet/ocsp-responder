@echo off
rmdir /s /q packages\linux-x64\ocsp-responder
mkdir packages\linux-x64\ocsp-responder
dotnet publish OcspResponder --force --output packages\linux-x64\ocsp-responder -c Integration -r linux-x64 --no-self-contained
rmdir /s /q packages\linux-x64\ocsp-responder-minimal
mkdir packages\linux-x64\ocsp-responder-minimal
dotnet publish OcspResponder --force --output packages\linux-x64\ocsp-responder-minimal -c Integration -r linux-x64 --no-self-contained -p:MinimalBuild=true
