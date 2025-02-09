namespace GroupAllocator.WebAPI.Middleware.TokenAccess
{
    public static class TokenAccessMiddlewareExtension
    {
        public static IApplicationBuilder UseAccessTokenMiddleware(this IApplicationBuilder app, string accessToken)
        {
            app.UseMiddleware<TokenAccessMiddleware>(accessToken);
            return app;
        }
    }
}
