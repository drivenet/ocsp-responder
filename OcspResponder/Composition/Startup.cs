using System;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OcspResponder.Core;
using OcspResponder.Implementation;

namespace OcspResponder.Composition
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<CaDescriptionStore>();
            services.AddSingleton<ICaDescriptionSource>(provider => provider.GetRequiredService<CaDescriptionStore>());
            services.AddSingleton<CaDescriptionLoader>();
            services.AddSingleton<CaDescriptionUpdater>();
            services.AddSingleton(_ => new CaDescriptionFilesSource(Path.GetFullPath("../..")));
            services.AddSingleton<OpenSslDbParser>();
            services.AddSingleton<ResponderChainLoader>();
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
