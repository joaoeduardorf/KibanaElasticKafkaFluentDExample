using Serilog;
using System.Text;

namespace WebApplicationExample.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Log Request
            var requestBody = await ReadStreamAsync(context.Request.Body);
            Log.Information("Request: {@Request}", new
            {
                Method = context.Request.Method,
                Path = context.Request.Path,
                Query = context.Request.QueryString.ToString(),
                Body = requestBody
            });

            // Capture Response
            var originalResponseBody = context.Response.Body;
            using var newResponseBody = new MemoryStream();
            context.Response.Body = newResponseBody;

            await _next(context);

            // Log Response
            newResponseBody.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(newResponseBody).ReadToEndAsync();
            Log.Information("Response: {@Response}", new
            {
                StatusCode = context.Response.StatusCode,
                Body = responseBody
            });

            newResponseBody.Seek(0, SeekOrigin.Begin);
            await newResponseBody.CopyToAsync(originalResponseBody);
        }

        private async Task<string> ReadStreamAsync(Stream stream)
        {
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var text = await reader.ReadToEndAsync();
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
            return text;
        }
    }
}
