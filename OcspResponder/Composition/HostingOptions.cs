namespace OcspResponder.Composition
{
    internal sealed class HostingOptions
    {
        private string? _listen;

        public string? Listen
        {
            get => _listen;
            set
            {
                var listen = value?.Trim();
                if (listen is object && listen.Length == 0)
                {
                    listen = null;
                }

                _listen = listen;
            }
        }

        public ushort MaxConcurrentConnections { get; set; }

        public bool ForceConsoleLogging { get; set; }
    }
}
