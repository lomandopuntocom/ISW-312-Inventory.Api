-- ============================================================
-- SCRIPT DE POBLACIÓN DE DATOS SEMILLA (C# / .NET)
-- ============================================================

BEGIN;

CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- 1. Forzar generación de default para UUIDs en cen si no estuvieran presentes
ALTER TABLE inventory.companies ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.categories ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.units_measure ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.products ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.locations ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.warehouses ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.stock ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.movement_types ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.movements ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.operation_documents ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.operation_document_items ALTER COLUMN cen SET DEFAULT gen_random_uuid();

ALTER TABLE sales."Company" ALTER COLUMN "Cen" SET DEFAULT gen_random_uuid();
ALTER TABLE sales."Location" ALTER COLUMN "Cen" SET DEFAULT gen_random_uuid();
ALTER TABLE sales."Vendor" ALTER COLUMN "Cen" SET DEFAULT gen_random_uuid();
ALTER TABLE sales."Ticket" ALTER COLUMN "Cen" SET DEFAULT gen_random_uuid();
ALTER TABLE sales."TicketItem" ALTER COLUMN "Cen" SET DEFAULT gen_random_uuid();
ALTER TABLE sales."Payment" ALTER COLUMN "Cen" SET DEFAULT gen_random_uuid();
ALTER TABLE sales.command_stations ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE sales.commands ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE sales.command_items ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE sales.tax_configuration ALTER COLUMN cen SET DEFAULT gen_random_uuid();

ALTER TABLE purchases.orders ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE purchases.order_items ALTER COLUMN cen SET DEFAULT gen_random_uuid();

-- ============================================================
-- DATOS: inventory
-- ============================================================

-- Empresa Demo (El Sabor)
INSERT INTO inventory.companies (name, nit, phone, email, address, city, country, active, created_at, updated_at)
SELECT
  'Restaurante El Sabor', '20123456789', '01-4445566', 'info@elsabor.pe',
  'Av. Larco 123', 'Lima', 'Peru', true, NOW(), NOW()
WHERE NOT EXISTS (
  SELECT 1 FROM inventory.companies WHERE nit = '20123456789'
);

-- Unidades de Medida
INSERT INTO inventory.units_measure (code, name, abbreviation, active, created_at)
SELECT v.code, v.name, v.abbreviation, true, NOW()
FROM (VALUES
  ('UNI-00001', 'Unidad',    'und'),
  ('UNI-00002', 'Kilogramo', 'kg'),
  ('UNI-00003', 'Litro',     'lt'),
  ('UNI-00004', 'Porcion',   'por')
) AS v(code, name, abbreviation)
WHERE NOT EXISTS (
  SELECT 1 FROM inventory.units_measure u WHERE u.code = v.code
);

-- Categorías
INSERT INTO inventory.categories (company_id, company_cen, code, name, description, active, created_at, updated_at)
SELECT c.id, c.cen, v.code, v.name, v.description, true, NOW(), NOW()
FROM inventory.companies c
CROSS JOIN (VALUES
  ('CAT-00001', 'Entradas', 'Platos de entrada'),
  ('CAT-00002', 'Fondos',   'Platos principales'),
  ('CAT-00003', 'Bebidas',  'Bebidas y jugos'),
  ('CAT-00004', 'Postres',  'Postres y dulces')
) AS v(code, name, description)
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM inventory.categories cat
    WHERE cat.company_cen = c.cen AND cat.code = v.code
  );

-- Productos
INSERT INTO inventory.products (
  company_id, company_cen, code, sku, name, description,
  category_id, category_cen, unit_measure_id, unit_measure_cen,
  price, cost, track_stock, is_out_of_stock, active, station_code, created_at, updated_at
)
SELECT
  c.id, c.cen, v.code, v.sku, v.name, v.description,
  cat.id, cat.cen, u.id, u.cen,
  v.price, v.cost, v.track_stock, false, true, v.station_code, NOW(), NOW()
