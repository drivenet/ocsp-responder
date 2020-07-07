using System;
using System.IO;
using System.Reflection;

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
            var databasePath = _configuration.GetValue<string>("databasePath") ?? throw new ArgumentException("Missing database path.");
            var fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "", databasePath));
            var password = _configuration.GetValue<string>("certificatePassword") ?? throw new ArgumentException("Missing certificate password.");
            services.AddControllers();
            services.AddSingleton<CaDescriptionStore>();
            services.AddSingleton<ICaDescriptionSource>(provider => provider.GetRequiredService<CaDescriptionStore>());
            services.AddSingleton<ICaDescriptionUpdater>(
                provider => new LoggingCaDescriptionUpdater(
                    provider.GetRequiredService<CaDescriptionStore>(),
                    provider.GetRequiredService<ILogger<ICaDescriptionUpdater>>()));
            services.AddSingleton<CaDescriptionLoader>();
            services.AddSingleton<CaDescriptionsLoader>();
            services.AddSingleton<ICaDescriptionsLoader>(
                provider => new LoggingCaDescriptionsLoader(
                    provider.GetRequiredService<CaDescriptionsLoader>(),
                    provider.GetRequiredService<ILogger<ICaDescriptionsLoader>>()));
            services.AddSingleton(_ => new CaDescriptionFilesSource(fullPath));
            services.AddSingleton<OpenSslDbParser>();
            services.AddSingleton(_ => new ResponderChainLoader(password));
            services.AddSingleton<IOcspResponder, Core.OcspResponder>();
            services.AddSingleton<IOcspResponderRepository, OcspResponderRepository>();
            services.AddSingleton<IOcspLogger, OcspLogger>();
            services.AddHostedService<BackgroundUpdaterService>();
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
