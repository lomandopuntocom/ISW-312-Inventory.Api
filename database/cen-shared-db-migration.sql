BEGIN;

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

ALTER TABLE IF EXISTS inventory.companies ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.categories ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.categories ADD COLUMN IF NOT EXISTS company_cen uuid;
ALTER TABLE IF EXISTS inventory.locations ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.locations ADD COLUMN IF NOT EXISTS company_cen uuid;
ALTER TABLE IF EXISTS inventory.movement_types ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.movements ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.movements ADD COLUMN IF NOT EXISTS company_cen uuid;
ALTER TABLE IF EXISTS inventory.movements ADD COLUMN IF NOT EXISTS location_cen uuid;
ALTER TABLE IF EXISTS inventory.movements ADD COLUMN IF NOT EXISTS warehouse_cen uuid;
ALTER TABLE IF EXISTS inventory.movements ADD COLUMN IF NOT EXISTS product_cen uuid;
ALTER TABLE IF EXISTS inventory.movements ADD COLUMN IF NOT EXISTS movement_type_cen uuid;
ALTER TABLE IF EXISTS inventory.operation_documents ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.operation_documents ADD COLUMN IF NOT EXISTS company_cen uuid;
ALTER TABLE IF EXISTS inventory.operation_documents ADD COLUMN IF NOT EXISTS location_cen uuid;
ALTER TABLE IF EXISTS inventory.operation_documents ADD COLUMN IF NOT EXISTS warehouse_cen uuid;
ALTER TABLE IF EXISTS inventory.operation_document_items ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.operation_document_items ADD COLUMN IF NOT EXISTS document_cen uuid;
ALTER TABLE IF EXISTS inventory.operation_document_items ADD COLUMN IF NOT EXISTS product_cen uuid;
ALTER TABLE IF EXISTS inventory.products ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.products ADD COLUMN IF NOT EXISTS company_cen uuid;
ALTER TABLE IF EXISTS inventory.products ADD COLUMN IF NOT EXISTS category_cen uuid;
ALTER TABLE IF EXISTS inventory.products ADD COLUMN IF NOT EXISTS unit_measure_cen uuid;
ALTER TABLE IF EXISTS inventory.stock ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.stock ADD COLUMN IF NOT EXISTS company_cen uuid;
ALTER TABLE IF EXISTS inventory.stock ADD COLUMN IF NOT EXISTS location_cen uuid;
ALTER TABLE IF EXISTS inventory.stock ADD COLUMN IF NOT EXISTS warehouse_cen uuid;
ALTER TABLE IF EXISTS inventory.stock ADD COLUMN IF NOT EXISTS product_cen uuid;
ALTER TABLE IF EXISTS inventory.units_measure ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.warehouse_locations ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.warehouse_locations ADD COLUMN IF NOT EXISTS warehouse_cen uuid;
ALTER TABLE IF EXISTS inventory.warehouses ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS inventory.warehouses ADD COLUMN IF NOT EXISTS company_cen uuid;
ALTER TABLE IF EXISTS inventory.warehouses ADD COLUMN IF NOT EXISTS location_cen uuid;

UPDATE inventory.categories c SET company_cen = co.cen FROM inventory.companies co WHERE c.company_id = co.id AND c.company_cen IS NULL;
UPDATE inventory.locations l SET company_cen = co.cen FROM inventory.companies co WHERE l.company_id = co.id AND l.company_cen IS NULL;
UPDATE inventory.products p SET company_cen = co.cen FROM inventory.companies co WHERE p.company_id = co.id AND p.company_cen IS NULL;
UPDATE inventory.products p SET category_cen = c.cen FROM inventory.categories c WHERE p.category_id = c.id AND p.category_cen IS NULL;
UPDATE inventory.products p SET unit_measure_cen = u.cen FROM inventory.units_measure u WHERE p.unit_measure_id = u.id AND p.unit_measure_cen IS NULL;
UPDATE inventory.warehouses w SET company_cen = co.cen FROM inventory.companies co WHERE w.company_id = co.id AND w.company_cen IS NULL;
UPDATE inventory.warehouses w SET location_cen = l.cen FROM inventory.locations l WHERE w.location_id = l.id AND w.location_cen IS NULL;
UPDATE inventory.stock s SET company_cen = co.cen FROM inventory.companies co WHERE s.company_id = co.id AND s.company_cen IS NULL;
UPDATE inventory.stock s SET location_cen = l.cen FROM inventory.locations l WHERE s.location_id = l.id AND s.location_cen IS NULL;
UPDATE inventory.stock s SET warehouse_cen = w.cen FROM inventory.warehouses w WHERE s.warehouse_id = w.id AND s.warehouse_cen IS NULL;
UPDATE inventory.stock s SET product_cen = p.cen FROM inventory.products p WHERE s.product_id = p.id AND s.product_cen IS NULL;

