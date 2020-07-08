using System.Collections.Generic;
using System.Net;

namespace OcspResponder.Services
{
    public interface IMetricReader
    {
        IEnumerable<KeyValuePair<string, long>> RequestMethodCounters { get; }

        IEnumerable<KeyValuePair<HttpStatusCode, long>> ResponseStatusCounters { get; }
    }
}
