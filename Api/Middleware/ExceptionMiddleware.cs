using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;
        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment environment)
        {
            _environment = environment;
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                // TODO: StatusText ??

                var response = _environment.IsDevelopment()
                    ? new ApiException(
                        context.Response.StatusCode,
                        ex.Message,
                        ex.StackTrace?.ToString())
                    : new ApiException(
                        context.Response.StatusCode,
                        "Internal Server Error"
                        );

                var options = new JsonSerializerOptions
                { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}