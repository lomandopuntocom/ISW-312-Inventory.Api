namespace Inventory.Api.Domain.Entities;

public sealed class UnitMeasure
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
