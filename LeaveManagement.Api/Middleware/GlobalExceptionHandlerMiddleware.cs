using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using LeaveManagement.Application.Exceptions;
using LeaveManagement.Application.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LeaveManagement.Api.Middleware;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred during the request.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var statusCode = (int)HttpStatusCode.InternalServerError;
        var code = "INTERNAL_SERVER_ERROR";
        var message = "An internal server error occurred.";
        List<ValidationErrorDetail>? details = null;

        if (exception is ApiException apiException)
        {
            statusCode = apiException.StatusCode;
            code = apiException.Code;
            message = apiException.Message;

            if (apiException is ValidationException validationException)
            {
                details = validationException.Errors.Select(e => new ValidationErrorDetail { Field = e.Field, Message = e.Message }).ToList();
            }
        }
        else if (env.IsDevelopment())
        {
            message = exception.Message;
        }

        context.Response.StatusCode = statusCode;

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Error = new ApiError { Code = code, Message = message, Details = details }
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
