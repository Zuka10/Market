namespace Market.Domain.Filters;

public class UserFilterParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public long? RoleId { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "Id";
    public string? SortDirection { get; set; } = "asc";
}