# Clínica - Proyecto Base

Este repositorio contiene la base para implementar un sistema integral de clínica utilizando **Vue 3 + TypeScript** para el frontend, **.NET Minimal API** para el backend y **SQL Server 2014** para la base de datos.

## Estructura del repositorio

```
├── backend/        # API minimal con documentación Swagger UI
├── frontend/       # Aplicación SPA con Vue 3 + TypeScript (Vite)
├── db-scripts/     # Scripts de base de datos (tablas, stored procedures)
└── README.md
```

## Backend (.NET Minimal API)

- Proyecto ubicado en `backend/`.
- Swagger UI disponible inmediatamente en la ruta raíz (`/`).
- Clase `Resultado` para estandarizar las respuestas del servicio con las propiedades `value`, `message` y `data`.
- Integración preparada para ejecutar stored procedures que devuelvan los parámetros `@pResultado` y `@pMsg`.
- Configurar la cadena de conexión en `backend/appsettings.Development.json` o mediante variables de entorno.

### Endpoints principales

| Recurso    | Ruta base        | Descripción                                           |
|------------|------------------|-------------------------------------------------------|
| Médicos    | `/api/medicos`   | CRUD completo sobre `CatMedicos`                      |
| Pacientes  | `/api/pacientes` | CRUD completo sobre `CatPacientes`                    |
| Usuarios   | `/api/usuarios`  | CRUD completo sobre `CatUsuarios`                     |
| Consultas  | `/api/consultas` | CRUD completo sobre `Consultas`                       |
| Demo       | `/api/demo`      | Ejemplo de respuesta utilizando la clase `Resultado`  |

Cada operación consume los stored procedures definidos en `db-scripts/001_init.sql` y mapea los parámetros de salida `@pResultado` y `@pMsg` hacia la clase `Resultado`.

## Frontend (Vue 3 + TypeScript)

- Proyecto localizado en `frontend/` creado con Vite.
- Servicio HTTP centralizado en `src/services/api.ts` que interpreta la clase `Resultado`.
- Incluye componentes y estilos base listos para ampliarse con vistas CRUD.
- Ejecuta `npm install` y `npm run dev` dentro del directorio `frontend/` para iniciar el entorno de desarrollo (necesita acceso a internet).

## Base de datos (SQL Server 2014)

- Script `db-scripts/001_init.sql` crea la base de datos `ClinicaDB`, tablas principales, y stored procedures con la nomenclatura solicitada.
- Todos los procedimientos almacenados incluyen `SET NOCOUNT ON`, parámetros `@pResultado` y `@pMsg`, y manejo de transacciones para operaciones `INSERT`, `UPDATE` y `DELETE`.

## Configuración inicial

1. Ejecuta el script `db-scripts/001_init.sql` en tu instancia de SQL Server 2014.
2. Ajusta la cadena de conexión en `backend/appsettings.Development.json` o mediante variables de entorno.
3. (Opcional) Restaura los paquetes ejecutando `dotnet restore` dentro de `backend/`.
4. En el frontend ejecuta `npm install` y posteriormente `npm run dev`.

> Nota: En este entorno de ejemplo no se ejecutaron los comandos que requieren acceso a internet (como `npm install` o `dotnet restore`).

## Licencia

Distribuido con fines educativos.
