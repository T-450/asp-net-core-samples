// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using HttpClientFactory.Filters;
using HttpClientFactory.Handlers;
using HttpClientFactory.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddTransient<TimingHandler>();
builder.Services.AddTransient<ValidateHeaderHandler>();

// Behind the scenes this will register a few required services,
// one of which will be an implementation of IHttpClientFactory.
// The AddHttpClient has a signature which accepts two generic arguments and wires up DI appropriately.
builder.Services.AddHttpClient<IGitHubHttpService, GitHubHttpService>(client =>
    {
        client.BaseAddress = new Uri("https://api.github.com/");
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactoryTesting");
    }).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10)))
    .AddHttpMessageHandler<
        TimingHandler>() // This handler is on the outside and executes first on the way out and last on the way in.
    .AddHttpMessageHandler<ValidateHeaderHandler>();

// The replace method will find the first registered service of IHttpMessageHandlerBuilderFilter and replace that
// registration with this new one, where our CustomLoggingFilter is the implementation.
builder.Services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CustomLoggingFilter>());

// Share Policy  with two named clients
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

builder.Services.AddHttpClient("github")
    .AddPolicyHandler(timeoutPolicy);
builder.Services.AddHttpClient("google")
    .AddPolicyHandler(timeoutPolicy);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
