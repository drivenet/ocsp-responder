using System;
using System.Linq;

using OcspResponder.CaDatabase.Services;

namespace OcspResponder.CaDatabase.Core.Services;

internal sealed class CaDatabaseLoader : ICaDatabaseLoader
{
    private readonly ICaDatabaseUpdater _updater;
    private readonly CaDescriptionLoader _loader;
    private readonly CaDatabasePathsSource _fileSource;
    private readonly TimeProvider _timeProvider;

    public CaDatabaseLoader(ICaDatabaseUpdater updater, CaDescriptionLoader loader, CaDatabasePathsSource fileSource, TimeProvider timeProvider)
    {
        _updater = updater ?? throw new ArgumentNullException(nameof(updater));
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _fileSource = fileSource ?? throw new ArgumentNullException(nameof(fileSource));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public void Load()
    {
        var paths = _fileSource.GetPaths();
        var now = _timeProvider.GetUtcNow();
        var descriptions = paths
            .Select(path => _loader.Load(path, now))
            .ToList();
        _updater.Update(descriptions);
    }
}
