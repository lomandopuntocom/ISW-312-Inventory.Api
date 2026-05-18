namespace Inventory.Api.Domain.Entities;

public sealed class Product
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int CompanyId { get; set; }
    public Guid CompanyCen { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public Guid? CategoryCen { get; set; }
    public int? UnitMeasureId { get; set; }
    public Guid? UnitMeasureCen { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public bool TrackStock { get; set; } = true;
    public bool IsOutOfStock { get; set; }
    public bool Active { get; set; } = true;
    public string? StationCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
