using System;
using System.Globalization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

#if !MINIMAL_BUILD
using Tmds.Systemd;
#endif

namespace OcspResponder.Composition
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = BuildHost(args);
            await host.RunAsync();
        }

        private static IHost BuildHost(string[] args)
            => new HostBuilder()
                .ConfigureHostConfiguration(configBuilder => configBuilder.AddCommandLine(args))
                .ConfigureWebHost(webHost => ConfigureWebHost(webHost))
                .ConfigureLogging((builderContext, loggingBuilder) => ConfigureLogging(builderContext, loggingBuilder))
#if !MINIMAL_BUILD
                .UseSystemd()
#endif
                .ConfigureAppConfiguration((builderContext, configBuilder) => ConfigureAppConfiguration(args, builderContext, configBuilder))
                .Build();

        private static IWebHostBuilder ConfigureWebHost(IWebHostBuilder webHost)
            => webHost
                .UseKestrel((builderContext, options) => ConfigureKestrel(builderContext, options))
#if !MINIMAL_BUILD
                .UseLibuv()
#endif
                .UseStartup<Startup>();

        private static void ConfigureLogging(HostBuilderContext builderContext, ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddFilter(
                (category, level) => level >= LogLevel.Warning
                    || (level >= LogLevel.Information && !category.StartsWith("Microsoft.AspNetCore.", StringComparison.OrdinalIgnoreCase)));

#if !MINIMAL_BUILD
            if (Journal.IsSupported)
            {
                loggingBuilder.AddJournal(options =>
                {
                    options.SyslogIdentifier = builderContext.HostingEnvironment.ApplicationName;
                });
            }
#endif

#if !MINIMAL_BUILD
            if (builderContext.Configuration.GetValue<bool>("ForceConsoleLogging")
                 || !Journal.IsAvailable)
#endif
            {
                loggingBuilder.AddConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.Format = ConsoleLoggerFormat.Systemd;
                    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffffffzzz \""
                        + Environment.MachineName
                        + "\" \""
                        + builderContext.HostingEnvironment.ApplicationName
                        + ":\" ";
                });
            }
        }

        private static void ConfigureKestrel(WebHostBuilderContext builderContext, KestrelServerOptions options)
        {
            options.AddServerHeader = false;
            options.Limits.MaxRequestLineSize = 2048;
            options.Limits.MaxRequestBodySize = 65536;
            options.Limits.MaxRequestHeadersTotalSize = 4096;

            var kestrelSection = builderContext.Configuration.GetSection("Kestrel");
            options.Configure(kestrelSection);

            if (kestrelSection.Get<KestrelServerOptions>() is { } kestrelOptions)
            {
                options.Limits.MaxConcurrentConnections = kestrelOptions.Limits.MaxConcurrentConnections;
            }
            else
            {
                options.Limits.MaxConcurrentConnections = 100;
            }

#if !MINIMAL_BUILD
            // SD_LISTEN_FDS_START https://www.freedesktop.org/software/systemd/man/sd_listen_fds.html
            const ulong SdListenFdsStart = 3;

            byte extraListenFds = 0;
            options.UseSystemd(listenOptions =>
            {
                if (listenOptions.FileHandle == SdListenFdsStart)
                {
                    // LISTEN_FDS https://www.freedesktop.org/software/systemd/man/sd_listen_fds.html
                    const string ListenFds = "LISTEN_FDS";
                    if (Environment.GetEnvironmentVariable(ListenFds) is { } listenFdsString)
                    {
                        if (byte.TryParse(listenFdsString, NumberStyles.None, NumberFormatInfo.InvariantInfo, out extraListenFds))
                        {
                            --extraListenFds;
                        }
                    }
                }
            });

            for (ulong handle = SdListenFdsStart + 1, lastHandle = SdListenFdsStart + extraListenFds;
                handle <= lastHandle;
                ++handle)
            {
                options.ListenHandle(handle);
            }
#endif
        }

        private static IConfigurationBuilder ConfigureAppConfiguration(string[] args, HostBuilderContext builderContext, IConfigurationBuilder configBuilder)
            => configBuilder
                .AddJsonFile(builderContext.Configuration.GetValue("ConfigPath", "appsettings.json"), optional: true, reloadOnChange: true)
                .AddCommandLine(args)
                .AddEnvironmentVariables("OCSPR_");
    }
}
