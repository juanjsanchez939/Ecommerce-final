# Trabajo final: e-commerce y PaymentService

Se eligió **PaymentService**: aprueba montos positivos menores a 100000 y rechaza desde 100000. Ambas aplicaciones apuntan a .NET 8 y viven en un único repositorio:

```text
Ecommerce-final/
  Ecommerce.sln        solución del e-commerce
  src/                 capas del e-commerce
  tests/               pruebas del e-commerce
  PaymentService/      segundo servicio, con su solución, capas y pruebas
```

La carpeta `PaymentService` dentro de este repositorio es la versión para entregar. Si existe una copia anterior fuera del repositorio, no forma parte de la entrega.

## Preparación

Requiere SDK .NET 8, con acceso a los paquetes NuGet. En PowerShell, desde `Ecommerce-final`, configurá secretos locales propios (no usar estas credenciales en producción):

```powershell
dotnet user-secrets set "JwtSettings:SecretKey" "<clave-local-aleatoria-de-al-menos-32-caracteres>" --project .\src\Ecommerce.Api
dotnet user-secrets set "AdminSeed:Email" "admin@example.test" --project .\src\Ecommerce.Api
dotnet user-secrets set "AdminSeed:Password" "<contraseña-local-de-prueba>" --project .\src\Ecommerce.Api
dotnet test .\Ecommerce.sln
```

El administrador de prueba es el email y contraseña que configuraste. `--seed` aplica las migraciones SQLite y crea ese Admin si no existe ninguno; si el email ya pertenece a un cliente, falla sin modificarlo. Si ya hay usuarios, no agrega el catálogo de ejemplo. La base predeterminada es `src\Ecommerce.Api\Ecommerce.db`; configurá `ConnectionStrings:DefaultConnection` para cambiarla. No se eliminan bases existentes.

## Levantar los dos servicios

En una terminal, desde la raíz `Ecommerce-final`:

```powershell
dotnet test .\PaymentService\PaymentService.sln
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project .\PaymentService\src\PaymentService.Api --no-launch-profile --urls http://localhost:5100
```

En otra terminal, desde `Ecommerce-final`:

```powershell
dotnet run --project .\src\Ecommerce.Api -- --seed
```

Swagger: `http://localhost:5100/swagger` para pagos y `http://localhost:5049/swagger` para e-commerce. `--seed` mantiene la API ejecutándose; en arranques posteriores se puede omitir. La URL de pagos se configura en `src\Ecommerce.Api\appsettings.json`, sección `PaymentService:BaseUrl`. Si cambia el puerto, actualizala. Swagger está habilitado en Development.

## Probar el recorrido en Swagger

1. En e-commerce, usar `POST /api/auth/register` con email, password, firstName y lastName. Copiar `data.token` y pulsar **Authorize**; también se puede obtener desde `POST /api/auth/login`.
2. Consultar `GET /api/products`, copiar un `id` y agregarlo con `POST /api/cart/items`: `{ "productId": "<id>", "quantity": 1 }`.
3. Crear la orden con `POST /api/orders`: `{ "street": "1", "city": "San Juan", "state": "San Juan", "country": "Argentina", "zipCode": "5400" }`. Consultar `GET /api/orders` o `GET /api/orders/{id}`: con total menor a 100000 queda `Paid`.
4. Para probar rechazo, iniciar sesión con el Admin configurado, autorizar su token y crear con `POST /api/products` un producto con `price: 100000`, `stock: 10` y un `categoryId` de `GET /api/categories`. Volver a autorizar el token del cliente, agregar ese producto al carrito y crear la orden: devuelve 400 y queda `PaymentRejected`. Se devuelve el stock y el carrito permanece.
5. Con un producto en el carrito, detener PaymentService y crear otra orden: devuelve 503 con el id y queda `Pending`, con stock reservado y el carrito intacto. Consultar el id en `GET /api/orders`. **No volver a enviar la orden automáticamente**: un timeout o error de red no prueba que el pago no se haya procesado. No hay idempotencia ni mecanismo de conciliación automática; requiere revisión manual y el resultado puede ser irrecuperable si se reinició PaymentService.
6. Para seguridad, sin token el endpoint de creación de productos devuelve 401; con token de cliente devuelve 403; con token Admin permite crear. No se asigna Admin mediante registro público.

## Para explicar las capas

En cada servicio, **Domain** contiene reglas y entidades; **Application** define commands/queries, handlers y contratos sin depender de Infrastructure; **Infrastructure** implementa persistencia y comunicación; **Api** expone endpoints. En e-commerce, `CreateOrderHandler` guarda la orden y llama al cliente HTTP tipado registrado con `IHttpClientFactory`; procesa la respuesta y guarda el estado. En PaymentService, `ProcessPaymentHandler` usa MediatR y delega la regla a `Payment.Process`. Los contratos HTTP son DTOs, no entidades.

E-commerce usa SQLite y migraciones EF Core 8.0.11. PaymentService usa memoria y pierde sus pagos al reiniciarse. E-commerce usa JWT con rol; PaymentService no exige token entre servicios para simplificar la demostración local, por lo que no debe exponerse públicamente. No se implementaron reintentos HTTP automáticos.
