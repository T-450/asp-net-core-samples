// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
namespace Asp.NetCore.Identity.Sample.Extensions;

public class AppSettings
{
    public string Secret { get; set; }
    public int ExpirationHours { get; set; }
    public string Issuer { get; set; }
    public string ValidIn { get; set; }
}
