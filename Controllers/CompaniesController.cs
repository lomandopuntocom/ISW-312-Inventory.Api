using Inventory.Api.Application.Dtos;
using Inventory.Api.Domain.Entities;
using Inventory.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/inventory/companies")]
public sealed class CompaniesController(InventoryDbContext db) : InventoryControllerBase(db)
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companies = await Db.Companies
            .OrderBy(x => x.Name)
            .Select(x => new CompanyDto(x.Cen.ToString(), x.Name, x.Nit, x.Active))
            .ToListAsync();

        return Ok(companies);
    }

    [HttpGet("{companyCen}")]
    public async Task<IActionResult> GetByCen(string companyCen)
    {
        var company = await FindCompanyAsync(companyCen);
        return company is null
            ? NotFound()
            : Ok(new CompanyDto(company.Cen.ToString(), company.Name, company.Nit, company.Active));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCompany(CreateCompanyContractRequest request)
    {
        var company = new Company
        {
            Name = request.Name,
            Active = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Db.Companies.Add(company);
        await Db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetByCen),
            new { companyCen = company.Cen.ToString() },
            new CompanyContractDto(company.Cen.ToString(), company.Name, company.Active));
    }

    [HttpPut("{companyCen}")]
    public async Task<IActionResult> UpdateCompany(string companyCen, UpdateCompanyContractRequest request)
    {
        if (!TryParseCen(companyCen, out var cen)) return NotFound();

        var company = await Db.Companies.FirstOrDefaultAsync(x => x.Cen == cen);
        if (company is null) return NotFound();

        company.Name = request.Name;
        company.Active = request.IsActive;
        company.UpdatedAt = DateTime.UtcNow;

        await Db.SaveChangesAsync();

        return Ok(new CompanyContractDto(company.Cen.ToString(), company.Name, company.Active));
    }
}

