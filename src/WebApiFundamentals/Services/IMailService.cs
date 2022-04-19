// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace WebApiFundamentals.Services;

public interface IMailService
{
    void Send(string subject, string message);
}
