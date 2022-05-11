using System;
using System.IO;

using Microsoft.Extensions.Options;

namespace OcspResponder.CaDatabase.Core.Services;

internal sealed class ConfigureCaDatabaseOptions : IPostConfigureOptions<CaDatabaseOptions>
{
    public void PostConfigure(string name, CaDatabaseOptions options)
    {
        var path = options.DatabasePath;
        if (!Path.IsPathFullyQualified(path))
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(AppContext.BaseDirectory, path);
            }

            path = Path.GetFullPath(path);
        }

        options.DatabasePath = path;
    }
}
