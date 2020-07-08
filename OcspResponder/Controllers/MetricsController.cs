using System;
using System.Globalization;
using System.Net.Mime;
using System.Text;

using Microsoft.AspNetCore.Mvc;

using OcspResponder.Responder.Services;

namespace OcspResponder.Controllers
{
    [Route("v0.1/metrics")]
    public sealed class MetricsController : Controller
    {
        private readonly IMetricReader _metricReader;

        public MetricsController(IMetricReader metricReader)
        {
            _metricReader = metricReader ?? throw new ArgumentNullException(nameof(metricReader));
        }

        [Route("requests/total")]
        public ActionResult GetTotalRequests()
            => Content(_metricReader.Requests);

        [Route("errors/total")]
        public ActionResult GetTotalErrors()
            => Content(_metricReader.Errors);

        private ActionResult Content(ulong counter)
        {
            return Content(
                counter.ToString(NumberFormatInfo.InvariantInfo),
                MediaTypeNames.Text.Plain,
                Encoding.ASCII);
        }
    }
}
