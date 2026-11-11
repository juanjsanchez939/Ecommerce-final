# Development database seeding

The API seed is intentionally not populated with an operational password. Before running `dotnet run --project src/Ecommerce.Api -- --seed`, configure the development-only admin credentials with user-secrets:

```powershell
dotnet user-secrets set "AdminSeed:Email" "admin@example.test" --project src/Ecommerce.Api
dotnet user-secrets set "AdminSeed:Password" "<development-password>" --project src/Ecommerce.Api
```

The same settings can be supplied through environment variables (`AdminSeed__Email` and `AdminSeed__Password`). If either setting is missing, the seed command fails explicitly instead of creating a predictable account. These values are only read when the database is empty.
