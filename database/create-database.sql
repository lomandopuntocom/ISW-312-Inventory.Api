-- ============================================================
-- SCRIPT DE INICIALIZACIÓN - pgAdmin
-- Ejecutar este script primero conectado al servidor de PostgreSQL (base 'postgres')
-- ============================================================

-- 1. Crear la base de datos
SELECT 'Creando base de datos...' AS msg;
-- Nota: Si ejecutas esto en pgAdmin, asegúrate de no estar conectado a una BD que bloquee la creación.
CREATE DATABASE "ISW-312-PROJ1";

-- ============================================================
-- INSTRUCCIONES: Una vez creada la base de datos, conéctate a
-- "ISW-312-PROJ1" y ejecuta el siguiente bloque:
-- ============================================================

-- 2. Crear los esquemas de base de datos
CREATE SCHEMA IF NOT EXISTS inventory;
CREATE SCHEMA IF NOT EXISTS sales;
CREATE SCHEMA IF NOT EXISTS purchases;

-- 3. Habilitar la extensión para generación de UUIDs/GUIDs
CREATE EXTENSION IF NOT EXISTS pgcrypto;

SELECT 'Base de datos y esquemas inicializados correctamente.' AS resultado;
