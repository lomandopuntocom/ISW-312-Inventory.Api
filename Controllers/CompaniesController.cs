using Inventory.Api.Application.Dtos;
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
}
