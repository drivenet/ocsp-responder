using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Tmds.Systemd;

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
                .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                .Build();

        private static CommandLineOptions GetCommandLineOptions(string[] args)
            => new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build()
                .Get<CommandLineOptions>() ?? new CommandLineOptions();

        private static HostingOptions GetHostingOptions(string configPath)
            => new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false)
                .Build()
                .Get<HostingOptions>() ?? new HostingOptions();

        private static IHost BuildHost(HostingOptions hostingOptions, IConfiguration appConfiguration)
            => new HostBuilder()
                .ConfigureWebHost(webHost => webHost
                    .UseUrls(hostingOptions.Listen)
                    .UseKestrel(options => ConfigureKestrel(options, hostingOptions))
                    .UseLibuv()
                    .UseStartup<Startup>())
                .ConfigureLogging(loggingBuilder => ConfigureLogging(loggingBuilder, hostingOptions))
                .UseSystemd()
                .ConfigureAppConfiguration(configBuilder => configBuilder.AddConfiguration(appConfiguration))
                .Build();

        private static void ConfigureLogging(ILoggingBuilder loggingBuilder, HostingOptions hostingOptions)
        {
            loggingBuilder.AddFilter(
                (category, level) => level >= LogLevel.Warning
                    || (level >= LogLevel.Information && !category.StartsWith("Microsoft.AspNetCore.", StringComparison.OrdinalIgnoreCase)));
            var hasJournalD = Journal.IsSupported;
            if (hasJournalD)
            {
                loggingBuilder.AddJournal(options =>
                {
                    options.SyslogIdentifier = "gridfs-server";
                    options.DropWhenBusy = true;
                });
            }

            if (!hasJournalD || hostingOptions.ForceConsoleLogging)
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
