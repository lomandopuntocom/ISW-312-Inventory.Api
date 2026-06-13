using Inventory.Api.Application.Dtos;
using Inventory.Api.Domain.Entities;
using Inventory.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/inventory/companies/{companyCen}/stock")]
public sealed class StockController(
    InventoryDbContext db,
    Inventory.Api.Infrastructure.RestockEventBroadcaster broadcaster
) : InventoryControllerBase(db)
{
    [HttpGet]
    public async Task<IActionResult> GetStock(string companyCen, [FromQuery] string? productCen, [FromQuery] string? warehouseCen)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var query = Db.Stock.Where(x => x.CompanyCen == company.Cen);
        if (TryParseCen(productCen ?? string.Empty, out var product)) query = query.Where(x => x.ProductCen == product);
        if (TryParseCen(warehouseCen ?? string.Empty, out var warehouse)) query = query.Where(x => x.WarehouseCen == warehouse);

        var items = await query
            .Join(Db.Products, s => s.ProductCen, p => p.Cen, (s, p) => new StockDto(
                s.ProductCen.ToString(),
                p.Code,
                s.WarehouseCen.ToString(),
                s.Quantity,
                s.MinQuantity,
                s.Quantity <= s.MinQuantity))
            .ToListAsync();

        var stockedCens = items.Select(x => x.ProductCen).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = await Db.Products
            .Where(x => x.CompanyCen == company.Cen && x.TrackStock && !stockedCens.Contains(x.Cen.ToString()))
            .Select(x => new StockDto(x.Cen.ToString(), x.Code, null, 0, 0, true))
            .ToListAsync();

        return Ok(items.Concat(missing).OrderBy(x => x.ProductCode).ToList());
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate(string companyCen, StockValidationRequest request)
    {
        var stock = await FindStockAsync(companyCen, request.ProductCen, request.WarehouseCen);
        return Ok(new { available = stock is not null && stock.Quantity >= request.Quantity, quantity = stock?.Quantity ?? 0 });
    }

    [HttpPost("consume")]
    public Task<IActionResult> Consume(string companyCen, StockMutationRequest request)
    {
        return Mutate(companyCen, request, -Math.Abs(request.Quantity), "SALE", "OUT");
    }

    [HttpPost("increase")]
    public Task<IActionResult> Increase(string companyCen, StockMutationRequest request)
    {
        return Mutate(companyCen, request, Math.Abs(request.Quantity), "PURCHASE", "IN");
    }

    [HttpPost("adjustments")]
    public async Task<IActionResult> Adjust(string companyCen, StockAdjustmentRequest request)
    {
        if (request.Quantity == 0 || request.Quantity != Math.Floor(request.Quantity))
            return BadRequest("Quantity must be a non-zero whole number.");

        var delta = request.Quantity;
        var mutation = new StockMutationRequest(request.ProductCen, request.WarehouseCen, Math.Abs(delta), "ADJUSTMENT", request.Reason);
        return await Mutate(companyCen, mutation, delta, "ADJUSTMENT", delta >= 0 ? "IN" : "OUT");
    }

    private async Task<IActionResult> Mutate(string companyCen, StockMutationRequest request, decimal delta, string reference, string direction)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null || !TryParseCen(request.ProductCen, out var productCen)) return NotFound();

        var stock = await FindStockAsync(companyCen, request.ProductCen, request.WarehouseCen);
        stock ??= await CreateStockRowAsync(company, productCen, request.WarehouseCen);
        if (stock is null) return NotFound("Product, location, or warehouse not found for stock operation.");
        if (stock.Quantity + delta < 0) return Conflict("Insufficient stock.");

        var before = stock.Quantity;
        stock.Quantity += delta;
        stock.UpdatedAt = DateTime.UtcNow;

        var movementType = await Db.MovementTypes.FirstOrDefaultAsync(x => x.Code == reference);
        if (movementType is null)
        {
            movementType = new MovementType
            {
                Code = reference,
                Name = reference,
                MovementDirection = direction,
                Active = true
            };
            Db.MovementTypes.Add(movementType);
            await Db.SaveChangesAsync();
        }

        Db.Movements.Add(new Movement
        {
            CompanyId = company.Id,
            CompanyCen = company.Cen,
            LocationId = stock.LocationId,
            LocationCen = stock.LocationCen,
            WarehouseId = stock.WarehouseId,
            WarehouseCen = stock.WarehouseCen,
            ProductId = stock.ProductId,
            ProductCen = productCen,
            MovementTypeId = movementType.Id,
            MovementTypeCen = movementType.Cen,
            Quantity = Math.Abs(delta),
            BalanceBefore = before,
            BalanceAfter = stock.Quantity,
            Reference = request.Reference ?? reference,
            Notes = request.Notes ?? direction
        });

        await Db.SaveChangesAsync();
        var product = await Db.Products.FirstOrDefaultAsync(x => x.Cen == stock.ProductCen);

        if (delta > 0)
        {
            broadcaster.Broadcast(new Inventory.Api.Domain.Entities.RestockEvent(
                stock.CompanyCen.ToString(),
                stock.ProductCen.ToString(),
                product?.Code ?? stock.ProductCen.ToString(),
                product?.Name ?? "Producto",
                Math.Abs(delta),
                stock.Quantity,
                stock.WarehouseCen.ToString(),
                DateTime.UtcNow
            ));
        }

        return Ok(new StockDto(
            stock.ProductCen.ToString(),
            product?.Code ?? stock.ProductCen.ToString(),
            stock.WarehouseCen.ToString(),
            stock.Quantity,
            stock.MinQuantity,
            stock.Quantity <= stock.MinQuantity));
    }

    private async Task<StockItem?> FindStockAsync(string companyCen, string productCen, string? warehouseCen)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null || !TryParseCen(productCen, out var product)) return null;

        var query = Db.Stock.Where(x => x.CompanyCen == company.Cen && x.ProductCen == product);
        if (TryParseCen(warehouseCen ?? string.Empty, out var warehouse)) query = query.Where(x => x.WarehouseCen == warehouse);
        return await query.FirstOrDefaultAsync();
    }

    private async Task<StockItem?> CreateStockRowAsync(Company company, Guid productCen, string? warehouseCen)
    {
        var product = await Db.Products.FirstOrDefaultAsync(x => x.CompanyCen == company.Cen && x.Cen == productCen);
        if (product is null) return null;

        Warehouse warehouse;
        if (TryParseCen(warehouseCen ?? string.Empty, out var requestedWarehouse))
        {
            var selected = await Db.Warehouses.FirstOrDefaultAsync(x => x.CompanyCen == company.Cen && x.Cen == requestedWarehouse);
            warehouse = selected ?? await EnsureDefaultWarehouseAsync(company);
        }
        else
        {
            warehouse = await EnsureDefaultWarehouseAsync(company);
        }

        return await EnsureStockRowAsync(company, product, warehouse);
    }
}
