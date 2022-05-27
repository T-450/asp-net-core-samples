// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
namespace RateLimiting;

[AttributeUsage(AttributeTargets.Method)]
public class LimitRequests : Attribute
{
    public int TimeWindow { get; set; }
    public int MaxRequests { get; set; }
}
