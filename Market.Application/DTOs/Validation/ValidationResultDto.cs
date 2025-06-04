namespace Market.Application.DTOs.Validation;

public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public List<ValidationErrorDto> Errors { get; set; } = new();

    public static ValidationResultDto Success()
    {
        return new ValidationResultDto { IsValid = true };
    }

    public static ValidationResultDto Failure(List<ValidationErrorDto> errors)
    {
        return new ValidationResultDto { IsValid = false, Errors = errors };
    }

    public static ValidationResultDto Failure(string field, string message)
    {
        return new ValidationResultDto
        {
            IsValid = false,
            Errors = [new ValidationErrorDto { Field = field, Message = message }]
        };
    }
}