using Inventory.Api.Application.Dtos;
using Inventory.Api.Domain.Entities;
using Inventory.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/inventory/companies/{companyCen}")]
public sealed class CatalogController(InventoryDbContext db) : InventoryControllerBase(db)
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(string companyCen)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var products = Db.Products.Where(x => x.CompanyCen == company.Cen);
        var lowStockCount = await Db.Stock.CountAsync(x => x.CompanyCen == company.Cen && x.Quantity <= x.MinQuantity);

        return Ok(new InventoryDashboardDto(
            await products.CountAsync(),
            await products.CountAsync(x => x.Active),
            await products.CountAsync(x => x.IsOutOfStock),
            lowStockCount));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(string companyCen)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var items = await Db.Categories
            .Where(x => x.CompanyCen == company.Cen)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto(x.Cen.ToString(), x.Code, x.Name, x.Description, x.Active))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(string companyCen, UpsertCategoryRequest request)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var categoryCode = string.IsNullOrWhiteSpace(request.Code)
            ? await GenerateNextCodeAsync(company.Cen, "CAT", () => Db.Categories.CountAsync(x => x.CompanyCen == company.Cen))
            : request.Code.Trim();

        var category = new Category
        {
            CompanyId = company.Id,
            CompanyCen = company.Cen,
            Code = categoryCode,
            Name = request.Name,
            Description = request.Description,
            Active = request.Active
        };

        Db.Categories.Add(category);
        await Db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new { companyCen }, new CategoryDto(category.Cen.ToString(), category.Code, category.Name, category.Description, category.Active));
    }

    [HttpPut("categories/{categoryCen}")]
    public async Task<IActionResult> UpdateCategory(string companyCen, string categoryCen, UpsertCategoryRequest request)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null || !TryParseCen(categoryCen, out var cen)) return NotFound();

        var category = await Db.Categories.FirstOrDefaultAsync(x => x.CompanyCen == company.Cen && x.Cen == cen);
        if (category is null) return NotFound();

        category.Code = request.Code;
        category.Name = request.Name;
        category.Description = request.Description;
        category.Active = request.Active;
        category.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync();

        return Ok(new CategoryDto(category.Cen.ToString(), category.Code, category.Name, category.Description, category.Active));
    }

    [HttpGet("units")]
    public async Task<IActionResult> GetUnits()
    {
        var items = await Db.UnitsMeasure
            .OrderBy(x => x.Name)
            .Select(x => new UnitDto(x.Cen.ToString(), x.Code, x.Name, x.Abbreviation, x.Active))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("units")]
    public async Task<IActionResult> CreateUnit(UpsertUnitRequest request)
    {
        var unitCode = string.IsNullOrWhiteSpace(request.Code)
            ? await GenerateNextCodeAsync(Guid.Empty, "UNI", () => Db.UnitsMeasure.CountAsync())
            : request.Code.Trim();

        var unit = new UnitMeasure
        {
            Code = unitCode,
            Name = request.Name,
            Abbreviation = request.Abbreviation,
            Active = request.Active
        };

        Db.UnitsMeasure.Add(unit);
        await Db.SaveChangesAsync();

        return Ok(new UnitDto(unit.Cen.ToString(), unit.Code, unit.Name, unit.Abbreviation, unit.Active));
    }

    [HttpPut("units/{unitCen}")]
    public async Task<IActionResult> UpdateUnit(string unitCen, UpsertUnitRequest request)
    {
        if (!TryParseCen(unitCen, out var cen)) return NotFound();
        var unit = await Db.UnitsMeasure.FirstOrDefaultAsync(x => x.Cen == cen);
        if (unit is null) return NotFound();

        unit.Code = request.Code;
        unit.Name = request.Name;
        unit.Abbreviation = request.Abbreviation;
        unit.Active = request.Active;
        await Db.SaveChangesAsync();

        return Ok(new UnitDto(unit.Cen.ToString(), unit.Code, unit.Name, unit.Abbreviation, unit.Active));
    }

    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses(string companyCen)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var items = await Db.Warehouses
            .Where(x => x.CompanyCen == company.Cen && x.Active)
            .OrderBy(x => x.Name)
            .Select(x => new WarehouseDto(x.Cen.ToString(), x.Code, x.Name, x.Description, x.Active))
            .ToListAsync();

        return Ok(items);
    }
}
