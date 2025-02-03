using System.Text;

namespace fintrack_api
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
            // Log basic request information
            _logger.LogInformation("Incoming request: {Method} {Path}", context.Request.Method, context.Request.Path);

            // Optional: Log request headers
            foreach (var header in context.Request.Headers)
            {
                _logger.LogInformation("Header: {Key}: {Value}", header.Key, header.Value);
            }

            // Optional: If you need to log the request body, enable buffering first.
            // Note: Be cautious when logging the body as it may contain sensitive data.
            if (context.Request.ContentLength > 0 &&
                context.Request.Body.CanRead)
            {
                // Allow the stream to be read multiple times
                context.Request.EnableBuffering();

                // Read the stream
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
                {
                    string body = await reader.ReadToEndAsync();
                    _logger.LogInformation("Request Body: {Body}", body);

                    // Reset the stream position so the next middleware can read it
                    context.Request.Body.Position = 0;
                }
            }

            // Call the next middleware in the pipeline
            await _next(context);

            // Optionally, log after the response is sent
            _logger.LogInformation("Finished handling request.");
        }
    }
}
