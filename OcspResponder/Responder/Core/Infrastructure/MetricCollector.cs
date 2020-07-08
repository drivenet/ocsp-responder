using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

using OcspResponder.Core;
using OcspResponder.Responder.Services;

namespace OcspResponder.Responder.Core.Infrastructure
{
    internal sealed class MetricCollector : IMetricRecorder, IMetricReader
    {
        private static readonly Func<string, long, long> IncrementMethod = (_, counter) => counter + 1;
        private static readonly Func<HttpStatusCode, long, long> IncrementStatus = (_, counter) => counter + 1;

        private readonly ConcurrentDictionary<string, long> _requestMethodCounters = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentDictionary<HttpStatusCode, long> _responseStatusCounters = new ConcurrentDictionary<HttpStatusCode, long>();

        public IEnumerable<KeyValuePair<string, long>> RequestMethodCounters => _requestMethodCounters;

        public IEnumerable<KeyValuePair<HttpStatusCode, long>> ResponseStatusCounters => _responseStatusCounters;

        public void Record(OcspHttpRequest request) => _requestMethodCounters.AddOrUpdate(request.HttpMethod, 0, IncrementMethod);

        public void Record(OcspHttpResponse response) => _responseStatusCounters.AddOrUpdate(response.Status, 0, IncrementStatus);
    }
}
