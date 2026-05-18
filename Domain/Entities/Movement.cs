namespace Inventory.Api.Domain.Entities;

public sealed class Movement
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int CompanyId { get; set; }
    public Guid CompanyCen { get; set; }
    public int LocationId { get; set; }
    public Guid LocationCen { get; set; }
    public int WarehouseId { get; set; }
    public Guid WarehouseCen { get; set; }
    public int ProductId { get; set; }
    public Guid ProductCen { get; set; }
    public int MovementTypeId { get; set; }
    public Guid MovementTypeCen { get; set; }
    public decimal Quantity { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
