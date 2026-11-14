# PaymentService

Servicio HTTP independiente de pagos para el e-commerce del mismo repositorio. La guía completa para ejecutar ambos servicios y reproducir los flujos está en `..\README.md`.

## Arquitectura

- **Api** recibe el DTO y envía el command mediante `ISender` de MediatR.
- **Application** define `ProcessPaymentRequest`, su handler y la interfaz `IPaymentRepository`.
- **Domain** contiene `Payment` y la regla: aprueba un monto positivo menor a 100000; desde 100000 lo rechaza. Un `orderId` vacío o monto no positivo devuelve 400.
- **Infrastructure** implementa el repositorio en memoria. Los registros se pierden al reiniciar: no usa EF Core ni migraciones.

La API devuelve `{ "status": "Approved" | "Rejected", "transactionId": "..." }`. Es una simulación sin autenticación entre servicios: usar solamente en un entorno local confiable; no exponerla públicamente.

## Ejecutar y probar

Con SDK .NET 8 o posterior, desde esta carpeta:

```powershell
dotnet test .\PaymentService.sln
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project .\src\PaymentService.Api --no-launch-profile --urls http://localhost:5100
```

Swagger: `http://localhost:5100/swagger`. El argumento de puerto es importante: el perfil predeterminado del proyecto usa otro puerto.

Ejemplo para `POST /api/payments/process`:

```json
{ "orderId": "a1b20000-0000-0000-0000-000000000001", "amount": 45000 }
```

No hay idempotencia ni consulta de transacciones. Si una respuesta HTTP se pierde, no se puede asumir que el pago no se procesó ni reenviar la solicitud de forma segura.
