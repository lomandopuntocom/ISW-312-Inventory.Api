namespace Inventory.Api.Domain.Entities;

public sealed class MovementType
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MovementDirection { get; set; } = "IN";
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
