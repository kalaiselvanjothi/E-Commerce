# ShopVerse API — ASP.NET Core 8 Backend

Premium e-commerce REST API built with Clean Architecture.

## Prerequisites
- .NET 8 SDK
- PostgreSQL 14+
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## Quick Start

### 1. Clone & Configure
```bash
cd ecommerce-api/src/ShopVerse.API
cp appsettings.json appsettings.Development.json
```

Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=shopverse_db;Username=YOUR_USER;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyAtLeast32CharsLong!",
    "Issuer": "ShopVerseAPI",
    "Audience": "ShopVerseClient"
  }
}
```

### 2. Create Database & Run Migrations
```bash
# From solution root
dotnet ef database update --project src/ShopVerse.Infrastructure --startup-project src/ShopVerse.API
```

### 3. Run the API
```bash
dotnet run --project src/ShopVerse.API
```

The API starts at `https://localhost:7xxx` and seeds demo data automatically.

### 4. Swagger UI
Visit `https://localhost:7xxx/swagger`

## Demo Accounts
| Role     | Email                  | Password        |
|----------|------------------------|-----------------|
| Admin    | admin@shopverse.com    | Admin@123456    |
| Customer | john@example.com       | Customer@123456 |
| Customer | priya@example.com      | Customer@123456 |

## Architecture
```
ShopVerse.sln
└── src/
    ├── ShopVerse.Domain/          # Entities, Enums, Interfaces
    ├── ShopVerse.Application/     # DTOs, Service Interfaces, Business Logic
    ├── ShopVerse.Infrastructure/  # EF Core, Repositories, External Services
    └── ShopVerse.API/             # Controllers, Middleware, Program.cs
```

## Dependency Direction
```
API → Application → Domain ← Infrastructure
```

## Tech Stack
- ASP.NET Core 8 Web API
- Entity Framework Core 8 + Npgsql (PostgreSQL)
- ASP.NET Identity + JWT (access + refresh tokens)
- Swashbuckle/OpenAPI (Swagger UI)
- Clean Architecture (Domain / Application / Infrastructure / API)
