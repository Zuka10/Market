namespace Market.Application.DTOs.Validation;

public class ValidationErrorDto
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? AttemptedValue { get; set; }
}