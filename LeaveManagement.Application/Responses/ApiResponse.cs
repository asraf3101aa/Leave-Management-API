using System;
using System.Collections.Generic;

namespace LeaveManagement.Application.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public ApiError? Error { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> FailureResponse(string code, string message, List<ValidationErrorDetail>? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = new ApiError { Code = code, Message = message, Details = details }
        };
    }
}

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<ValidationErrorDetail>? Details { get; set; }
}

public class ValidationErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PaginatedResponse<T> : ApiResponse<PaginatedData<T>>
{
}

public class PaginatedData<T>
{
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    public PaginationMeta Meta { get; set; } = new();
}

public class PaginationMeta
{
    public int TotalItems { get; set; }
    public int ItemCount { get; set; }
    public int ItemsPerPage { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}