FROM inventory.companies c
CROSS JOIN (VALUES
  ('PRO-00001', 'SKU-001', 'Ceviche Clasico',     'Ceviche de pescado blanco',    'CAT-00002', 'UNI-00001', 35.00, 12.00, false, 'COCINA'),
  ('PRO-00002', 'SKU-002', 'Lomo Saltado',        'Lomo saltado con papas',       'CAT-00002', 'UNI-00001', 42.00, 15.00, false, 'COCINA'),
  ('PRO-00003', 'SKU-003', 'Causa Limena',        'Causa de pollo',               'CAT-00001', 'UNI-00001', 22.00,  7.00, false, 'COCINA'),
  ('PRO-00004', 'SKU-004', 'Inca Kola 500ml',     'Gaseosa Inca Kola personal',   'CAT-00003', 'UNI-00001',  8.00,  3.00, true,  'BAR'),
  ('PRO-00005', 'SKU-005', 'Agua Mineral 600ml',  'Agua mineral San Luis',        'CAT-00003', 'UNI-00001',  5.00,  1.50, true,  'BAR'),
  ('PRO-00006', 'SKU-006', 'Picarones',           'Picarones con miel de cana',   'CAT-00004', 'UNI-00001', 15.00,  5.00, false, 'COCINA'),
  ('PRO-00007', 'SKU-007', 'Chicharron de Pollo', 'Chicharron crocante',          'CAT-00001', 'UNI-00001', 28.00, 10.00, false, 'COCINA'),
  ('PRO-00008', 'SKU-008', 'Jugo de Maracuya',    'Jugo fresco de maracuya',      'CAT-00003', 'UNI-00001', 12.00,  3.00, false, 'BAR')
) AS v(code, sku, name, description, cat_code, unit_code, price, cost, track_stock, station_code)
JOIN inventory.categories cat ON cat.company_cen = c.cen AND cat.code = v.cat_code
JOIN inventory.units_measure u ON u.code = v.unit_code
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM inventory.products p
    WHERE p.company_cen = c.cen AND p.code = v.code
  );

-- Local Principal
INSERT INTO inventory.locations (company_id, company_cen, code, name, address, phone, active, created_at, updated_at)
SELECT c.id, c.cen, 'LOC-00001', 'Local Principal', 'Av. Larco 123', '01-4445566', true, NOW(), NOW()
FROM inventory.companies c
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM inventory.locations l WHERE l.company_id = c.id AND l.code = 'LOC-00001'
  );

-- Almacén Principal
INSERT INTO inventory.warehouses (company_id, company_cen, location_id, location_cen, code, name, description, active, created_at, updated_at)
SELECT c.id, c.cen, l.id, l.cen, 'ALM-00001', 'Almacen Principal', 'Almacen central', true, NOW(), NOW()
FROM inventory.companies c
JOIN inventory.locations l ON l.company_id = c.id AND l.code = 'LOC-00001'
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM inventory.warehouses w WHERE w.company_id = c.id AND w.code = 'ALM-00001'
  );

-- Stock Inicial
INSERT INTO inventory.stock (
  company_id, company_cen, location_id, location_cen,
  warehouse_id, warehouse_cen, product_id, product_cen,
  quantity, min_quantity, max_quantity, created_at, updated_at
)
SELECT
  c.id, c.cen, l.id, l.cen, w.id, w.cen, p.id, p.cen,
  v.quantity, v.min_quantity, v.max_quantity, NOW(), NOW()
FROM inventory.companies c
JOIN inventory.locations l ON l.company_id = c.id AND l.code = 'LOC-00001'
JOIN inventory.warehouses w ON w.company_id = c.id AND w.code = 'ALM-00001'
JOIN (VALUES
  ('PRO-00004', 48.00, 12.00, 100.00),
  ('PRO-00005', 24.00,  6.00,  60.00)
) AS v(prod_code, quantity, min_quantity, max_quantity) ON true
JOIN inventory.products p ON p.company_cen = c.cen AND p.code = v.prod_code
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM inventory.stock s
    WHERE s.product_cen = p.cen AND s.warehouse_cen = w.cen
  );

-- Tipos de Movimientos de Inventario
INSERT INTO inventory.movement_types (code, name, "Description", movement_direction, active, created_at)
SELECT v.code, v.name, v.description, v.direction, true, NOW()
FROM (VALUES
  ('ENT', 'Entrada por compra',   'Ingreso de mercaderia por compra',     'IN'),
  ('SAL', 'Salida por venta',     'Salida por consumo en punto de venta', 'OUT'),
  ('AJU', 'Ajuste de inventario', 'Correccion manual de stock',           'IN')
) AS v(code, name, description, direction)
WHERE NOT EXISTS (
  SELECT 1 FROM inventory.movement_types mt WHERE mt.code = v.code
);

