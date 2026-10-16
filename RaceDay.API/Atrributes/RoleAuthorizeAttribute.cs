
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _role;

    public RoleAuthorizeAttribute(string role)
    {
        _role = role;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Check whether the user is logged in
        var userId = context.HttpContext.Session.GetInt32("UserID");

        if (userId == null)
        {
            context.Result = new UnauthorizedObjectResult(
                "Please log in before accessing this endpoint.");

            return;
        }

        // Get the user's role from the session
        var role = context.HttpContext.Session.GetString("Role");

        // Check whether the user has the required role
        if (string.IsNullOrEmpty(role) || role != _role)
        {
            context.Result = new ObjectResult(
                "You do not have permission to access this endpoint.")
            {
                StatusCode = StatusCodes.Status403Forbidden
            };

            return;
        }

        // The user is logged in and has the required role.
        // Allow the request to continue.
    }
}

