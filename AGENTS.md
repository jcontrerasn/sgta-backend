# Guía de arquitectura y flujo del backend

Este backend utiliza Clean Architecture para separar la API HTTP, la lógica de aplicación, el dominio y el acceso a datos. Todos los endpoints nuevos deben seguir esta estructura.

## Estructura de proyectos

```text
sgta-backend/
├── Controllers/                         # Entrada HTTP de la API
├── SGTA.Application/
│   ├── Features/{Entidad}/
│   │   ├── DTOs/                         # Requests y responses
│   │   └── {Entidad}Service.cs           # Casos de uso y reglas de aplicación
│   └── Interfaces/                       # Contratos de repositorios
├── SGTA.Domain/
│   └── Entities/                         # Entidades y reglas propias del dominio
└── SGTA.Infrastructure/
    ├── Persistence/                     # DbContext y configuración EF Core
    └── Repositories/                    # Implementaciones de persistencia
```

## Dependencias permitidas

Las dependencias deben apuntar hacia el centro de la arquitectura:

```text
API (sgta-backend) -> Application -> Domain
API (sgta-backend) -> Infrastructure -> Application -> Domain
```

- `SGTA.Domain` no depende de ningún proyecto.
- `SGTA.Application` puede depender de `SGTA.Domain`, pero no de Entity Framework ni de `Infrastructure`.
- `SGTA.Infrastructure` implementa los contratos de `Application` y puede usar Entity Framework Core.
- `Controllers` no deben acceder directamente a `DbContext` ni a repositorios concretos.
- No colocar lógica de negocio, consultas SQL o `SaveChangesAsync` dentro de los controladores.

## Flujo estándar de un endpoint

```text
HTTP request
    ↓
Controller
    ↓
Service / caso de uso
    ↓
Interface del repositorio
    ↓
Repositorio de Infrastructure
    ↓
SgtaDbContext / SQL Server
    ↓
Response DTO
```

El flujo de respuesta debe regresar por las mismas capas en sentido inverso. Los controladores reciben y devuelven DTOs; no deben exponer entidades de dominio directamente.

## Cómo crear un endpoint nuevo

Para una entidad nueva llamada `Producto`, seguir estos pasos:

### 1. Crear la entidad en Domain

Archivo: `SGTA.Domain/Entities/Producto.cs`

La entidad representa el modelo de negocio y no debe contener referencias a ASP.NET Core, EF Core ni DTOs.

### 2. Crear los DTOs en Application

Ubicación: `SGTA.Application/Features/Productos/DTOs/`

Usar nombres explícitos:

- `CrearProductoRequest.cs` para datos de entrada.
- `ActualizarProductoRequest.cs` para modificaciones.
- `ProductoResponse.cs` para datos de salida.

Los DTOs no deben reutilizarse como entidades de base de datos. Usar anotaciones como `[Required]`, `[StringLength]` y `[Range]` para validaciones básicas de entrada.

### 3. Crear la interfaz del repositorio

Archivo: `SGTA.Application/Interfaces/IProductoRepository.cs`

La interfaz solo declara operaciones. Todas las operaciones asíncronas deben aceptar `CancellationToken`:

```csharp
Task<List<Producto>> ObtenerTodosAsync(
    CancellationToken cancellationToken = default);

Task<Producto?> ObtenerPorIdAsync(
    int id,
    CancellationToken cancellationToken = default);

Task<Producto> CrearAsync(
    Producto producto,
    CancellationToken cancellationToken = default);
```

### 4. Crear el servicio de aplicación

Archivo: `SGTA.Application/Features/Productos/ProductoService.cs`

El servicio debe:

- Recibir la interfaz del repositorio por inyección de dependencias.
- Aplicar reglas de negocio.
- Convertir `Request DTO` a entidad.
- Convertir entidades a `Response DTO`.
- No conocer detalles de EF Core o SQL Server.

Ejemplo para obtener datos:

```csharp
public async Task<List<ProductoResponse>> ObtenerTodosAsync(
    CancellationToken cancellationToken = default)
{
    var productos = await _repository.ObtenerTodosAsync(cancellationToken);

    return productos.Select(producto => new ProductoResponse
    {
        Id = producto.Id,
        Nombre = producto.Nombre
    }).ToList();
}
```

### 5. Implementar el repositorio en Infrastructure

Archivo: `SGTA.Infrastructure/Repositories/ProductoRepository.cs`

El repositorio es el único lugar donde se deben realizar consultas con EF Core:

```csharp
return await _context.Productos
    .AsNoTracking()
    .ToListAsync(cancellationToken);
```

Para crear datos, usar `AddAsync` y `SaveChangesAsync` en el repositorio. No hacer persistencia parcial desde el controlador.

### 6. Registrar la entidad en `SgtaDbContext`

En `SGTA.Infrastructure/Persistence/SgtaDbContext.cs`:

```csharp
public DbSet<Producto> Productos => Set<Producto>();
```

