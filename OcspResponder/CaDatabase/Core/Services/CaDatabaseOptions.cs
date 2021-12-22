namespace OcspResponder.CaDatabase.Core.Services;

internal sealed class CaDatabaseOptions
{
    private string? _databasePath;

    public string DatabasePath
    {
        get => _databasePath ?? "db";
        set
        {
            var path = value?.TrimEnd();
            if (path?.Length == 0)
            {
                path = null;
            }

            _databasePath = path;
        }
    }
}
