namespace Market.Migration.Entities;

public record MigrationInfo(string Version, string Description, DateTime AppliedAt, bool IsApplied);