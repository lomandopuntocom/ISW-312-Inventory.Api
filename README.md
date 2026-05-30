# Módulo de Inventario (Inventory.Api)

Este proyecto es la API de backend para el módulo de Gestión de Inventario, construida sobre **ASP.NET Core 9.0** y **Entity Framework Core**.

---

## 🛠️ Requisitos Previos

Asegúrate de tener instalado:
* [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
* [PostgreSQL](https://www.postgresql.org/download/)
* [pgAdmin 4](https://www.pgadmin.org/download/) (o herramienta de administración similar)
* Herramientas de EF Core (instalar globalmente con `dotnet tool install --global dotnet-ef` si no lo tienes).

---

## 🚀 Guía de Instalación y Ejecución Paso a Paso

### Paso 1: Configurar la Base de Datos en pgAdmin
1. Abre **pgAdmin** y conéctate a tu servidor local de PostgreSQL.
2. Abre la consola de consultas (**Query Tool**) sobre el servidor o la base de datos por defecto (`postgres`).
3. Abre y ejecuta el contenido del script localizado en:  
   `database/create-database.sql` (esto creará la base de datos `ISW-312-PROJ1`, los esquemas `inventory`, `sales`, `purchases` y la extensión `pgcrypto`).

### Paso 2: Configurar las Variables de Entorno
1. En la raíz del repositorio, copia el archivo de plantilla `.env.example` y nómbralo como `.env`.
2. Edita `.env` con tus credenciales de PostgreSQL si son distintas a las por defecto:
   ```ini
   DATABASE_HOST=localhost
   DATABASE_PORT=5432
   DATABASE_NAME=ISW-312-PROJ1
   DATABASE_USER=tu_usuario_postgres
   DATABASE_PASSWORD=tu_contraseña_postgres
   ```

### Paso 3: Aplicar las Migraciones (Creación de Tablas)
Desde la terminal en la raíz del proyecto global (donde se encuentra el archivo `.sln`), ejecuta los siguientes comandos para crear las tablas de base de datos del módulo de Inventario y del contexto compartido:

```bash
# Crear tablas de configuración común (Empresa, CEN Counters)
dotnet ef database update --project Inventory.Api --context SharedDbContext

# Crear tablas del esquema de Inventario (Productos, Categorías, Stock)
dotnet ef database update --project Inventory.Api --context InventoryDbContext
```

### Paso 4: Poblar la Base de Datos con Datos Semilla (Seed)
1. En pgAdmin, conéctate a la base de datos recién creada **`ISW-312-PROJ1`**.
2. Abre el **Query Tool** en esta base de datos.
3. Carga y ejecuta por completo el script SQL de datos iniciales ubicado en:  
   `database/seed.sql`

### Paso 5: Ejecutar la API de Inventario
1. Desde tu terminal, navega a la carpeta del proyecto de Inventario si aun no estas ahí:
   ```bash
   cd Inventory.Api 
   ```
2. Ejecuta la aplicación:
   ```bash
   dotnet run
   ```
3. El servicio levantará en modo desarrollo y podrás acceder a la documentación interactiva en:
   * **Swagger UI (Clásico):** [http://localhost:5143/swagger](http://localhost:5143/swagger)
   * **Scalar UI (Moderno):** [http://localhost:5143/scalar/v1](http://localhost:5143/scalar/v1)
   * **Especificación OpenAPI JSON:** [http://localhost:5143/openapi/v1.json](http://localhost:5143/openapi/v1.json)
