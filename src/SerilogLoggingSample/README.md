# Effective Logging in ASP.NET Core

## Table of Contents

Tips and tricks to write logs.

### What makes logging “effective”?

- **Good, consistent Information:** an entry contains all of the information that might be pertinent to understanding
  the event. If an exception occurs, we should know about the route or page it occurred on, any parameters that may have
  influenced the behavior and even the user that was logged in;
- **Keeps application code clean:** We should have application code that is easy to read without having to navigate lots
  of exception handling or logging logic. A corollary here is that the act of logging should not affect the performance
  or behavior of the application;
- **Easily consumable:** This generally means some kind of web interface to review and summarize log entries and
  generally not just entries that are getting written to the files on web servers themselves. Support people, testers,
  developers, and sometimes business analysts or product managers should be able to see and review log entries;
- Improves “fixability”: This often means that you can understand and fix the problem simply by looking at a log entry
  and the code, and should rarely need to set up complex scenarios to reproduce a problem that was reported. Test the
  fix, but making the fix should not require reproducing the problem as a general rule. Good logging can enable a much
  deeper understanding of your application;
- **Enables deeper understanding:**  Good logging can enable a much deeper understanding of your application. If you're
  logging performance information for every API call, for example, you can easily start to see the volume of calls your
  API is fielding, as well as the average, max, and min timings for those calls. Maybe your performance slows down
  around noon, but the load doesn't really change;
- **Adds data for priorization:** Having good log entries and aggregation techniques can help you sniff out anomalies
  and feature usage. Having good aggregation means you have good information that can feed a prioritization process,
  like which errors should you fix first, or what should be your next performance improvement area? This may be based on
  sheer volume of occurrences or other things like which users are affected. But either way, you should have some data
  to support these types of decisions;

## Log Levels

| Level | Description |
| --- | --- |
| Trace/Verbose | The lowest level of log entries. This is a very noisy level of logging and should only be used in the lowest level of troubleshooting. Used for complex and low-level issues one is trying to resolve; |
| Debug | Where you could review and use your own messages for development and debugging purposes. The same thing as Console.Write. |
| Information |  Trace the general flow of the application. Used for performance log entry or the fact that a user did something might be places where one could use informational entries. |
| Information |  Trace the general flow of the application. Used for performance log entry or the fact that a user did something might be places where one could use informational entries. |
| Error | Handled exceptions. |
| Critical/Fatal | Used to log a complete application failure - something like the database being unavailable or transaction logs filling up. |

## Categories and EventIds: Logical Grouping

### Categories

Used to apply logical groupings to log entries. Defining categories for your log entries can help you isolate entries
and events ocurring in a certain class.

```markdown
ILogger<ClassName>

# Logged as "SourceContext";
# Use LoggerFactory **to define custom;
# Easy to see everything in "Category";**
```

### EventsIds

Using **EventIds** can make it easy to see all of your log entries for a certain EventId or even a set of them.

```markdown
# Officially struct with Id and Name;
# Can just be Id (int) - need lookup;
# Easy to see all of the certain type of event;
```

## Filtering

Apply minimum levels to different things.

**Minimum levels:**

- Global;
- Per Category;
- Per provider / sink;
- Per Provider / sink category;

Resource:

