namespace GroupAllocator.WebAPI.Middleware.TokenAccess
{
    public class TokenAccessMiddleware
    {
        private readonly string _accessToken;

        private readonly RequestDelegate _next;


        public TokenAccessMiddleware(RequestDelegate next, string accessToken)
        {
            this._next = next;
            this._accessToken = accessToken;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Query["token"];

            if (string.IsNullOrWhiteSpace(token) || _accessToken != token)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { message = "Нет доступа" });
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
