using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserService.Domain.Exceptions;

namespace UserService.Api.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            object response;
            HttpStatusCode statusCode;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    response = new
                    {
                        error = new
                        {
                            message = "Validation failed",
                            statusCode = (int)statusCode,
                            errors = validationException.Errors.Select(e => new
                            {
                                field = e.PropertyName,
                                message = e.ErrorMessage
                            })
                        }
                    };
                    _logger.LogWarning(exception, "Validation error occurred");
                    break;

                case UserNotFoundException userNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    response = new
                    {
                        error = new
                        {
                            message = exception.Message,
                            statusCode = (int)statusCode
                        }
                    };
                    _logger.LogWarning(exception, "User not found: {UserId}", userNotFoundException.UserId);
                    break;

                case DuplicateEmailException duplicateEmailException:
                    statusCode = HttpStatusCode.Conflict;
                    response = new
                    {
                        error = new
                        {
                            message = exception.Message,
                            statusCode = (int)statusCode
                        }
                    };
                    _logger.LogWarning(exception, "Duplicate email: {Email}", duplicateEmailException.Email);
                    break;

                case DomainException:
                    statusCode = HttpStatusCode.BadRequest;
                    response = new
                    {
                        error = new
                        {
                            message = exception.Message,
                            statusCode = (int)statusCode
                        }
                    };
                    _logger.LogWarning(exception, "Domain exception occurred");
                    break;

                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    response = new
                    {
                        error = new
                        {
                            message = exception.Message,
                            statusCode = (int)statusCode
                        }
                    };
                    _logger.LogWarning(exception, "Argument exception occurred");
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    response = new
                    {
                        error = new
                        {
                            message = "An unexpected error occurred.",
                            statusCode = (int)statusCode
                        }
                    };
                    _logger.LogError(exception, "Unhandled exception occurred");
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