Agregar también la configuración de tabla, clave, longitudes, nulabilidad y relaciones dentro de `OnModelCreating`.

### 7. Registrar las dependencias en `Program.cs`

```csharp
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<ProductoService>();
```

### 8. Crear el controlador

Archivo: `Controllers/ProductosController.cs`

El controlador debe ser delgado: recibe la petición, llama al servicio y define el código HTTP.

```csharp
[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly ProductoService _service;

    public ProductosController(ProductoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        var productos = await _service.ObtenerTodosAsync(cancellationToken);
        return Ok(productos);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearProductoRequest request,
        CancellationToken cancellationToken)
    {
        var producto = await _service.CrearAsync(
            request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, producto);
    }
}
```

## Convenciones HTTP

- `GET /api/{recurso}`: `200 OK` con una colección de `Response DTO`.
- `GET /api/{recurso}/{id}`: `200 OK` o `404 NotFound`.
- `POST /api/{recurso}`: `201 Created` con el recurso creado.
- `PUT/PATCH`: `200 OK` o `204 NoContent` cuando corresponda.
- `DELETE`: `204 NoContent`.
- Datos inválidos: `400 BadRequest`. `[ApiController]` debe encargarse de las validaciones de DataAnnotations.
- No devolver entidades EF Core directamente ni incluir mensajes de excepción internos en producción.

## Formato estandar de respuesta

Las respuestas exitosas y de error deben utilizar `ApiResponse<T>`, ubicado en `Common/Responses/ApiResponse.cs`:

```csharp
public class ApiResponse<T>
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
```

Reglas para usar `ApiResponse<T>`:

- `Status` debe coincidir con el codigo HTTP devuelto.
- `Message` debe ser claro y orientado al consumidor de la API.
- `Data` debe contener un `Response DTO`, nunca una entidad de EF Core.
- En errores, `Data` normalmente debe ser `null`.
- Las respuestas `400`, `409` y `500` tambien deben conservar este formato.

Ejemplo de respuesta exitosa:

```json
{
  "status": 201,
  "message": "Rol creado correctamente",
  "data": {
    "idRol": 1,
    "nombre": "Administrador",
    "descripcion": "Rol principal",
    "activo": true
  }
}
```

## Manejo de excepciones

El manejo global esta configurado en `Program.cs` mediante `app.UseExceptionHandler`. Los controladores no deben repetir bloques `try/catch` para errores comunes.

### Conflictos de negocio

Para duplicados u otras reglas que impidan crear el recurso, lanzar `ConflictException` desde el servicio:

```csharp
throw new ConflictException("Ya existe un rol con ese nombre.");
```

El manejador global transforma esta excepcion en HTTP `409 Conflict` y devuelve un `ApiResponse<object>` con `Status = 409`.

Antes de insertar se debe validar la existencia del registro en el repositorio. Ademas, el repositorio debe traducir una violacion de indice unico de SQL Server a `ConflictException` para cubrir condiciones de concurrencia.

### Validaciones

- `ArgumentException` se transforma en HTTP `400 BadRequest`.
- Las validaciones basicas deben estar en los `Request DTO` mediante DataAnnotations.
- Las reglas de negocio deben validarse en el servicio.

### Errores inesperados

- Cualquier excepcion no conocida se transforma en HTTP `500 InternalServerError`.
- No devolver `ex.Message`, SQL, stack traces ni detalles internos al cliente en produccion.
- Los conflictos y validaciones se registran como advertencias breves.
- Los errores inesperados se registran con stack trace para diagnostico.

## Reglas importantes

- Usar `async/await` y propagar siempre el `CancellationToken`.
- Usar `AsNoTracking()` en consultas de solo lectura.
- Validar reglas de negocio en el servicio, no únicamente en el controlador.
- Mantener nombres consistentes en español con el módulo existente: `RolService`, `IRolRepository`, `RolRepository`, `RolResponse`.
- Mantener los `Request DTO` y `Response DTO` dentro de `Features/{Entidad}/DTOs`.
- Preferir `Response DTO` en todos los endpoints para evitar acoplar la API con la base de datos.
- No guardar contraseñas ni cadenas de conexión sensibles en el repositorio.
- Usar `Server=tcp:127.0.0.1,1433` en desarrollo cuando SQL Server esté publicado en ese puerto.

## Ejemplo del módulo `Roles`

El módulo actual sigue este flujo:

```text
GET /api/roles
    -> RolesController
    -> RolService.ObtenerTodosAsync
    -> IRolRepository.ObtenerTodosAsync
    -> RolRepository
    -> SgtaDbContext.Roles
    -> List<RolResponse>

POST /api/roles
    -> RolesController con CrearRolRequest
    -> RolService.CrearAsync
    -> IRolRepository.CrearAsync
    -> RolRepository
    -> SaveChangesAsync
    -> respuesta 201 Created
```

Los endpoints nuevos deben conservar este flujo y cambiar únicamente la entidad, los DTOs, las reglas y las operaciones específicas del módulo.
