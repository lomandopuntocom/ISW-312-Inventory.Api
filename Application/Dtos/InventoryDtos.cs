namespace Inventory.Api.Application.Dtos;

public sealed record PagedResultDto<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public sealed record CompanyDto(string Cen, string Name, string? Nit, bool Active);
public sealed record CategoryDto(string Cen, string Code, string Name, string? Description, bool Active);
public sealed record UnitDto(string Cen, string Code, string Name, string? Abbreviation, bool Active);
public sealed record WarehouseDto(string Cen, string Code, string Name, string? Description, bool Active);
public sealed record ProductDto(
    string Cen,
    string Code,
    string Sku,
    string Name,
    string? Description,
    string? CategoryCen,
    string? UnitCen,
    decimal Price,
    decimal Cost,
    bool TrackStock,
    bool IsOutOfStock,
    bool Active,
    string? StationCode);

public sealed record StockDto(string ProductCen, string ProductCode, string? WarehouseCen, decimal Quantity, decimal MinQuantity, bool LowStock);
public sealed record InventoryDashboardDto(int ProductCount, int ActiveProductCount, int OutOfStockCount, int LowStockCount);
public sealed record KardexMovementDto(string Cen, string ProductCen, decimal Quantity, decimal BalanceBefore, decimal BalanceAfter, string? Reference, string? Notes, DateTime CreatedAt);

public sealed record UpsertCategoryRequest(string Code, string Name, string? Description, bool Active = true);
public sealed record UpsertUnitRequest(string Code, string Name, string? Abbreviation, bool Active = true);
public sealed record UpsertProductRequest(
    string Code,
    string Sku,
    string Name,
    string? Description,
    string? CategoryCen,
    string? UnitCen,
    decimal Price,
    decimal Cost,
    bool TrackStock = true,
    string? StationCode = null);

public sealed record ProductStatusRequest(bool Active, bool IsOutOfStock);
public sealed record ProductLookupRequest(IReadOnlyList<string> ProductCens);
public sealed record StockValidationRequest(string ProductCen, string? WarehouseCen, decimal Quantity);
public sealed record StockMutationRequest(string ProductCen, string? WarehouseCen, decimal Quantity, string? Reference, string? Notes);
public sealed record StockAdjustmentRequest(string ProductCen, string? WarehouseCen, decimal Quantity, string Reason);
public sealed record CreateDocumentRequest(string? LocationCen, string? WarehouseCen, string OperationType, string? Reference, string? Notes, IReadOnlyList<CreateDocumentItemRequest> Items);
public sealed record CreateDocumentItemRequest(string ProductCen, decimal Quantity, string? Notes);
