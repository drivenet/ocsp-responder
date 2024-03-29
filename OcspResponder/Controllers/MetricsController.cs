﻿using System;
using System.Globalization;
using System.Net.Mime;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using OcspResponder.Responder.Services;

namespace OcspResponder.Controllers;

[Route("v0.1/metrics")]
public sealed class MetricsController : Controller
{
    private readonly IMetricReader _metricReader;

    public MetricsController(IMetricReader metricReader)
    {
        _metricReader = metricReader ?? throw new ArgumentNullException(nameof(metricReader));
    }

    [Route("requests")]
    public ContentResult GetRequests()
        => Content(_metricReader.Requests);

    [Route("errors")]
    public ContentResult GetErrors()
        => Content(_metricReader.Errors);

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var connection = context.HttpContext.Connection;
        if (connection.RemoteIpAddress?.Equals(connection.LocalIpAddress) != true)
        {
            context.Result = StatusCode(StatusCodes.Status403Forbidden);
            return;
        }

        base.OnActionExecuting(context);
    }

    private ContentResult Content(ulong counter)
        => Content(
            counter.ToString(NumberFormatInfo.InvariantInfo),
            MediaTypeNames.Text.Plain,
            Encoding.ASCII);
}
