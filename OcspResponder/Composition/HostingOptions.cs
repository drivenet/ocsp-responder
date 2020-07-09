namespace OcspResponder.Composition
{
    internal sealed class HostingOptions
    {
        private string? _config;

        public string Config
        {
            get => string.IsNullOrWhiteSpace(_config) ? "appsettings.json" : _config;
            set => _config = value;
        }

        public ushort MaxConcurrentConnections { get; set; } = 100;

#if !MINIMAL_BUILD
        public bool ForceConsoleLogging { get; set; }
#endif
    }
}
