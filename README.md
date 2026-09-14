# OrderingSystem API

A .NET backend API for a simple customer ordering system: customers can register, sign in, place orders, and manage them, while admins can view every order. Built with **ASP.NET Core 10**, **Entity Framework Core**, **SQL Server**, and **JWT authentication**.

This is the backend for the [OrderingSystem client](https://github.com/mohamedshehatadev17/ordersytemclient).

## Features

- Customer registration and login (ASP.NET Core Identity)
- JWT access tokens + rotating refresh tokens
- Role-based access (`Admin` / `Customer`)
- Order management: create, view, update, and soft-delete orders
- Admins can view all orders or a specific customer's orders
- Anti-abuse rule: deleting 3+ orders in the same day temporarily bans a customer from placing new orders for 6 hours
- Account lockout after repeated failed login attempts
- Swagger / OpenAPI docs with JWT bearer auth support

## Architecture

The solution follows a **Clean Architecture** layout:

```
OrderingSystem.API             # Controllers, DTOs, middleware, app startup
OrderingSystem.Application     # Application layer (currently minimal)
OrderingSystem.Domain          # Entities, enums, and repository interfaces
OrderingSystem.Infrastructure  # EF Core, Identity, JWT, repositories, migrations
```

- **Domain** — `Customer`, `Order`, `RefreshToken` entities and repository interfaces
- **Infrastructure** — `ApplicationDbContext`, Identity setup, JWT token service, EF Core migrations, identity seeder
- **API** — REST controllers (`AuthController`, `OrdersController`, `CustomersController`), global exception handling, Swagger

## Tech Stack

- ASP.NET Core 10 (Web API)
- Entity Framework Core 10 + SQL Server
- ASP.NET Core Identity
- JWT Bearer authentication
- Swashbuckle (Swagger UI)

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A SQL Server instance (LocalDB, SQL Express, or a hosted SQL Server)

### 1. Clone the repo

```bash
git clone https://github.com/mohamedshehatadev17/OrderingSystem.git
cd OrderingSystem
```

### 2. Configure your connection string and JWT settings

Set these via `dotnet user-secrets` (recommended) or environment variables rather than committing them to `appsettings.json`:

```bash
cd OrderingSystem.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:orderDB" "Server=YOUR_SERVER;Database=OrderingSystemDb;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "your-own-secret-key"
```
### 3. Apply migrations

```bash
dotnet ef database update --project OrderingSystem.Infrastructure --startup-project OrderingSystem.API
```

### 4. Run the API

```bash
dotnet run --project OrderingSystem.API
```

Swagger UI will be available at `/swagger` once the app is running.

On first run, the app seeds two roles (`Admin`, `Customer`) and a default admin account (see `IdentitySeeder.cs`). Change this account's password immediately in any non-local environment.

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | — | Register a new customer |
| POST | `/api/auth/login` | — | Log in, returns access + refresh tokens |
| POST | `/api/auth/refresh` | — | Exchange a refresh token for a new access token |
| GET | `/api/orders` | Customer | Get the current customer's orders |
| GET | `/api/orders/{id}` | Customer | Get one of the current customer's orders |
| POST | `/api/orders` | Customer | Place a new order |
| PUT | `/api/orders/{id}` | Customer | Update an order (amount/status) |
| DELETE | `/api/orders/{orderId}` | Customer | Soft-delete an order |
| GET | `/api/orders/all` | Admin | Get all orders |
| GET | `/api/orders/customers/{customerId}` | Admin | Get a specific customer's orders |

## License

No license specified.
