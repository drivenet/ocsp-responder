using System.Net.Mime;
using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Mvc;

namespace OcspResponder.Controllers
{
    public sealed class VersionController : Controller
    {
        private static readonly string VersionString = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "?";

        [Route("version")]
        public ContentResult GetVersion()
            => Content(VersionString, MediaTypeNames.Text.Plain, Encoding.ASCII);
    }
}
