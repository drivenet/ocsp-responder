using System;
using System.IO;
using System.Reflection;

using Microsoft.Extensions.Options;

namespace OcspResponder.Core.Infrastructure
{
    internal sealed class ConfigureCaDescriptionDatabaseOptions : IPostConfigureOptions<CaDescriptionDatabaseOptions>
    {
        private static readonly string? ProcessRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

        public void PostConfigure(string name, CaDescriptionDatabaseOptions options)
        {
            var path = options.DatabasePath;
            if (!Path.IsPathFullyQualified(path))
            {
                if (!Path.IsPathRooted(path))
                {
                    if (ProcessRoot is null)
                    {
                        throw new InvalidOperationException("Cannot determine absolute CA description database path.");
                    }

                    path = Path.Combine(ProcessRoot, path);
                }

                path = Path.GetFullPath(path);
            }

            options.DatabasePath = path;
        }
    }
}
