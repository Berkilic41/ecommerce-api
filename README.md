# Mini E-Commerce REST API

ASP.NET Core 8 Web API with SQL Server, ADO.NET (no ORM), JWT authentication, and a vanilla JS frontend.

## Tech Stack

- **Backend:** ASP.NET Core 8, C# 12
- **Database:** SQL Server / LocalDB, ADO.NET raw SQL
- **Auth:** JWT (15-min access token) + Refresh Token (7-day, DB-stored)
- **Docs:** Swagger / OpenAPI
- **Frontend:** Vanilla JS, single HTML file

---

## Setup

### 1. Database

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i Database/001_Schema.sql
sqlcmd -S "(localdb)\mssqllocaldb" -i Database/002_SeedData.sql
```

### 2. Run

```bash
cd ECommerceAPI
dotnet run
```

- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- Frontend: `http://localhost:5000`

### 3. Create an Admin user

Register normally via `POST /api/auth/register`, then update the role in the DB:

```sql
USE ECommerceDb;
UPDATE Users SET Role = 'Admin' WHERE Email = 'your@email.com';
```

---

## API Endpoints

### Auth — `/api/auth`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/register` | — | Register new user |
| POST | `/login` | — | Login, returns token pair |
| POST | `/refresh` | — | Refresh access token |
| POST | `/logout` | Bearer | Revoke refresh token |

**Register / Login response:**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "abc123...",
  "username": "johndoe",
  "email": "john@example.com",
  "role": "User"
}
```

---

### Categories — `/api/categories`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/` | — | List all categories |
| GET | `/{id}` | — | Get category by ID |
| POST | `/` | Admin | Create category |
| PUT | `/{id}` | Admin | Update category |
| DELETE | `/{id}` | Admin | Delete category |

---

### Products — `/api/products`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/` | — | List products (paginated) |
| GET | `/{id}` | — | Get product by ID |
| POST | `/` | Admin | Create product |
| PUT | `/{id}` | Admin | Update product |
| DELETE | `/{id}` | Admin | Soft-delete product |

**Query params for GET `/`:**

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `search` | string | — | Search in name and description |
| `categoryId` | int | — | Filter by category |
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page (max 100) |

---

### Cart — `/api/cart` (requires auth)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/` | Get current user's cart |
| POST | `/items` | Add item (body: `{ productId, quantity }`) |
| PUT | `/items/{productId}` | Set exact quantity |
| DELETE | `/items/{productId}` | Remove item |
| DELETE | `/` | Clear entire cart |

---

### Orders — `/api/orders` (requires auth)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/` | User | Get own order history |
| GET | `/{id}` | User/Admin | Get order details |
| POST | `/` | User | Place order from cart |
| PUT | `/{id}/status` | Admin | Update order status |

**Valid order statuses:** `Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled`

**Place order body:**
```json
{ "shippingAddress": "123 Main St, City, Country" }
```

---

## Error Responses

All errors follow this format:

```json
{
  "error": "Human-readable error message",
  "statusCode": 400
}
```

| Code | When |
|------|------|
| 400 | Validation error, bad request, insufficient stock |
| 401 | Invalid / expired token, wrong credentials |
| 404 | Resource not found |
| 500 | Unexpected server error |

---

## Project Structure

```
ECommerceAPI/
├── Database/
│   ├── 001_Schema.sql          # Table definitions
│   └── 002_SeedData.sql        # Sample categories + products
└── ECommerceAPI/
    ├── Controllers/             # HTTP endpoints
    ├── Data/                    # DbConnectionFactory
    ├── DTOs/                    # Request / Response records
    ├── Middleware/              # Global exception handler
    ├── Models/                  # Domain entities
    ├── Repositories/            # ADO.NET data access
    │   └── Interfaces/
    ├── Services/                # Business logic
    │   └── Interfaces/
    ├── wwwroot/
    │   └── index.html           # Single-file vanilla JS frontend
    ├── Program.cs
    └── appsettings.json
```