-- Documentos de Operación inicial
INSERT INTO inventory.operation_documents (
  company_id, company_cen, location_id, location_cen, warehouse_id, warehouse_cen,
  document_number, operation_type, status, reference, notes, created_at, confirmed_at, updated_at
)
SELECT
  c.id, c.cen, l.id, l.cen, w.id, w.cen,
  'DOC-00001', 'ENTRY', 'CONFIRMED', 'Compra inicial', 'Stock inicial de bebidas', NOW(), NOW(), NOW()
FROM inventory.companies c
JOIN inventory.locations l ON l.company_id = c.id AND l.code = 'LOC-00001'
JOIN inventory.warehouses w ON w.company_id = c.id AND w.code = 'ALM-00001'
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM inventory.operation_documents d
    WHERE d.company_cen = c.cen AND d.document_number = 'DOC-00001'
  );

INSERT INTO inventory.operation_document_items (
  document_id, document_cen, product_id, product_cen, quantity, notes, created_at, updated_at
)
SELECT
  d.id, d.cen, p.id, p.cen, v.quantity, v.notes, NOW(), NOW()
FROM inventory.companies c
JOIN inventory.operation_documents d ON d.company_cen = c.cen AND d.document_number = 'DOC-00001'
CROSS JOIN (VALUES
  ('PRO-00004', 48.00, 'Stock inicial Inca Kola'),
  ('PRO-00005', 24.00, 'Stock inicial Agua Mineral')
) AS v(prod_code, quantity, notes)
JOIN inventory.products p ON p.company_cen = c.cen AND p.code = v.prod_code
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM inventory.operation_document_items oi
    WHERE oi.document_cen = d.cen AND oi.product_cen = p.cen
  );

-- ============================================================
-- DATOS: sales
-- ============================================================

INSERT INTO sales."Company" ("Id", "Cen", "Name")
SELECT c.id, c.cen, c.name
FROM inventory.companies c
WHERE c.nit = '20123456789'
  AND NOT EXISTS (SELECT 1 FROM sales."Company" sc WHERE sc."Cen" = c.cen);

INSERT INTO sales."Location" ("Id", "Cen", "CompanyId", "CompanyCen", "Name")
SELECT l.id, l.cen, c.id, c.cen, l.name
FROM inventory.companies c
JOIN inventory.locations l ON l.company_id = c.id AND l.code = 'LOC-00001'
WHERE c.nit = '20123456789'
  AND NOT EXISTS (SELECT 1 FROM sales."Location" sl WHERE sl."Cen" = l.cen);

INSERT INTO sales.tax_configuration (company_cen, tax_rate)
SELECT c.cen, 0.18
FROM inventory.companies c
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM sales.tax_configuration t WHERE t.company_cen = c.cen
  );

-- Vendedores / Meseros
INSERT INTO sales."Vendor" ("CompanyId", "CompanyCen", "Name", "Email", "Phone", "IsWaiter", "Active", "CreatedAt", "UpdatedAt")
SELECT c.id, c.cen, v.name, v.email, v.phone, v.is_waiter, true, NOW(), NOW()
FROM inventory.companies c
CROSS JOIN (VALUES
  ('Carlos Ramirez', 'carlos@elsabor.pe', '987654321', true),
  ('Maria Lopez',    'maria@elsabor.pe',  '987654322', true),
  ('Admin',          'admin@elsabor.pe',  '987654323', false)
) AS v(name, email, phone, is_waiter)
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM sales."Vendor" sv
    WHERE sv."CompanyCen" = c.cen AND sv."Email" = v.email
  );

-- Estaciones KDS
INSERT INTO sales.command_stations (company_id, company_cen, code, name, station_type, description, active, created_at, updated_at)
SELECT c.id, c.cen, v.code, v.name, v.station_type, v.description, true, NOW(), NOW()
FROM inventory.companies c
CROSS JOIN (VALUES
  ('EST-00001', 'Cocina Principal', 'KITCHEN', 'Estacion de cocina caliente'),
  ('EST-00002', 'Bar',              'BAR',     'Estacion de bebidas')
) AS v(code, name, station_type, description)
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM sales.command_stations cs
    WHERE cs.company_cen = c.cen AND cs.code = v.code
  );

