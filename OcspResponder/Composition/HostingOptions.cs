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

#if !MINIMAL_BUILD
        public bool ForceConsoleLogging { get; set; }
#endif

#if !MINIMAL_BUILD
        public bool NoLibUv { get; set; }
#endif
    }
}
