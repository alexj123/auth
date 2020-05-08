using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AppUserAuthentication.Middleware.Authentication
{
    /// <summary>
    /// A middleware class which sets the JWToken in the header of a request.
    ///
    /// Sets the Authorization header field with type: Bearer.
    /// 
    /// Retrieves the token from the "bearer" cookie.
    /// </summary>
    public class JwtSetterMiddleware
    {
        private readonly RequestDelegate _next;
        private const string Bearer = "bearer";

        /// <summary>
        /// Constructs this middleware
        /// </summary>
        /// <param name="next">The next requestDelegate.</param>
        public JwtSetterMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Called when the middleware is invoked.
        ///
        /// Sets the authorization header with the token from the bearer cookie.
        /// </summary>
        /// <param name="context">the http context</param>
        /// <returns>the next invoke in the pipeline</returns>
        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Cookies[Bearer];
            
            if (token != null)
            {
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            }

            await _next.Invoke(context);
        }
    }
}