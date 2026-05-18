namespace Inventory.Api.Domain.Entities;

public sealed class Company
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Nit { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
