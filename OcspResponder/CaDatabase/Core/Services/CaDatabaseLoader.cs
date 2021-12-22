using System;
using System.Linq;

using OcspResponder.CaDatabase.Services;

namespace OcspResponder.CaDatabase.Core.Services;

internal sealed class CaDatabaseLoader : ICaDatabaseLoader
{
    private readonly ICaDatabaseUpdater _updater;
    private readonly CaDescriptionLoader _loader;
    private readonly CaDatabasePathsSource _fileSource;

    public CaDatabaseLoader(ICaDatabaseUpdater updater, CaDescriptionLoader loader, CaDatabasePathsSource fileSource)
    {
        _updater = updater ?? throw new ArgumentNullException(nameof(updater));
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _fileSource = fileSource ?? throw new ArgumentNullException(nameof(fileSource));
    }

    public void Load()
    {
        var paths = _fileSource.GetPaths();
        var now = DateTimeOffset.UtcNow;
        var descriptions = paths
            .Select(path => _loader.Load(path, now))
            .ToList();
        _updater.Update(descriptions);
    }
}
