using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace RaceDay.API.Atrributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles) => _roles = roles;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            var role = context.HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (_roles.Any() && !_roles.Contains(role))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
