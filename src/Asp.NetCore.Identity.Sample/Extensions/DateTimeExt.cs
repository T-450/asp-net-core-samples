// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
namespace Asp.NetCore.Identity.Sample.Extensions;

public static class DateTimeExt
{
    public static long ToUnixEpochDate(this DateTime dt, DateTime date)
    {
        var offSet = date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var unixEpochDate = (long) Math.Round(offSet.TotalSeconds);
        return unixEpochDate;
    }
}
