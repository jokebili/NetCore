using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace nc.Nexts
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class Auth
    {
        private readonly RequestDelegate _next;

        public Auth(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var tk = httpContext.Request.Headers["token"];
            if (tk == "abc")
            {
                return _next(httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                httpContext.Response.WriteAsync("");
                return null;
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class AuthExtensions
    {
        public static IApplicationBuilder UseAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Auth>();
        }
    }
}
