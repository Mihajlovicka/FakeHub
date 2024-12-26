using FakeHubApi.Model.ServiceResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FakeHubApi.Filters;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errorMessages = new List<string>();
            foreach (var entry in context.ModelState.Values)
            {
                foreach (var entryError in entry.Errors)
                {
                    errorMessages.Add(entryError.ErrorMessage);
                }
            }
            var response = new ResponseBase()
            {
                Success = false,
                ErrorMessage = string.Join(" \n", errorMessages),
            };
            context.Result = new BadRequestObjectResult(response);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
