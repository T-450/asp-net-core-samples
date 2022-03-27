// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FileUpload.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace FileUpload.Filters;

public class ValidateMimeMultipartContentFilterService : ActionFilterAttribute
{
    private readonly ILogger _logger;
    private readonly int _maxAllowedFileSize;

    public ValidateMimeMultipartContentFilterService(ILoggerFactory loggerFactory, IConfiguration config)
    {
        _maxAllowedFileSize = config.GetValue<int>("MaxFileSize");
        _logger = loggerFactory.CreateLogger("ValidateMimeMultipartContentFilterAttribute");
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var req = context.HttpContext.Request;
        ValidateFileHeaders(context, req);

        base.OnActionExecuting(context);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var req = context.HttpContext.Request;
        if (string.IsNullOrEmpty(req.ContentType) || !MultipartRequestHelper.IsMultipartContentType(req.ContentType))
        {
            context.Result = new StatusCodeResult(415);
        }

        base.OnActionExecuted(context);
    }

    private void ValidateFileHeaders(ActionExecutingContext context, HttpRequest req)
    {
        if (string.IsNullOrEmpty(req.ContentType) || !MultipartRequestHelper.IsMultipartContentType(req.ContentType))
        {
            context.Result = new StatusCodeResult(415);
        }
        else if (!MediaTypeHeaderValue.TryParse(req.ContentType, out var mediaTypeHeader) ||
                 string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
        {
            context.Result = new StatusCodeResult(415);
        }
        else if (!MultipartRequestHelper.TryGetBoundary(mediaTypeHeader, _maxAllowedFileSize, out var boundary)
                 || string.IsNullOrEmpty(boundary))
        {
            context.Result = new StatusCodeResult(415);
        }
        else if (req!.Body == null)
        {
            context.Result = new BadRequestResult();
        }
    }
}
