using Serilog;
using System.Diagnostics;
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

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Capturar o stream original da resposta
            var originalResponseBodyStream = context.Response.Body;

            try
            {
                // Substituir o response body com um MemoryStream
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Capturar o corpo da requisição
                var requestBodyContent = await SafeReadRequestBody(context.Request);

                // Executar o restante do pipeline
                await _next(context);

                // Copiar o conteúdo da resposta para o stream original
                await SafeCopyResponseBody(responseBody, originalResponseBodyStream);

                // Capturar o corpo da resposta
                var responseBodyContent = await SafeReadResponseBody(responseBody);

                // Log detalhado da requisição e resposta
                stopwatch.Stop();
                Log.Information("Request-Response Log: {@LogDetails}", new
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Information",
                    MessageTemplate = "Request-Response",
                    TraceId = context.TraceIdentifier,
                    Properties = new
                    {
                        Request = new
                        {
                            Method = context.Request.Method,
                            Path = context.Request.Path,
                            QueryString = context.Request.QueryString.ToString(),
                            Headers = context.Request.Headers,
                            Body = requestBodyContent
                        },
                        Response = new
                        {
                            StatusCode = context.Response.StatusCode,
                            Headers = context.Response.Headers,
                            Body = responseBodyContent
                        },
                        ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao processar a requisição.");
                throw;
            }
            finally
            {
                // Restaurar o stream original, mesmo em caso de falha
                context.Response.Body = originalResponseBodyStream;
            }
        }

        private async Task<string> SafeReadRequestBody(HttpRequest request)
        {
            try
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
                return body;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Erro ao ler o corpo da requisição.");
                return string.Empty;
            }
        }

        private async Task<string> SafeReadResponseBody(MemoryStream responseBody)
        {
            try
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(responseBody);
                var body = await reader.ReadToEndAsync();
                return body;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Erro ao ler o corpo da resposta.");
                return string.Empty;
            }
        }

        private async Task SafeCopyResponseBody(MemoryStream source, Stream destination)
        {
            try
            {
                source.Seek(0, SeekOrigin.Begin);
                await source.CopyToAsync(destination);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Erro ao copiar o corpo da resposta.");
            }
        }
    }

    //public class RequestResponseLoggingMiddleware
    //{
    //    private readonly RequestDelegate _next;

    //    public RequestResponseLoggingMiddleware(RequestDelegate next)
    //    {
    //        _next = next;
    //    }

    //    public async Task InvokeAsync(HttpContext context)
    //    {
    //        // Somente registra um log resumido no final da requisição
    //        await _next(context);

    //        var logDetails = new
    //        {
    //            Timestamp = DateTime.UtcNow,
    //            Level = "Information",
    //            TraceId = context.TraceIdentifier,
    //            Request = new
    //            {
    //                context.Request.Method,
    //                context.Request.Path,
    //                QueryString = context.Request.QueryString.ToString(),
    //                context.Response.StatusCode,
    //                ContentType = context.Response.ContentType
    //            }
    //        };

    //        // Log único para requisição e resposta
    //        Log.Information("Request-Response Log: {@LogDetails}", logDetails);
    //    }
    //}
}
