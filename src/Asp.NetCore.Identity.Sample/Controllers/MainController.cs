// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Asp.NetCore.Identity.Sample.Controllers;

[ApiController]
public abstract class MainController : Controller
{
    protected ICollection<string> Errors = new List<string>();

    protected ActionResult CustomResponse(object result = null)
    {
        if (IsOperationValid())
        {
            return Ok(result);
        }

        var validationProblems = new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            {
                "Messages", Errors.ToArray()
            }
        });
        return BadRequest(validationProblems);
    }

    protected ActionResult CustomResponse(ModelStateDictionary modelState)
    {
        var errors = modelState.Values.SelectMany(e => e.Errors);
        foreach (var error in errors)
        {
            AddErrors(error.ErrorMessage);
        }

        return CustomResponse();
    }

    protected bool IsOperationValid()
    {
        return !Errors.Any();
    }

    protected void AddErrors(string error)
    {
        Errors.Add(error);
    }

    protected void ClearErrors()
    {
        Errors.Clear();
    }
}
