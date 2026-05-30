-- ============================================================
-- BOOTSTRAP POR CEN DE EMPRESA (idempotente)
-- Cuando ya existe inventory.companies y faltan almacén, stock, sales, etc.
--
-- Requisito: DEFAULT en columnas cen (este script los configura al inicio).
-- CEN de filas nuevas: generado por la BD. Solo se referencia el CEN existente
-- de la empresa en _seed_vars (no es un UUID inventado para el seed).
--
-- 1. Edita company_cen abajo (SELECT id, cen, name FROM inventory.companies;)
-- 2. Ejecuta el script completo (BEGIN … COMMIT)
-- ============================================================

BEGIN;

CREATE EXTENSION IF NOT EXISTS pgcrypto;

ALTER TABLE inventory.companies ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.locations ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.warehouses ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE inventory.stock ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE sales."Company" ALTER COLUMN "Cen" SET DEFAULT gen_random_uuid();
ALTER TABLE sales."Location" ALTER COLUMN "Cen" SET DEFAULT gen_random_uuid();
ALTER TABLE sales."Vendor" ALTER COLUMN "Cen" SET DEFAULT gen_random_uuid();
ALTER TABLE sales.command_stations ALTER COLUMN cen SET DEFAULT gen_random_uuid();
ALTER TABLE sales.tax_configuration ALTER COLUMN cen SET DEFAULT gen_random_uuid();

UPDATE inventory.companies SET cen = gen_random_uuid() WHERE cen IS NULL;

CREATE TEMP TABLE _seed_vars (company_cen uuid);
INSERT INTO _seed_vars (company_cen) VALUES ('9f2a4e4e-ac9d-46a4-98ea-412d1c168d12'::uuid);

CREATE TEMP TABLE _company AS
SELECT c.id, c.cen, c.name
FROM inventory.companies c
JOIN _seed_vars v ON c.cen = v.company_cen;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM _company) THEN
    RAISE EXCEPTION 'No existe inventory.companies con ese CEN. Revisa _seed_vars.company_cen.';
  END IF;
END $$;

INSERT INTO inventory.locations (company_id, company_cen, code, name, active, created_at, updated_at)
SELECT c.id, c.cen, 'LOC-00001', 'Local Principal', true, NOW(), NOW()
FROM _company c
WHERE NOT EXISTS (
  SELECT 1 FROM inventory.locations l WHERE l.company_id = c.id AND l.code = 'LOC-00001'
);

INSERT INTO inventory.warehouses (company_id, company_cen, location_id, location_cen, code, name, description, active, created_at, updated_at)
SELECT c.id, c.cen, l.id, l.cen, 'ALM-00001', 'Almacén Principal', 'Almacén por defecto', true, NOW(), NOW()
FROM _company c
JOIN inventory.locations l ON l.company_id = c.id AND l.code = 'LOC-00001'
WHERE NOT EXISTS (
  SELECT 1 FROM inventory.warehouses w WHERE w.company_id = c.id AND w.code = 'ALM-00001'
);

INSERT INTO inventory.stock (
  company_id, company_cen, location_id, location_cen,
  warehouse_id, warehouse_cen, product_id, product_cen,
  quantity, min_quantity, max_quantity, created_at, updated_at
)
SELECT
  p.company_id, p.company_cen, l.id, l.cen,
  w.id, w.cen, p.id, p.cen,
  0, 0, 100, NOW(), NOW()
FROM _company c
JOIN inventory.products p ON p.company_id = c.id AND p.track_stock = true
JOIN inventory.warehouses w ON w.company_id = c.id AND w.code = 'ALM-00001'
JOIN inventory.locations l ON l.id = w.location_id
WHERE NOT EXISTS (
  SELECT 1 FROM inventory.stock s
  WHERE s.product_cen = p.cen AND s.warehouse_cen = w.cen
);

INSERT INTO purchases.suppliers (company_id, code, name, active, created_at, updated_at)
SELECT c.id, v.code, v.name, true, NOW(), NOW()
FROM _company c
CROSS JOIN (VALUES
  ('SUP-00001', 'Distribuidora Miraflores'),
  ('SUP-00002', 'Importaciones del Sur'),
  ('SUP-00003', 'Bebidas del Norte')
) AS v(code, name)
WHERE NOT EXISTS (
  SELECT 1 FROM purchases.suppliers s
  WHERE s.company_id = c.id AND s.code = v.code
);

INSERT INTO sales."Company" ("Id", "Cen", "Name")
SELECT c.id, c.cen, c.name
FROM _company c
WHERE NOT EXISTS (
  SELECT 1 FROM sales."Company" sc WHERE sc."Cen" = c.cen
);

INSERT INTO sales."Location" ("Id", "Cen", "CompanyId", "CompanyCen", "Name")
SELECT l.id, l.cen, c.id, c.cen, l.name
FROM _company c
JOIN inventory.locations l ON l.company_id = c.id AND l.code = 'LOC-00001'
WHERE NOT EXISTS (
  SELECT 1 FROM sales."Location" sl WHERE sl."Cen" = l.cen
);

INSERT INTO sales.tax_configuration (company_cen, tax_rate)
SELECT c.cen, 0.18
FROM _company c
WHERE NOT EXISTS (
  SELECT 1 FROM sales.tax_configuration t WHERE t.company_cen = c.cen
);

INSERT INTO sales.command_stations (company_id, company_cen, code, name, station_type, description, active, created_at, updated_at)
SELECT c.id, c.cen, v.code, v.name, v.station_type, v.description, true, NOW(), NOW()
FROM _company c
CROSS JOIN (VALUES
  ('EST-00001', 'Cocina Principal', 'KITCHEN', 'Estación cocina'),
  ('EST-00002', 'Bar',              'BAR',     'Estación bar')
) AS v(code, name, station_type, description)
WHERE NOT EXISTS (
  SELECT 1 FROM sales.command_stations cs
  WHERE cs.company_cen = c.cen AND cs.code = v.code
);

INSERT INTO sales."Vendor" ("CompanyId", "CompanyCen", "Name", "Email", "Phone", "IsWaiter", "Active", "CreatedAt", "UpdatedAt")
SELECT c.id, c.cen, 'Mesero Demo', 'mesero@demo.local', '999000111', true, true, NOW(), NOW()
FROM _company c
WHERE NOT EXISTS (
  SELECT 1 FROM sales."Vendor" v WHERE v."CompanyCen" = c.cen AND v."Email" = 'mesero@demo.local'
);

SELECT 'company' AS entidad, c.id::text AS id, c.cen::text AS cen, c.name
FROM _company c
UNION ALL
SELECT 'warehouse', w.id::text, w.cen::text, w.name
FROM inventory.warehouses w
JOIN _company c ON w.company_id = c.id
UNION ALL
SELECT 'supplier', s.id::text, s.code, s.name
FROM purchases.suppliers s
JOIN _company c ON s.company_id = c.id
ORDER BY entidad, name;

COMMIT;