ALTER TABLE IF EXISTS sales."Company" ADD COLUMN IF NOT EXISTS "Cen" uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS sales."Location" ADD COLUMN IF NOT EXISTS "Cen" uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS sales."Location" ADD COLUMN IF NOT EXISTS "CompanyCen" uuid;
ALTER TABLE IF EXISTS sales."Vendor" ADD COLUMN IF NOT EXISTS "Cen" uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS sales."Vendor" ADD COLUMN IF NOT EXISTS "CompanyCen" uuid;
ALTER TABLE IF EXISTS sales."Ticket" ADD COLUMN IF NOT EXISTS "Cen" uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS sales."Ticket" ADD COLUMN IF NOT EXISTS "CompanyCen" uuid;
ALTER TABLE IF EXISTS sales."Ticket" ADD COLUMN IF NOT EXISTS "LocationCen" uuid;
ALTER TABLE IF EXISTS sales."Ticket" ADD COLUMN IF NOT EXISTS "VendorCen" uuid;
ALTER TABLE IF EXISTS sales."TicketItem" ADD COLUMN IF NOT EXISTS "Cen" uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS sales."TicketItem" ADD COLUMN IF NOT EXISTS "TicketCen" uuid;
ALTER TABLE IF EXISTS sales."TicketItem" ADD COLUMN IF NOT EXISTS "ProductCen" uuid;
ALTER TABLE IF EXISTS sales."TicketItem" ADD COLUMN IF NOT EXISTS "Quantity" numeric NOT NULL DEFAULT 1;
ALTER TABLE IF EXISTS sales."TicketItem" ADD COLUMN IF NOT EXISTS "UnitPrice" numeric NOT NULL DEFAULT 0;
ALTER TABLE IF EXISTS sales."Payment" ADD COLUMN IF NOT EXISTS "Cen" uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS sales."Payment" ADD COLUMN IF NOT EXISTS "TicketCen" uuid;
ALTER TABLE IF EXISTS sales.command_stations ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS sales.command_stations ADD COLUMN IF NOT EXISTS company_cen uuid;
ALTER TABLE IF EXISTS sales.commands ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS sales.commands ADD COLUMN IF NOT EXISTS company_cen uuid;
ALTER TABLE IF EXISTS sales.commands ADD COLUMN IF NOT EXISTS location_cen uuid;
ALTER TABLE IF EXISTS sales.commands ADD COLUMN IF NOT EXISTS ticket_cen uuid;
ALTER TABLE IF EXISTS sales.commands ADD COLUMN IF NOT EXISTS station_cen uuid;
ALTER TABLE IF EXISTS sales.command_items ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS sales.command_items ADD COLUMN IF NOT EXISTS command_cen uuid;
ALTER TABLE IF EXISTS sales.command_items ADD COLUMN IF NOT EXISTS ticket_item_cen uuid;
ALTER TABLE IF EXISTS sales.command_items ADD COLUMN IF NOT EXISTS product_cen uuid;

CREATE TABLE IF NOT EXISTS sales.tax_configuration (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    cen uuid NOT NULL DEFAULT gen_random_uuid(),
    company_cen uuid NOT NULL,
    tax_rate numeric(5, 2) NOT NULL DEFAULT 0.18,
    CONSTRAINT "PK_tax_configuration" PRIMARY KEY (id)
);

UPDATE sales."Location" l SET "CompanyCen" = c."Cen" FROM sales."Company" c WHERE l."CompanyId" = c."Id" AND l."CompanyCen" IS NULL;
UPDATE sales."Vendor" v SET "CompanyCen" = c."Cen" FROM sales."Company" c WHERE v."CompanyId" = c."Id" AND v."CompanyCen" IS NULL;
UPDATE sales."Ticket" t SET "CompanyCen" = c."Cen" FROM sales."Company" c WHERE t."CompanyId" = c."Id" AND t."CompanyCen" IS NULL;
UPDATE sales."Ticket" t SET "LocationCen" = l."Cen" FROM sales."Location" l WHERE t."LocationId" = l."Id" AND t."LocationCen" IS NULL;
UPDATE sales."Ticket" t SET "VendorCen" = v."Cen" FROM sales."Vendor" v WHERE t."VendorId" = v."Id" AND t."VendorCen" IS NULL;
UPDATE sales."TicketItem" ti SET "TicketCen" = t."Cen" FROM sales."Ticket" t WHERE ti."TicketId" = t."Id" AND ti."TicketCen" IS NULL;
UPDATE sales.command_stations cs SET company_cen = c."Cen" FROM sales."Company" c WHERE cs.company_id = c."Id" AND cs.company_cen IS NULL;

ALTER TABLE IF EXISTS purchases.orders ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS purchases.orders ADD COLUMN IF NOT EXISTS company_cen uuid;
ALTER TABLE IF EXISTS purchases.orders ADD COLUMN IF NOT EXISTS supplier_cen uuid;
ALTER TABLE IF EXISTS purchases.order_items ADD COLUMN IF NOT EXISTS cen uuid NOT NULL DEFAULT gen_random_uuid();
ALTER TABLE IF EXISTS purchases.order_items ADD COLUMN IF NOT EXISTS order_cen uuid;
ALTER TABLE IF EXISTS purchases.order_items ADD COLUMN IF NOT EXISTS product_cen uuid;

UPDATE purchases.orders o SET company_cen = c.cen FROM inventory.companies c WHERE o.company_id = c.id AND o.company_cen IS NULL;
UPDATE purchases.order_items oi SET order_cen = o.cen FROM purchases.orders o WHERE oi.order_id = o.id AND oi.order_cen IS NULL;
UPDATE purchases.order_items oi SET product_cen = p.cen FROM inventory.products p WHERE oi.product_id = p.id AND oi.product_cen IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS "UX_inventory_companies_cen" ON inventory.companies(cen);
CREATE UNIQUE INDEX IF NOT EXISTS "UX_inventory_categories_cen" ON inventory.categories(cen);
CREATE UNIQUE INDEX IF NOT EXISTS "UX_inventory_products_cen" ON inventory.products(cen);
CREATE UNIQUE INDEX IF NOT EXISTS "UX_inventory_stock_cen" ON inventory.stock(cen);
CREATE UNIQUE INDEX IF NOT EXISTS "UX_sales_company_cen" ON sales."Company"("Cen");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_sales_ticket_cen" ON sales."Ticket"("Cen");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_sales_ticket_item_cen" ON sales."TicketItem"("Cen");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_purchases_orders_cen" ON purchases.orders(cen);
CREATE UNIQUE INDEX IF NOT EXISTS "UX_purchases_order_items_cen" ON purchases.order_items(cen);

COMMIT;
