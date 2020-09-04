@echo off
rmdir /s /q packages\linux-x64\ocsp-responder
mkdir packages\linux-x64\ocsp-responder
dotnet publish OcspResponder --output packages\linux-x64\ocsp-responder -c Integration -r linux-x64 --self-contained false
move packages\linux-x64\ocsp-responder\Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.dll "%TEMP%"
move packages\linux-x64\ocsp-responder\Microsoft.Extensions.Hosting.Systemd.dll "%TEMP%"
del packages\linux-x64\ocsp-responder\web.config packages\linux-x64\ocsp-responder\*.deps.json packages\linux-x64\ocsp-responder\appsettings.json packages\linux-x64\ocsp-responder\Microsoft.*.dll
move "%TEMP%\Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.dll" packages\linux-x64\ocsp-responder
move "%TEMP%\Microsoft.Extensions.Hosting.Systemd.dll" packages\linux-x64\ocsp-responder
