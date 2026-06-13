using Inventory.Api.Application.Dtos;
using Inventory.Api.Domain.Entities;
using Inventory.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/inventory/companies/{companyCen}/documents")]
public sealed class DocumentsController(InventoryDbContext db) : InventoryControllerBase(db)
{
    [HttpGet]
    public async Task<IActionResult> GetDocuments(string companyCen)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var documents = await Db.OperationDocuments
            .Where(x => x.CompanyCen == company.Cen)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                cen = x.Cen,
                x.DocumentNumber,
                x.OperationType,
                x.Status,
                x.Reference,
                x.CreatedAt,
                x.ConfirmedAt
            })
            .ToListAsync();

        return Ok(documents);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDocument(string companyCen, CreateDocumentRequest request)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var document = new OperationDocument
        {
            CompanyId = company.Id,
            CompanyCen = company.Cen,
            LocationCen = TryParseCen(request.LocationCen ?? string.Empty, out var location) ? location : Guid.Empty,
            WarehouseCen = TryParseCen(request.WarehouseCen ?? string.Empty, out var warehouse) ? warehouse : Guid.Empty,
            DocumentNumber = $"DOC-{DateTime.UtcNow:yyyyMMddHHmmss}",
            OperationType = request.OperationType,
            Status = "DRAFT",
            Reference = request.Reference,
            Notes = request.Notes
        };

        foreach (var item in request.Items)
        {
            if (!TryParseCen(item.ProductCen, out var productCen)) continue;
            document.Items.Add(new OperationDocumentItem
            {
                DocumentCen = document.Cen,
                ProductCen = productCen,
                Quantity = item.Quantity,
                Notes = item.Notes
            });
        }

        Db.OperationDocuments.Add(document);
        await Db.SaveChangesAsync();

        return Ok(new { cen = document.Cen, document.DocumentNumber, document.Status });
    }

    [HttpGet("{documentCen}")]
    public async Task<IActionResult> GetDocumentByCen(string companyCen, string documentCen)
    {
        var company = await FindCompanyAsync(companyCen);
        if (company is null || !TryParseCen(documentCen, out var docCen)) return NotFound();

        var doc = await Db.OperationDocuments
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.CompanyCen == company.Cen && x.Cen == docCen);

        if (doc is null) return NotFound();

        var movementCens = await Db.Movements
            .Where(x => x.CompanyCen == company.Cen && (x.Reference == doc.DocumentNumber || x.Reference == doc.Cen.ToString()))
            .Select(x => x.Cen.ToString())
            .ToListAsync();

        var dto = new InventoryDocumentContractDto(
            doc.Cen.ToString(),
            doc.OperationType,
            doc.Status,
            doc.DocumentNumber,
            doc.CreatedAt,
            doc.Items.Count,
            movementCens);

        return Ok(dto);
    }
}

