﻿# ocsp-responder
## What it is and why it was created?
This is a simple OCSP responder (see [RFC6960](https://tools.ietf.org/html/rfc6960)) built with ASP.NET Core.
I needed an OCSP responder for our internal PKI and found a [wonderful library](https://github.com/gabrielcalegari/OCSPResponder) that simplified the development a lot.

## Configuration
The responder itself can be configured with `appsettings.json` along with `OCSPR_...` environment variables, when configuration file is used, it is automatically reloaded on change.
The web host is configured as a [normal ASP.NET Core app](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#host).
For serverless containers compatibility, the `PORT` envvar is supported, which forces the responder to listen on `http://localhost:<PORT>`.

## Additional endpoints
- `/healthcheck` -- perform a healthcheck
- `/version` -- get version
- `/v0.1/metrics/requests` -- number of OCSP requests
- `/v0.1/metrics/errors` -- number of OCSP errors

## Systemd
It has full support for running via `systemd`, including `Type=notify` unit, socket inheritance via Libuv, journald logging, etc.

## Docker
### Static database
Place database in `db` directory, build with `docker-build`, run with `docker run --rm -it -p 8080:80 -e OCSPR_LOADINTERVAL=0 -e OCSPR_CERTIFICATEPASSWORD=password ocsp-responder`.

### Dynamic host-mounted database
Ensure that `db` directory does not contain anything except `.placeholder`, build with `docker-build`, run with `docker run --rm -it -p 8080:80 -v path-to-local-db:/db -e OCSPR_CERTIFICATEPASSWORD=password ocsp-responder`.
