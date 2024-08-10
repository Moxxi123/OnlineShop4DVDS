using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Ultility;

namespace Authorize
{
    public class AdminOrSelfHandler : AuthorizationHandler<AdminOrSelfRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminOrSelfHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOrSelfRequirement requirement)
        {
            if (context.User.IsInRole(StaticDetail.Role_Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var httpContext = _httpContextAccessor.HttpContext;
            string targetUserId = httpContext.Request.Query["userId"];

            if (userId == targetUserId)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
