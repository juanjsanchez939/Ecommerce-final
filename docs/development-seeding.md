# Development database seeding

The API seed is intentionally not populated with an operational password. Before running `dotnet run --project src/Ecommerce.Api -- --seed`, configure the development-only admin credentials with user-secrets:

```powershell
dotnet user-secrets set "AdminSeed:Email" "admin@example.test" --project src/Ecommerce.Api
dotnet user-secrets set "AdminSeed:Password" "<development-password>" --project src/Ecommerce.Api
```

The same settings can be supplied through environment variables (`AdminSeed__Email` and `AdminSeed__Password`). If no administrator exists, both values are required, even if other users already exist. An email already used by another account is rejected; no existing account is promoted or overwritten. The sample catalog is created only when the database has no users.
