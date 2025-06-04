namespace Market.Application.DTOs.Market;

public class ProcurementDto
{
    public long Id { get; set; }
    public long VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public long LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public DateTime ProcurementDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string UpdatedByName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public int LineItemCount { get; set; }
    public decimal CalculatedTotal { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public List<ProcurementDetailDto> ProcurementDetails { get; set; } = [];
}