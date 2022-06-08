## Action Filters in ASP.NET Core
- A great way to hook into the [MVC](https://code-maze.com/asp-net-core-mvc-series/) action invocation pipeline.
-  Use filters to extract code that can be reused and make our actions cleaner and maintainable.
- Filter types:
	- **Authorization filters:** They run first to determine whether a user is authorized for the current request;
	- **Resource filters:** They run right after the authorization filters and are very useful for caching and performance;
	- **Action filters:** They run right before and after the action method execution;
	- **Exception filters:** They are used to handle exceptions before the response body is populated;
	- **Result filters:** They run before and after the execution of the action methods result.
 
 Order of execution: 
 
  ![](https://geeksarray.com/images/blog/asp-net-core-mvc-filter-execution-sequence.png)


---

## Resources:

[How to use ASP.NET Core MVC built-in Filters](https://geeksarray.com/blog/how-to-use-asp-net-core-mvc-built-in-filters)

[ASP.NET CORE ACTION ARGUMENTS VALIDATION USING AN ACTIONFILTER](https://damienbod.com/2016/09/09/asp-net-core-action-arguments-validation-using-an-actionfilter/)

[Filters in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-6.0)

[Recommended way to read the Request.Body in an ActionFilter](https://github.com/aspnet/Mvc/issues/5260)

[Implementing Action Filters in ASP.NET Core](https://code-maze.com/action-filters-aspnetcore/)

[action-filters-dotnetcore-webapi](https://github.com/CodeMazeBlog/action-filters-dotnetcore-webapi)
