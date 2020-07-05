namespace OcspResponder.Composition
{
    internal sealed class CommandLineOptions
    {
        private string? _config;

        private string? _hostingConfig;

        public string Config
        {
            get => string.IsNullOrWhiteSpace(_config) ? "appsettings.json" : _config;
            set => _config = value;
        }

        public string HostingConfig
        {
            get => string.IsNullOrWhiteSpace(_hostingConfig) ? "hostingsettings.json" : _hostingConfig;
            set => _hostingConfig = value;
        }
    }
}
