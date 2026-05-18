using Inventory.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<UnitMeasure> UnitsMeasure => Set<UnitMeasure>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockItem> Stock => Set<StockItem>();
    public DbSet<MovementType> MovementTypes => Set<MovementType>();
    public DbSet<Movement> Movements => Set<Movement>();
    public DbSet<OperationDocument> OperationDocuments => Set<OperationDocument>();
    public DbSet<OperationDocumentItem> OperationDocumentItems => Set<OperationDocumentItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Nit).HasColumnName("nit");
            entity.Property(x => x.Phone).HasColumnName("phone");
            entity.Property(x => x.Email).HasColumnName("email");
            entity.Property(x => x.Address).HasColumnName("address");
            entity.Property(x => x.City).HasColumnName("city");
            entity.Property(x => x.Country).HasColumnName("country");
            entity.Property(x => x.Active).HasColumnName("active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.HasIndex(x => new { x.CompanyCen, x.Code }).IsUnique();
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.CompanyCen).HasColumnName("company_cen");
            entity.Property(x => x.Code).HasColumnName("code");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.Active).HasColumnName("active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<UnitMeasure>(entity =>
        {
            entity.ToTable("units_measure");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.Code).HasColumnName("code");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Abbreviation).HasColumnName("abbreviation");
            entity.Property(x => x.Active).HasColumnName("active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.HasIndex(x => new { x.CompanyCen, x.Code }).IsUnique();
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.CompanyCen).HasColumnName("company_cen");
            entity.Property(x => x.Code).HasColumnName("code");
            entity.Property(x => x.Sku).HasColumnName("sku");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.CategoryId).HasColumnName("category_id");
            entity.Property(x => x.CategoryCen).HasColumnName("category_cen");
            entity.Property(x => x.UnitMeasureId).HasColumnName("unit_measure_id");
            entity.Property(x => x.UnitMeasureCen).HasColumnName("unit_measure_cen");
            entity.Property(x => x.Price).HasColumnName("price").HasPrecision(12, 2);
            entity.Property(x => x.Cost).HasColumnName("cost").HasPrecision(12, 2);
            entity.Property(x => x.TrackStock).HasColumnName("track_stock");
            entity.Property(x => x.IsOutOfStock).HasColumnName("is_out_of_stock");
            entity.Property(x => x.Active).HasColumnName("active");
            entity.Property(x => x.StationCode).HasColumnName("station_code");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("locations");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.CompanyCen).HasColumnName("company_cen");
            entity.Property(x => x.Code).HasColumnName("code");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Address).HasColumnName("address");
            entity.Property(x => x.Phone).HasColumnName("phone");
            entity.Property(x => x.Active).HasColumnName("active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("warehouses");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.CompanyCen).HasColumnName("company_cen");
            entity.Property(x => x.LocationId).HasColumnName("location_id");
            entity.Property(x => x.LocationCen).HasColumnName("location_cen");
            entity.Property(x => x.Code).HasColumnName("code");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.Active).HasColumnName("active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.ToTable("stock");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.CompanyCen).HasColumnName("company_cen");
            entity.Property(x => x.LocationId).HasColumnName("location_id");
            entity.Property(x => x.LocationCen).HasColumnName("location_cen");
            entity.Property(x => x.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(x => x.WarehouseCen).HasColumnName("warehouse_cen");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.ProductCen).HasColumnName("product_cen");
            entity.Property(x => x.Quantity).HasColumnName("quantity").HasPrecision(12, 2);
            entity.Property(x => x.MinQuantity).HasColumnName("min_quantity").HasPrecision(12, 2);
            entity.Property(x => x.MaxQuantity).HasColumnName("max_quantity").HasPrecision(12, 2);
            entity.Property(x => x.LastCountedAt).HasColumnName("last_counted_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<MovementType>(entity =>
        {
            entity.ToTable("movement_types");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.Code).HasColumnName("code");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Description).HasColumnName("Description");
            entity.Property(x => x.MovementDirection).HasColumnName("movement_direction");
            entity.Property(x => x.Active).HasColumnName("active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Movement>(entity =>
        {
            entity.ToTable("movements");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.CompanyCen).HasColumnName("company_cen");
            entity.Property(x => x.LocationId).HasColumnName("location_id");
            entity.Property(x => x.LocationCen).HasColumnName("location_cen");
            entity.Property(x => x.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(x => x.WarehouseCen).HasColumnName("warehouse_cen");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.ProductCen).HasColumnName("product_cen");
            entity.Property(x => x.MovementTypeId).HasColumnName("movement_type_id");
            entity.Property(x => x.MovementTypeCen).HasColumnName("movement_type_cen");
            entity.Property(x => x.Quantity).HasColumnName("quantity").HasPrecision(12, 2);
            entity.Property(x => x.BalanceBefore).HasColumnName("balance_before").HasPrecision(12, 2);
            entity.Property(x => x.BalanceAfter).HasColumnName("balance_after").HasPrecision(12, 2);
            entity.Property(x => x.Reference).HasColumnName("reference");
            entity.Property(x => x.Notes).HasColumnName("notes");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<OperationDocument>(entity =>
        {
            entity.ToTable("operation_documents");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.CompanyCen).HasColumnName("company_cen");
            entity.Property(x => x.LocationId).HasColumnName("location_id");
            entity.Property(x => x.LocationCen).HasColumnName("location_cen");
            entity.Property(x => x.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(x => x.WarehouseCen).HasColumnName("warehouse_cen");
            entity.Property(x => x.DocumentNumber).HasColumnName("document_number");
            entity.Property(x => x.OperationType).HasColumnName("operation_type");
            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.Reference).HasColumnName("reference");
            entity.Property(x => x.Notes).HasColumnName("notes");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.ConfirmedAt).HasColumnName("confirmed_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.DocumentId);
        });

        modelBuilder.Entity<OperationDocumentItem>(entity =>
        {
            entity.ToTable("operation_document_items");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.DocumentId).HasColumnName("document_id");
            entity.Property(x => x.DocumentCen).HasColumnName("document_cen");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.ProductCen).HasColumnName("product_cen");
            entity.Property(x => x.Quantity).HasColumnName("quantity").HasPrecision(12, 2);
            entity.Property(x => x.Notes).HasColumnName("notes");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });
    }
}