-- Tickets de Prueba
INSERT INTO sales."Ticket" (
  "CompanyId", "CompanyCen", "LocationId", "LocationCen",
  "TicketNumber", "VendorId", "VendorCen", "TableCode", "Status"
)
SELECT
  c.id, c.cen, l.id, l.cen,
  v.ticket_number, ven."Id", ven."Cen", v.table_code, v.status
FROM inventory.companies c
JOIN inventory.locations l ON l.company_id = c.id AND l.code = 'LOC-00001'
CROSS JOIN (VALUES
  ('TIC-00001', 'MESA-01', 'PAID', 'carlos@elsabor.pe'),
  ('TIC-00002', 'MESA-02', 'OPEN', 'maria@elsabor.pe'),
  ('TIC-00003', 'MESA-03', 'OPEN', 'carlos@elsabor.pe')
) AS v(ticket_number, table_code, status, vendor_email)
JOIN sales."Vendor" ven ON ven."CompanyCen" = c.cen AND ven."Email" = v.vendor_email
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM sales."Ticket" t
    WHERE t."CompanyCen" = c.cen AND t."TicketNumber" = v.ticket_number
  );

-- Items de los Tickets
INSERT INTO sales."TicketItem" (
  "TicketId", "TicketCen", "ProductId", "ProductCen",
  "Quantity", "UnitPrice", "Status", "Notes"
)
SELECT
  t."Id", t."Cen", p.id, p.cen,
  v.quantity, v.unit_price, v.status, v.notes
FROM inventory.companies c
JOIN sales."Ticket" t ON t."CompanyCen" = c.cen
CROSS JOIN (VALUES
  ('TIC-00001', 'PRO-00001', 1.00, 35.00, 'DELIVERED', NULL),
  ('TIC-00001', 'PRO-00004', 2.00,  8.00, 'DELIVERED', NULL),
  ('TIC-00002', 'PRO-00002', 2.00, 42.00, 'PENDING',   NULL),
  ('TIC-00002', 'PRO-00008', 2.00, 12.00, 'PENDING',   NULL),
  ('TIC-00003', 'PRO-00003', 1.00, 22.00, 'DELIVERED', NULL),
  ('TIC-00003', 'PRO-00006', 1.00, 15.00, 'PENDING',   'Sin canela')
) AS v(ticket_number, prod_code, quantity, unit_price, status, notes)
JOIN inventory.products p ON p.company_cen = c.cen AND p.code = v.prod_code
WHERE c.nit = '20123456789'
  AND t."TicketNumber" = v.ticket_number
  AND NOT EXISTS (
    SELECT 1 FROM sales."TicketItem" ti
    WHERE ti."TicketCen" = t."Cen" AND ti."ProductCen" = p.cen
  );

-- Pagos
INSERT INTO sales."Payment" (
  "TicketId", "TicketCen", "PaymentMethod", "Amount", "Reference", "PaidBy", "CreatedAt", "UpdatedAt"
)
SELECT
  t."Id", t."Cen", 'CASH', 51.00, NULL, 'Carlos Ramirez', NOW(), NOW()
FROM inventory.companies c
JOIN sales."Ticket" t ON t."CompanyCen" = c.cen AND t."TicketNumber" = 'TIC-00001'
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM sales."Payment" p WHERE p."TicketCen" = t."Cen"
  );

-- Comandas KDS
INSERT INTO sales.commands (
  company_id, company_cen, location_id, location_cen,
  ticket_id, ticket_cen, station_id, station_cen,
  command_number, status, is_reorder, created_at, sent_at, ready_at, updated_at
)
SELECT
  c.id, c.cen, l.id, l.cen,
  ticket."Id", ticket."Cen", st."Id", st.cen,
  v.command_number, v.status, false, NOW(), v.sent_at, v.ready_at, NOW()
