using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Server.API.Controllers.Gaurd
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private string[] _roles;

        public CustomAuthorizeAttribute(string roles)
        {
            _roles = roles.Split(',').Select(r => r.Trim()).ToArray();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Kiểm tra token có hợp lệ không
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Kiểm tra role có tồn tại trong claims không

            var hasRole = user.Claims.Any(c => c.Type == ClaimTypes.Role &&
                                               _roles.Contains(c.Value));
            if (!hasRole)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
