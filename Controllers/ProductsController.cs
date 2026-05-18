using Inventory.Api.Application.Dtos;
using Inventory.Api.Domain.Entities;
using Inventory.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/inventory/companies/{companyCen}")]
public sealed class ProductsController(InventoryDbContext db) : InventoryControllerBase(db)
{
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        string companyCen,
        [FromQuery] string? search,
        [FromQuery] string? categoryCen,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var query = Db.Products.Where(x => x.CompanyCen == company.Cen);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.ToLower().Contains(search.ToLower()) || x.Sku.ToLower().Contains(search.ToLower()));

        if (TryParseCen(categoryCen ?? string.Empty, out var category))
            query = query.Where(x => x.CategoryCen == category);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
            .Take(Math.Max(pageSize, 1))
            .Select(x => ToDto(x))
            .ToListAsync();

        return Ok(new PagedResultDto<ProductDto>(items, total, page, pageSize));
    }

    [HttpGet("sellable-products")]
    public async Task<IActionResult> GetSellableProducts(
        string companyCen,
        [FromQuery] string? search,
        [FromQuery] string? categoryCen,
        [FromQuery] bool onlyAvailable = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var query = Db.Products.Where(x => x.CompanyCen == company.Cen && x.Active);
        if (onlyAvailable) query = query.Where(x => !x.IsOutOfStock);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.ToLower().Contains(search.ToLower()));
        if (TryParseCen(categoryCen ?? string.Empty, out var category)) query = query.Where(x => x.CategoryCen == category);

        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.Name)
            .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
            .Take(Math.Max(pageSize, 1))
            .Select(x => ToDto(x))
            .ToListAsync();

        return Ok(new PagedResultDto<ProductDto>(items, total, page, pageSize));
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct(string companyCen, UpsertProductRequest request)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var productCode = string.IsNullOrWhiteSpace(request.Code)
            ? await GenerateNextCodeAsync(company.Cen, "PRO", () => Db.Products.CountAsync(x => x.CompanyCen == company.Cen))
            : request.Code.Trim();

        var product = new Product
        {
            CompanyId = company.Id,
            CompanyCen = company.Cen,
            Code = productCode,
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            CategoryCen = TryParseCen(request.CategoryCen ?? string.Empty, out var categoryCenValue) ? categoryCenValue : null,
            UnitMeasureCen = TryParseCen(request.UnitCen ?? string.Empty, out var unitCenValue) ? unitCenValue : null,
            Price = request.Price,
            Cost = request.Cost,
            TrackStock = request.TrackStock,
            StationCode = request.StationCode
        };

        Db.Products.Add(product);
        await Db.SaveChangesAsync();

        if (product.TrackStock)
            await EnsureInitialStockAsync(company, product);

        return Ok(ToDto(product));
    }

    [HttpPost("products/lookup")]
    public async Task<IActionResult> LookupProducts(string companyCen, ProductLookupRequest request)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var cens = request.ProductCens
            .Select(x => TryParseCen(x, out var cen) ? cen : Guid.Empty)
            .Where(x => x != Guid.Empty)
            .ToList();

        var products = await Db.Products
            .Where(x => x.CompanyCen == company.Cen && cens.Contains(x.Cen))
            .Select(x => ToDto(x))
            .ToListAsync();

        return Ok(products);
    }

    [HttpPut("products/{productCen}")]
    public async Task<IActionResult> UpdateProduct(string companyCen, string productCen, UpsertProductRequest request)
    {
        var product = await FindProductAsync(companyCen, productCen);
        if (product is null) return NotFound();

        product.Code = request.Code;
        product.Sku = request.Sku;
        product.Name = request.Name;
        product.Description = request.Description;
        product.CategoryCen = TryParseCen(request.CategoryCen ?? string.Empty, out var categoryCenValue) ? categoryCenValue : null;
        product.UnitMeasureCen = TryParseCen(request.UnitCen ?? string.Empty, out var unitCenValue) ? unitCenValue : null;
        product.Price = request.Price;
        product.Cost = request.Cost;
        product.TrackStock = request.TrackStock;
        product.StationCode = request.StationCode;
        product.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync();

        return Ok(ToDto(product));
    }

    [HttpPatch("products/{productCen}/status")]
    public async Task<IActionResult> UpdateStatus(string companyCen, string productCen, ProductStatusRequest request)
    {
        var product = await FindProductAsync(companyCen, productCen);
        if (product is null) return NotFound();

        product.Active = request.Active;
        product.IsOutOfStock = request.IsOutOfStock;
        product.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync();

        return Ok(ToDto(product));
    }

    [HttpGet("products/{productCen}/kardex")]
    public async Task<IActionResult> Kardex(string companyCen, string productCen)
    {
        var product = await FindProductAsync(companyCen, productCen);
        if (product is null) return NotFound();

        var movements = await Db.Movements
            .Where(x => x.CompanyCen == product.CompanyCen && x.ProductCen == product.Cen)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new KardexMovementDto(x.Cen.ToString(), x.ProductCen.ToString(), x.Quantity, x.BalanceBefore, x.BalanceAfter, x.Reference, x.Notes, x.CreatedAt))
            .ToListAsync();

        return Ok(movements);
    }

    private async Task<Product?> FindProductAsync(string companyCen, string productCen)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null || !TryParseCen(productCen, out var productCenValue)) return null;
        return await Db.Products.FirstOrDefaultAsync(x => x.CompanyCen == company.Cen && x.Cen == productCenValue);
    }

    private async Task EnsureInitialStockAsync(Company company, Product product)
    {
        if (!product.TrackStock) return;
        var warehouse = await EnsureDefaultWarehouseAsync(company);
        await EnsureStockRowAsync(company, product, warehouse);
    }

    private static ProductDto ToDto(Product x)
    {
        return new ProductDto(
            x.Cen.ToString(),
            x.Code,
            x.Sku,
            x.Name,
            x.Description,
            x.CategoryCen?.ToString(),
            x.UnitMeasureCen?.ToString(),
            x.Price,
            x.Cost,
            x.TrackStock,
            x.IsOutOfStock,
            x.Active,
            x.StationCode);
    }
}
