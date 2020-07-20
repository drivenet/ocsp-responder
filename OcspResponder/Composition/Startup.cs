using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OcspResponder.CaDatabase.Core;
using OcspResponder.CaDatabase.Core.Infrastructure;
using OcspResponder.CaDatabase.Core.Services;
using OcspResponder.CaDatabase.Services;
using OcspResponder.Core;
using OcspResponder.Responder.Core;
using OcspResponder.Responder.Core.Infrastructure;
using OcspResponder.Responder.Core.Services;
using OcspResponder.Responder.Services;

namespace OcspResponder.Composition
{
    public sealed class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<CaDatabaseStore>();
            services.AddSingleton<ICaDescriptionSource>(provider => provider.GetRequiredService<CaDatabaseStore>());
            services.AddSingleton<ICaDatabaseUpdater>(
                provider => new LoggingCaDatabaseUpdater(
                    provider.GetRequiredService<CaDatabaseStore>(),
                    provider.GetRequiredService<ILogger<ICaDatabaseUpdater>>()));
            services.AddSingleton<CaDescriptionLoader>();
            services.AddSingleton<CaDatabaseLoader>();
            services.Configure<CaDatabaseLoaderOptions>(_configuration);
            services.AddSingleton<ICaDatabaseLoader>(
                provider => new LoggingCaDescriptionsLoader(
                    provider.GetRequiredService<CaDatabaseLoader>(),
                    provider.GetRequiredService<ILogger<ICaDatabaseLoader>>()));
            services.AddSingleton<CaDatabasePathsSource>();
            services.Configure<CaDatabaseOptions>(_configuration);
            services.AddSingleton<OpenSslDbParser>();
            services.AddSingleton<ResponderChainLoader>();
            services.Configure<ResponderChainOptions>(_configuration);
            services.ConfigureOptions<ConfigureCaDatabaseOptions>();
            services.AddSingleton<Core.OcspResponder>();
            services.AddSingleton<OcspResponderEx>();
            services.AddSingleton<IOcspResponderEx>(
                provider => new LoggingOcspResponderEx(
                    provider.GetRequiredService<OcspResponderEx>(),
                    provider.GetRequiredService<ILogger<IOcspResponder>>()));
            services.AddSingleton<IOcspResponder>(
                provider => new MetricsCollectingOcspResponder(
                    provider.GetRequiredService<Core.OcspResponder>(),
                    provider.GetRequiredService<IMetricRecorder>()));
            services.AddSingleton<MetricCollector>();
            services.AddSingleton<IMetricRecorder>(provider => provider.GetRequiredService<MetricCollector>());
            services.AddSingleton<IMetricReader>(provider => provider.GetRequiredService<MetricCollector>());
            services.AddSingleton<OcspResponderRepository>();
            services.AddSingleton<IOcspResponderRepository>(
                provider => new LoggingOcspResponderRepository(
                    provider.GetRequiredService<OcspResponderRepository>(),
                    provider.GetRequiredService<ILogger<IOcspResponderRepository>>()));
            services.AddSingleton<OcspLogger>();
            services.AddSingleton<IOcspLogger>(
                provider => new MetricsCollectingOcspLogger(
                    provider.GetRequiredService<OcspLogger>(),
                    provider.GetRequiredService<IMetricRecorder>()));
            services.AddHostedService<CaDatabaseLoaderService>();
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
