using Microsoft.AspNetCore.Mvc;
using LeaveManagement.Application.Responses;

namespace LeaveManagement.Api.Helpers;

public class ResponseFactory
{
    public IActionResult Success<T>(T data, string message = "Operation successful")
    {
        var response = ApiResponse<T>.SuccessResponse(data, message);
        return new OkObjectResult(response);
    }

    public IActionResult Created<T>(T data, string message = "Resource created successfully")
    {
        var response = ApiResponse<T>.SuccessResponse(data, message);
        return new CreatedResult(string.Empty, response);
    }

    public IActionResult Paginated<T>(IEnumerable<T> items, int totalItems, int page, int pageSize, string message = "Data retrieved successfully")
    {
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var response = new ApiResponse<PaginatedData<T>>
        {
            Success = true,
            Message = message,
            Data = new PaginatedData<T>
            {
                Items = items,
                Meta = new PaginationMeta
                {
                    TotalItems = totalItems,
                    ItemCount = items.Count(),
                    ItemsPerPage = pageSize,
                    TotalPages = totalPages,
                    CurrentPage = page
                }
            }
        };
        return new OkObjectResult(response);
    }

    public IActionResult Error(string code, string message, int statusCode = 500)
    {
        var response = ApiResponse<object>.FailureResponse(code, message);
        return new ObjectResult(response) { StatusCode = statusCode };
    }

    public IActionResult ValidationError(List<ValidationErrorDetail> details, string message = "Validation failed")
    {
        var response = ApiResponse<object>.FailureResponse("VALIDATION_ERROR", message, details);
        return new UnprocessableEntityObjectResult(response);
    }
}
