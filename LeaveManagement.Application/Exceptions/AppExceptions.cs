using System;
using System.Collections.Generic;

namespace LeaveManagement.Application.Exceptions;

public class ApiException(string message, string code = "INTERNAL_SERVER_ERROR", int statusCode = 500) : Exception(message)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;
}

public class NotFoundException(string message) : ApiException(message, "NOT_FOUND", 404);

public class BadRequestException(string message) : ApiException(message, "BAD_REQUEST", 400);

public class UnauthorizedException(string message) : ApiException(message, "UNAUTHORIZED", 401);

public class ForbiddenException(string message) : ApiException(message, "FORBIDDEN", 403);

public class InternalServerException(string message) : ApiException(message, "INTERNAL_SERVER_ERROR", 500);

public class ValidationException(string message, List<FieldValidationError> errors) : ApiException(message, "VALIDATION_ERROR", 422)
{
    public List<FieldValidationError> Errors { get; } = errors;
}

public record FieldValidationError(string Field, string Message);
