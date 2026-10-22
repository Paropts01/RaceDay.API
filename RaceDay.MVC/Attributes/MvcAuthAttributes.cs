using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.MVC.Attributes;

public class SessionRequiredAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Session.GetInt32("UserID") == null)
        {
            context.Result = new RedirectToActionResult(
                "Login", "Account", null);
        }
    }
}

public class RoleRequiredAttribute : ActionFilterAttribute
{
    private readonly string _role;

    public RoleRequiredAttribute(string role)
    {
        _role = role;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Session.GetInt32("UserID") == null)
        {
            context.Result = new RedirectToActionResult(
                "Login", "Account", null);
            return;
        }

        if (context.HttpContext.Session.GetString("Role") != _role)
        {
            context.Result = new RedirectToActionResult(
                "Index", "Home", null);
        }
    }
}
