namespace Market.Application.Common.Models;

public class BaseResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = [];

    public static BaseResponse<T> Success(T data, string message = "Operation successful")
    {
        return new BaseResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    public static BaseResponse<T> Failure(List<string> errors)
    {
        return new BaseResponse<T>
        {
            IsSuccess = false,
            Message = "Operation failed",
            Errors = errors
        };
    }
}