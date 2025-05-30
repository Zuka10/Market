namespace Market.Domain.Filters;

public class LocationFilterParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "Name";
    public string? SortDirection { get; set; } = "asc";
}