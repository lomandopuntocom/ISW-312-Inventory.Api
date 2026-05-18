using Inventory.Api.Domain.Entities;
using Inventory.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

public abstract class InventoryControllerBase(InventoryDbContext db) : ControllerBase
{
    protected InventoryDbContext Db { get; } = db;

    protected static bool TryParseCen(string value, out Guid cen) => Guid.TryParse(value, out cen);

    protected async Task<Company?> FindCompanyAsync(string companyCen)
    {
        return TryParseCen(companyCen, out var cen)
            ? await Db.Companies.FirstOrDefaultAsync(x => x.Cen == cen && x.Active)
            : null;
    }

    protected async Task<string> GenerateNextCodeAsync(Guid companyCen, string prefix, Func<Task<int>> countAsync)
    {
        var count = await countAsync();
        return $"{prefix}-{(count + 1):D5}";
    }

    protected async Task<Warehouse> EnsureDefaultWarehouseAsync(Company company)
    {
        var warehouse = await Db.Warehouses.FirstOrDefaultAsync(x => x.CompanyCen == company.Cen && x.Active);
        if (warehouse is not null) return warehouse;

        var locationCount = await Db.Locations.CountAsync(x => x.CompanyCen == company.Cen);
        var location = new Location
        {
            CompanyId = company.Id,
            CompanyCen = company.Cen,
            Code = $"LOC-{(locationCount + 1):D5}",
            Name = "Principal",
            Active = true
        };
        Db.Locations.Add(location);
        await Db.SaveChangesAsync();

        warehouse = new Warehouse
        {
            CompanyId = company.Id,
            CompanyCen = company.Cen,
            LocationId = location.Id,
            LocationCen = location.Cen,
            Code = "ALM-00001",
            Name = "Almacén principal",
            Active = true
        };
        Db.Warehouses.Add(warehouse);
        await Db.SaveChangesAsync();
        return warehouse;
    }

    protected async Task<StockItem> EnsureStockRowAsync(Company company, Product product, Warehouse? warehouse = null)
    {
        warehouse ??= await EnsureDefaultWarehouseAsync(company);

        var stock = await Db.Stock.FirstOrDefaultAsync(x =>
            x.CompanyCen == company.Cen && x.ProductCen == product.Cen && x.WarehouseCen == warehouse.Cen);

        if (stock is not null) return stock;

        stock = new StockItem
        {
            CompanyId = company.Id,
            CompanyCen = company.Cen,
            LocationId = warehouse.LocationId,
            LocationCen = warehouse.LocationCen,
            WarehouseId = warehouse.Id,
            WarehouseCen = warehouse.Cen,
            ProductId = product.Id,
            ProductCen = product.Cen,
            Quantity = 0,
            MinQuantity = 0
        };

        Db.Stock.Add(stock);
        await Db.SaveChangesAsync();
        return stock;
    }
}
