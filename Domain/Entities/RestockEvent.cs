using System;

namespace Inventory.Api.Domain.Entities;

public sealed record RestockEvent(
    string CompanyCen,
    string ProductCen,
    string ProductCode,
    string ProductName,
    decimal Quantity,
    decimal NewStock,
    string WarehouseCen,
    DateTime Timestamp
);
