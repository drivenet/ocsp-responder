namespace OcspResponder.Core.Infrastructure
{
    internal sealed class ResponderChainOptions
    {
        private string? _certificatePassword;

        public string CertificatePassword
        {
            get => _certificatePassword ?? "";
            set => _certificatePassword = value;
        }
    }
}