FROM inventory.companies c
JOIN inventory.locations l ON l.company_id = c.id AND l.code = 'LOC-00001'
CROSS JOIN (VALUES
  ('TIC-00001', 'EST-00001', 'COM-00001', 'READY', NOW(), NOW()),
  ('TIC-00002', 'EST-00001', 'COM-00002', 'SENT',  NOW(), NULL)
) AS v(ticket_number, station_code, command_number, status, sent_at, ready_at)
JOIN sales."Ticket" ticket ON ticket."CompanyCen" = c.cen AND ticket."TicketNumber" = v.ticket_number
JOIN sales.command_stations st ON st.company_cen = c.cen AND st.code = v.station_code
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM sales.commands cmd
    WHERE cmd.company_cen = c.cen AND cmd.command_number = v.command_number
  );

INSERT INTO sales.command_items (
  command_id, command_cen, ticket_item_id, ticket_item_cen,
  product_id, product_cen, quantity, status, notes, created_at, updated_at
)
SELECT
  cmd."Id", cmd.cen, ti."Id", ti."Cen",
  p.id, p.cen, ti."Quantity", ti."Status", ti."Notes", NOW(), NOW()
FROM inventory.companies c
JOIN sales.commands cmd ON cmd.company_cen = c.cen
JOIN sales."Ticket" ticket ON ticket."Cen" = cmd.ticket_cen
JOIN sales."TicketItem" ti ON ti."TicketId" = ticket."Id"
JOIN inventory.products p ON p.cen = ti."ProductCen"
CROSS JOIN (VALUES
  ('COM-00001', 'PRO-00001'),
  ('COM-00002', 'PRO-00002')
) AS v(command_number, prod_code)
WHERE c.nit = '20123456789'
  AND cmd.command_number = v.command_number
  AND p.code = v.prod_code
  AND NOT EXISTS (
    SELECT 1 FROM sales.command_items ci
    WHERE ci.command_cen = cmd.cen AND ci.ticket_item_cen = ti."Cen"
  );

-- ============================================================
-- DATOS: purchases
-- ============================================================

-- Proveedores
INSERT INTO purchases.suppliers (company_id, code, name, active, created_at, updated_at)
SELECT c.id, v.code, v.name, true, NOW(), NOW()
FROM inventory.companies c
CROSS JOIN (VALUES
  ('SUP-00001', 'Distribuidora Miraflores'),
  ('SUP-00002', 'Importaciones del Sur')
) AS v(code, name)
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM purchases.suppliers s
    WHERE s.company_id = c.id AND s.code = v.code
  );

-- Órdenes de Compra
INSERT INTO purchases.orders (company_id, company_cen, supplier, supplier_cen, date, status, created_at, updated_at)
SELECT c.id, c.cen, s.name, NULL, NOW(), v.status, NOW(), NOW()
FROM inventory.companies c
CROSS JOIN (VALUES
  ('SUP-00001', 'CONFIRMED'),
  ('SUP-00002', 'DRAFT')
) AS v(supplier_code, status)
JOIN purchases.suppliers s ON s.company_id = c.id AND s.code = v.supplier_code
WHERE c.nit = '20123456789'
  AND NOT EXISTS (
    SELECT 1 FROM purchases.orders o
    WHERE o.company_cen = c.cen AND o.supplier = s.name AND o.status = v.status
  );

-- Detalle de Órdenes de Compra
INSERT INTO purchases.order_items (order_id, order_cen, product_id, product_cen, quantity, created_at)
SELECT ord."Id", ord.cen, p.id, p.cen, v.quantity, NOW()
FROM inventory.companies c
JOIN purchases.orders ord ON ord.company_cen = c.cen
JOIN purchases.suppliers s ON s.company_id = c.id
CROSS JOIN (VALUES
  ('Distribuidora Miraflores', 'CONFIRMED', 'PRO-00004', 48.00),
  ('Distribuidora Miraflores', 'CONFIRMED', 'PRO-00005', 24.00),
  ('Importaciones del Sur',    'DRAFT',     'PRO-00002', 10.00)
) AS v(supplier_name, order_status, prod_code, quantity)
JOIN inventory.products p ON p.company_cen = c.cen AND p.code = v.prod_code
WHERE c.nit = '20123456789'
  AND ord.supplier = v.supplier_name
  AND ord.status = v.order_status
  AND NOT EXISTS (
    SELECT 1 FROM purchases.order_items oi
    WHERE oi.order_cen = ord.cen AND oi.product_cen = p.cen
  );

COMMIT;
