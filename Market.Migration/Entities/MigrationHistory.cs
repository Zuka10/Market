namespace Market.Migration.Entities;

public class MigrationHistory
{
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public string CheckSum { get; set; } = string.Empty;
}