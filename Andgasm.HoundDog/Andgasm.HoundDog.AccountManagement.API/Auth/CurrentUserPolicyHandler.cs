using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

public class CurrentUserPolicyHandler : AuthorizationHandler<CurrentUserRequirement>
{
    private readonly IHttpContextAccessor _httpContext;

    public CurrentUserPolicyHandler(IHttpContextAccessor httpcontext)
    {
        _httpContext = httpcontext;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   CurrentUserRequirement requirement)
    {
        if (!context.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier)) return Task.CompletedTask;
        var currUserId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;

        _httpContext.HttpContext.Request.RouteValues.TryGetValue("userid", out var requestuseridobj);
        if (requestuseridobj == null) return Task.CompletedTask;
        
        var requestuserid = requestuseridobj as string;
        if (requestuseridobj == null) return Task.CompletedTask;

        if (currUserId != (string)requestuserid) return Task.CompletedTask;
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public class CurrentUserRequirement : IAuthorizationRequirement { }

