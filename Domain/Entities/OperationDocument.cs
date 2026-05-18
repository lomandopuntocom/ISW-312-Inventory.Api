namespace Inventory.Api.Domain.Entities;

public sealed class OperationDocument
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int CompanyId { get; set; }
    public Guid CompanyCen { get; set; }
    public int LocationId { get; set; }
    public Guid LocationCen { get; set; }
    public int WarehouseId { get; set; }
    public Guid WarehouseCen { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = "DRAFT";
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<OperationDocumentItem> Items { get; set; } = [];
}
