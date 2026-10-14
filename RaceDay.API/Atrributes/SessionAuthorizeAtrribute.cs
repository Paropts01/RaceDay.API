using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _role;

    public RoleAuthorizeAttribute(string role) => _role = role;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userId = context.HttpContext.Session.GetInt32("UserID");
        if (userId == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var role = context.HttpContext.Session.GetString("Role");
        if (role != _role)
            context.Result = new ForbidResult();
    }
}