[Logging in .NET Core and ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0#how-filtering-rules-are-applied)

[Configuration Basics · serilog/serilog Wiki](https://github.com/serilog/serilog/wiki/Configuration-Basics#filters)

## Scopes

Groups a set of logical operations together for example, capturing everything inside a single transaction.

**Resources:**

[High-performance logging with LoggerMessage in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/loggermessage?view=aspnetcore-2.2)

## Automating Standard Entries

### Exception Handling and Logging

- **Do not use the DeveloperExceptionPage:**
    - It exposes sensitive information.
    - Crutch in development - app is different in non-dev;

    ```csharp
    // Don't
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    // Do
    app.UseExceptionHandler("/Error");
    ```

- **Shield error details:**
    - Provide some kind of “ID”;
    - Log all details (mask sensitive);
- **Global handling covert most scenarios:**
    - Different between UI and API: The UI is going to need to show a user - friendly web page as a response, whereas an
      API needs to let a programmatic caller know that a problem has ocurred and no user interface is involved;
    - When developing API’s create an error handling Middleware;

**Resources:**

[Enrichment · serilog/serilog Wiki](https://github.com/serilog/serilog/wiki/Enrichment)

**[Exception Triage with Serilog](https://nblumhardt.com/2017/03/exception-triage/)**

[Exception handling middleware example](https://github.com/dahlsailrunner/aspnetcore-effective-logging/tree/master/AspNetCore-Effective-Logging/BookClub.Infrastructure/Middleware)

## Anatomy of a Log Entry

| Field | Description | Sample |
| --- | --- | --- |
| Timestamp | Time of the log entry | … |
| Level | Level of the entry | Information |
| Message Template | Entry with replaceable placeholders | {UserId} did {ActivityName} |
| Message | Entry with  replacements made | 123 did Book Submission |
| SourceContext | Category for the log entry | BookClub.API.Controllers.BookController |
| ActionID | Identifier for the Action (spans requests) | 7bda7a55-d8cf-46d7-9f57-98388a1e5956 |
| ActionName | Fully-qualified namespace / class / method | BookClub.API.Controllers.BookController.GetBooks (BookClub.Api) |
| RequestId | Unique id for the request | 1c549fb0-54d7-4b58-bf78-e926f7b6d8f3 |
| RequestPath | Local path for the request | /api/Book |
| CorrelationId | Something like session id to track user activity within a session |  |
| <replacement values> | Replacement values for the MessageTemplate |  |
| Exception | ToString( ) representation |  |
| Machine name |  |  |
| Enviroment |  |  |
| User Id |  |  |
| Role(s) |  |  |
| Entry Assembly |  |  |
| Version |  |  |
| Customer Id |  |  |
| HttpContext |  |  |
|  |  |  |

**Obs: Include what’s important to YOU**

- DO consider what will truly assist in troubleshooting / analysis
- DON’T log what you don’t need to log - it can be expensive!
- Excluding the standard info is harder
- DON’T add information that repeats already-included info

### Sugested Logs

- UserId;
- Scopes (OAuth2);
- Machine;
- EntryAssembly;
- Timestamp;

### Use Scope to add the info

- Go back to performance filters;
- Add scope info;
- Verify results;

### Detect higher exception levels

- Is application completely down or did a user experience an error?
- Can be used for alerting;

### Don’t log sensitive information

- Know what constitutes “sensitive”;
- Enable troubleshooting, but protect information;
- Do the protection at point-of-logging:
    - “Formatters” could help you centralize this;
    - Probably based on “field name”;
    - Add masks to sensitive information:

        ```csharp
        // Example to mask the user email address
        userDict.Add("Email", MaskEmailAddress(context.HttpContext.User.FindFirst("email")?.Value));

        private string MaskEmailAddress(string emailAddress)
        {
            int? atIndex = emailAddress?.IndexOf('@');
            if (atIndex > 1)
                return string.Format("{0}{1}***{2}", emailAddress[0], emailAddress[1],
                    emailAddress.Substring(atIndex.Value));
            return emailAddress;
        }
        ```

**Resources:**

[Serilog Best Practices](https://benfoster.io/blog/serilog-best-practices/#source-context)

**[Maintaining separate scope context with serilog in asp.net core](https://stackoverflow.com/questions/66359411/maintaining-separate-scope-context-with-serilog-in-asp-net-core)**

**[Logging with Serilog.Exceptions](https://rehansaeed.com/logging-with-serilog-exceptions/)**

## Types of log consumption

- Support for individual errors;
- Monthly reporting;
- Alerting and Monitoring;

### What’s wrong with files?

- Hard to query - analysis is made difficult;
- Need to be joined together if different servers / applications;
- No user interface available for exploration / search;

### Provider / Sink / Target Options / Appenders

**AZURE**

- Application Insights is easy go-to;
- Azure App Services Diagnostics is available;

**SERILOG**

- Seq is great option, SQL Server or ELK;

**Resources:**

[Serilog Provided Sinks](https://github.com/serilog/serilog/wiki/Provided-Sinks)

[Application Insights overview](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

[Official Linux images for the Seq log server](https://hub.docker.com/r/datalust/seq)

[Logging With ElasticSearch, Kibana, Serilog Using ASP.NET Core Docker](https://www.c-sharpcorner.com/article/logging-with-elasticsearch-kibana-serilog-using-asp-net-core-docker/)

[.Net 6 WebAPI - Intro to ElasticSearch & Kibana - Step by Step](https://dev.to/moe23/net-6-webapi-intro-to-elasticsearch-kibana-step-by-step-p9l)

[Logging with Serilog and Seq](https://www.code4it.dev/blog/logging-with-serilog-and-seq)

[Azure App Service diagnostics overview](https://docs.microsoft.com/en-us/azure/app-service/overview-diagnostics)

[Using Azure App Service Diagnostics](https://app.pluralsight.com/course-player?clipId=038437bd-e2d1-41eb-a7bd-1c31b6819294)
