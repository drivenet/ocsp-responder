namespace OcspResponder.Composition
{
    internal sealed class HostingOptions
    {
        private string? _urls;

        public string? Urls
        {
            get => _urls;
            set
            {
                var listen = value?.Trim();
                if (listen is object && listen.Length == 0)
                {
                    listen = null;
                }

                _urls = listen;
            }
        }

        public ushort MaxConcurrentConnections { get; set; }

        public bool ForceConsoleLogging { get; set; }
    }
}
