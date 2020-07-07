# ocsp-responder
## What it is and why it was created?
This is a simple OCSP responder (see [RFC6960](https://tools.ietf.org/html/rfc6960)) built with ASP.NET Core.
I needed an OCSP responder for our internal PKI and found a [wonderful library](https://github.com/gabrielcalegari/OCSPResponder) that simplified the development a lot.

## Configuration
It can be configured with `hostingsettings.json` + `appsettings.json` (paths are configurable via command line), also with `OCSPR_HOST_...` and `OCSPR_APP_...` environment variables.

## Systemd
It has full support for running via `systemd`, including `Type=notify` unit, socket inheritance via Libuv, journald logging, etc.

## Docker
`docker-build`
`docker run --rm -it -p 8080:80 -v path-to-local-db:/db -e OCSPR_CERTIFICATEPASSWORD=password ocsp-responder`
