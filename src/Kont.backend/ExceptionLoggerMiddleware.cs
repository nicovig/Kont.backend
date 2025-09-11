using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Kont.backend;

public class ExceptionLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private ILogger<ExceptionLoggerMiddleware> _logger;

    public ExceptionLoggerMiddleware(ILogger<ExceptionLoggerMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var originalStream = httpContext.Response.Body;
        try
        {
            using var captureStream = new MemoryStream();
            httpContext.Response.Body = captureStream;

            var request = await Log(httpContext.Request);

            await _next(httpContext);
            if (httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest)
            {
                _logger.LogWarning(request);
                await Log(captureStream);
            }

            captureStream.Seek(0, SeekOrigin.Begin);
            await captureStream.CopyToAsync(originalStream);

        }
        catch (Exception ex)
        {
            var current = ex;
            int i = 0;
            while (current != null && i < 5)
            {
                _logger.LogError(current, current.Message);
                current = current.InnerException;
                i++;
            }

            throw;
        }
        finally
        {
            httpContext.Response.Body = originalStream;
        }
    }

    private async Task Log(MemoryStream stream)
    {

        stream.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(stream).ReadToEndAsync();

        if (text != string.Empty)
        {
            _logger.LogWarning("Response: " + text);
        }
    }

    private async Task<string> Log(HttpRequest request)
    {
        if (request.ContentLength != null && request.ContentLength > 0L)
        {
            request.EnableBuffering();

            using (var reader = new StreamReader(request.Body,
                            encoding: Encoding.UTF8,
                            detectEncodingFromByteOrderMarks: false,
                            leaveOpen: true))
            {
                var text = await reader.ReadToEndAsync();

                // Reset the request body stream position so the next middleware can read it
                request.Body.Position = 0;

                return text;
            }
        }
        else
        {
            return request.QueryString.Value ?? "no-data-provided";
        }
    }
}

[ExcludeFromCodeCoverage]
public static class ExceptionLoggerMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionLoggerMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionLoggerMiddleware>();
    }
}