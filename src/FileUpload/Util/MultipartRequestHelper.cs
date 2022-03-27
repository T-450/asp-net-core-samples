// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Net.Http.Headers;

namespace FileUpload.Util;

public static class MultipartRequestHelper
{
    // Ex: Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
    // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
    public static bool TryGetBoundary(MediaTypeHeaderValue contentType, int lengthLimit, out string? headerBoundary)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
        if (string.IsNullOrEmpty(boundary) || boundary.Length > lengthLimit)
        {
            headerBoundary = boundary;
            return false;
        }

        headerBoundary = boundary;
        return true;
    }

    public static bool IsMultipartContentType(string? contentType)
    {
        return !string.IsNullOrEmpty(contentType) &&
               contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasContentDispositionHeader(ContentDispositionHeaderValue? contentDisposition)
    {
        // Content-Disposition: form-data; name="key";
        return contentDisposition != null && contentDisposition.DispositionType.Equals("form-data") &&
               string.IsNullOrEmpty(contentDisposition.FileName.Value) &&
               string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
    }

    public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        return contentDisposition != null
               && contentDisposition.DispositionType.Equals("form-data")
               && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                   || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }
}
