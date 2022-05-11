@echo off
rmdir /s /q packages\linux-x64\ocsp-responder
mkdir packages\linux-x64\ocsp-responder
dotnet publish OcspResponder --force --output packages\linux-x64\ocsp-responder -c Integration -r linux-x64 --no-self-contained
del packages\linux-x64\ocsp-responder\*.deps.json
