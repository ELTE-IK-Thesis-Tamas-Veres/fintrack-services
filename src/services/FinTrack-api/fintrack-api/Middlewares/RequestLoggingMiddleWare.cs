using System.Text;

namespace fintrack_api.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation("Incoming request: {Method} {Path}", context.Request.Method, context.Request.Path);

            // Optional: Log request headers
            foreach (var header in context.Request.Headers)
            {
                _logger.LogInformation("Header: {Key}: {Value}", header.Key, header.Value);
            }

            if (context.Request.ContentLength > 0 &&
                context.Request.Body.CanRead)
            {
                context.Request.EnableBuffering();

                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
                {
                    string body = await reader.ReadToEndAsync();
                    _logger.LogInformation("Request Body: {Body}", body);

                    context.Request.Body.Position = 0;
                }
            }

            await _next(context);

            _logger.LogInformation("Finished handling request.");
        }
    }
}
