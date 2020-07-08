using System;
using System.Diagnostics;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OcspResponder.Core;
using OcspResponder.Core.Infrastructure;
using OcspResponder.Core.Services;
using OcspResponder.Services;

namespace OcspResponder.Composition
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<CaDatabase>();
            services.AddSingleton<ICaDescriptionSource>(provider => provider.GetRequiredService<CaDatabase>());
            services.AddSingleton<ICaDatabaseUpdater>(
                provider => new LoggingCaDatabaseUpdater(
                    provider.GetRequiredService<CaDatabase>(),
                    provider.GetRequiredService<ILogger<ICaDatabaseUpdater>>()));
            services.AddSingleton<CaDescriptionLoader>();
            services.AddSingleton<CaDatabaseLoader>();
            services.AddSingleton<ICaDatabaseLoader>(
                provider => new LoggingCaDescriptionsLoader(
                    provider.GetRequiredService<CaDatabaseLoader>(),
                    provider.GetRequiredService<ILogger<ICaDatabaseLoader>>()));
            services.AddSingleton<CaDescriptionFilesSource>();
            services.Configure<CaDescriptionDatabaseOptions>(_configuration);
            services.AddSingleton<OpenSslDbParser>();
            services.AddSingleton<ResponderChainLoader>();
            services.Configure<ResponderChainOptions>(_configuration);
            services.ConfigureOptions<ConfigureCaDescriptionDatabaseOptions>();
            services.AddSingleton<Core.OcspResponder>();
            services.AddSingleton(
                provider => new LoggingOcspResponder(
                    provider.GetRequiredService<Core.OcspResponder>(),
                    provider.GetRequiredService<ILogger<IOcspResponder>>()));
            services.AddSingleton<IOcspResponder>(
                provider => new MetricsCollectingOcspResponder(
                    provider.GetRequiredService<LoggingOcspResponder>(),
                    provider.GetRequiredService<IMetricRecorder>()));
            services.AddSingleton<MetricCollector>();
            services.AddSingleton<IMetricRecorder>(provider => provider.GetRequiredService<MetricCollector>());
            services.AddSingleton<IOcspResponderRepository, OcspResponderRepository>();
            services.AddSingleton<IOcspLogger, OcspLogger>();
            services.AddHostedService<CaDatabaseUpdaterService>();
        }

#pragma warning disable CA1822 // Mark members as static -- future-proofing
        public void Configure(IApplicationBuilder app)
#pragma warning restore CA1822 // Mark members as static
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
