using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#if !MINIMAL_BUILD
using Tmds.Systemd;
#endif

namespace OcspResponder.Composition
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var commandLineOptions = GetCommandLineOptions(args);
            var appConfiguration = LoadAppConfiguration(commandLineOptions.Config);
            var hostingConfigPath = commandLineOptions.HostingConfig;
            var hostingOptions = GetHostingOptions(hostingConfigPath);
            using var host = BuildHost(hostingOptions, appConfiguration);
            await host.RunAsync();
        }

        private static IConfiguration LoadAppConfiguration(string configPath)
            => new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("OCSPR_APP_")
                .Build();

        private static CommandLineOptions GetCommandLineOptions(string[] args)
            => new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build()
                .Get<CommandLineOptions>() ?? new CommandLineOptions();

        private static HostingOptions GetHostingOptions(string configPath)
            => new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: true)
                .AddEnvironmentVariables("OCSPR_HOST_")
                .Build()
                .Get<HostingOptions>() ?? new HostingOptions();

        private static IHost BuildHost(HostingOptions hostingOptions, IConfiguration appConfiguration)
            => new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost
                        .UseUrls(hostingOptions.Urls)
                        .UseKestrel(options => ConfigureKestrel(options, hostingOptions))
                        .UseStartup<Startup>();
#if !MINIMAL_BUILD
                    if (!hostingOptions.NoLibUv)
                    {
                        webHost.UseLibuv();
                    }
#endif
                })
                .ConfigureLogging(loggingBuilder => ConfigureLogging(loggingBuilder, hostingOptions))
#if !MINIMAL_BUILD
                .UseSystemd()
#endif
                .ConfigureAppConfiguration(configBuilder => configBuilder.AddConfiguration(appConfiguration))
                .Build();

#pragma warning disable CA1801 // Review unused parameters -- conditional compilation
        private static void ConfigureLogging(ILoggingBuilder loggingBuilder, HostingOptions hostingOptions)
#pragma warning restore CA1801 // Review unused parameters
        {
            loggingBuilder.AddFilter(
                (category, level) => level >= LogLevel.Warning
                    || (level >= LogLevel.Information && !category.StartsWith("Microsoft.AspNetCore.", StringComparison.OrdinalIgnoreCase)));
#if !MINIMAL_BUILD
            if (Journal.IsSupported)
            {
                loggingBuilder.AddJournal(options =>
                {
                    options.SyslogIdentifier = "ocsp-responder";
                    options.DropWhenBusy = true;
                });
            }
#endif

#if !MINIMAL_BUILD
            if (hostingOptions.ForceConsoleLogging || !Journal.IsAvailable)
#endif
            {
                loggingBuilder.AddConsole(options => options.DisableColors = true);
            }
        }

        private static void ConfigureKestrel(KestrelServerOptions options, HostingOptions hostingOptions)
        {
            options.AddServerHeader = false;
            options.Limits.MaxRequestBodySize = 65536;
            options.Limits.MaxRequestHeadersTotalSize = 4096;

            if (hostingOptions is object)
            {
                var maxConcurrentConnections = hostingOptions.MaxConcurrentConnections;
                if (maxConcurrentConnections != 0)
                {
                    options.Limits.MaxConcurrentConnections = maxConcurrentConnections;
                }
            }

            options.UseSystemd();
        }
    }
}
