using Back_EndAPI.Services;


namespace Back_EndAPI.Middleware
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IdempotencyMiddleware> _logger;

        public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IIdempotencyService idempotencyService)
        {
            // Check for Idempotency-Key header on POST requests
            if (context.Request.Method == "POST" &&
                context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
            {
                var (isIdempotent, cachedResponse) = await idempotencyService.CheckIdempotencyAsync(
                    idempotencyKey.ToString());

                if (isIdempotent && cachedResponse != null)
                {
                    _logger.LogInformation($"Returning cached response for: {idempotencyKey}");
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(cachedResponse);
                    return;
                }

                // Capture response
                var originalBodyStream = context.Response.Body;
                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    await _next(context);

                    var body = await GetResponseBodyAsync(context.Response);

                    // Cache successful responses (2xx)
                    if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                    {
                        await idempotencyService.CacheResponseAsync(idempotencyKey.ToString(), body);
                    }

                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            else
            {
                await _next(context);
            }
        }
        private async Task<string> GetResponseBodyAsync(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(response.Body);
            var body = await reader.ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return body;
        }
    }

}

