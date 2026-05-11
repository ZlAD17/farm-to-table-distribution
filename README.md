# Farm-to-Table Distribution API

ASP.NET Core 8 REST API for the Regional Farm-to-Table Distribution platform.  
Pure ADO.NET — no ORM. SQL Server back-end.

---

## Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 8.0+ |
| SQL Server | 2019 / 2022 / LocalDB / Azure SQL |

---

## Quick Start

### 1. Set up the database

Run the DDL and seed scripts against an empty SQL Server database:

```sql
-- In SSMS or sqlcmd:
CREATE DATABASE FarmToTableDb;
USE FarmToTableDb;
-- then run:
```

```bash
sqlcmd -S localhost -d FarmToTableDb -i database/schema.sql
sqlcmd -S localhost -d FarmToTableDb -i database/seed.sql
```

### 2. Configure the connection string

Open `FarmToTable.Api/appsettings.json` and update:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FarmToTableDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

For SQL auth, use:
```
Server=localhost;Database=FarmToTableDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
```

### 3. Run the API

```bash
cd FarmToTable.Api
dotnet run
```

The API starts on `https://localhost:5001` (or `http://localhost:5000`).  
Swagger UI is available at: `https://localhost:5001/swagger`

---

## Project Structure

```
farm-to-table-distribution/
├── database/
│   ├── schema.sql                  DDL — all tables, PKs, FKs, indexes
│   └── seed.sql                    Sample data for testing
└── FarmToTable.Api/
    ├── Controllers/                One controller per resource
    │   ├── FarmsController.cs
    │   ├── RestaurantsController.cs
    │   ├── DriversController.cs
    │   ├── CropsController.cs
    │   ├── HarvestBatchesController.cs
    │   ├── OrdersController.cs
    │   ├── DeliveryTripsController.cs
    │   └── ReportsController.cs
    ├── Data/
    │   └── DatabaseHelper.cs       SqlConnection factory (singleton)
    ├── Middleware/
    │   └── GlobalExceptionMiddleware.cs
    ├── Models/
    │   ├── Requests/               Inbound DTOs (validated with DataAnnotations)
    │   └── Responses/              Outbound DTOs returned to the UI
    ├── Repositories/               ADO.NET / raw SQL data access
    ├── Services/                   Business logic + error translation
    ├── appsettings.json
    └── Program.cs
```

---

## API Reference

### Farms — `/api/farms`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/farms` | List all farms |
| GET | `/api/farms/{id}` | Get farm by ID |
| POST | `/api/farms` | Create farm |
| PUT | `/api/farms/{id}` | Update farm |
| DELETE | `/api/farms/{id}` | Delete farm |

### Restaurants — `/api/restaurants`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/restaurants` | List all restaurants |
| GET | `/api/restaurants/{id}` | Get by ID |
| POST | `/api/restaurants` | Create |
| PUT | `/api/restaurants/{id}` | Update |
| DELETE | `/api/restaurants/{id}` | Delete |

### Drivers — `/api/drivers`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/drivers` | List all drivers |
| GET | `/api/drivers/{id}` | Get by ID |
| POST | `/api/drivers` | Create |
| PUT | `/api/drivers/{id}` | Update |
| DELETE | `/api/drivers/{id}` | Delete |

### Crops — `/api/crops`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/crops` | List all crop types |
| POST | `/api/crops` | Create crop type |

### Harvest Batches — `/api/harvest-batches`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/harvest-batches` | List (supports filters below) |
| GET | `/api/harvest-batches/{id}` | Get by ID |
| POST | `/api/harvest-batches` | Create batch |
| PUT | `/api/harvest-batches/{id}` | Update price / status |
| DELETE | `/api/harvest-batches/{id}` | Delete |

**Query filters for GET `/api/harvest-batches`:**

| Param | Type | Example |
|-------|------|---------|
| `farmId` | int | `?farmId=1` |
| `cropId` | int | `?cropId=2` |
| `fromDate` | yyyy-MM-dd | `?fromDate=2026-05-01` |
| `toDate` | yyyy-MM-dd | `?toDate=2026-05-31` |
| `status` | string | `?status=Available` |

### Orders — `/api/orders`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/orders` | List all orders with batch lines |
| GET | `/api/orders/{id}` | Get order + all batch lines |
| POST | `/api/orders` | Create order (multi-batch, transactional) |
| PUT | `/api/orders/{id}` | Update status / driver / notes |

**Create order body example:**
```json
{
  "restaurantId": 1,
  "driverId": 1,
  "notes": "Please deliver before noon",
  "batches": [
    { "batchId": 1, "quantityOrdered": 50 },
    { "batchId": 2, "quantityOrdered": 30 }
  ]
}
```

**Update order body example:**
```json
{
  "status": "Delivered",
  "driverId": 2,
  "notes": "Left at back door"
}
```

Valid status values: `Pending` | `Confirmed` | `Delivered` | `Cancelled`

### Delivery Trips — `/api/delivery-trips`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/delivery-trips` | List all trips |
| GET | `/api/delivery-trips/{id}` | Get by ID |
| POST | `/api/delivery-trips` | Schedule a new trip |
| PUT | `/api/delivery-trips/{id}/complete` | Mark trip as completed |

### Reports — `/api/reports`

| Method | Route | Query Params | Description |
|--------|-------|--------------|-------------|
| GET | `/api/reports/top-crop` | `from`, `to` (dates) | Crop with most orders in date range |
| GET | `/api/reports/inactive-farms` | `year`, `month` | Farms with no batches in a month |
| GET | `/api/reports/top-driver` | `year`, `month` | Driver with most trips in a month |
| GET | `/api/reports/inactive-restaurants` | `year`, `month` | Restaurants with no orders in a month |
| GET | `/api/reports/restaurant-batches` | `year`, `month` | Batches delivered per restaurant in a month |
| GET | `/api/reports/farm-revenue` | _(none)_ | Total revenue per farm |

**Example report requests:**
```
GET /api/reports/top-crop?from=2026-05-01&to=2026-05-31
GET /api/reports/inactive-farms?year=2026&month=5
GET /api/reports/top-driver?year=2026&month=5
GET /api/reports/farm-revenue
```

---

## Testing with curl

```bash
# Create a farm
curl -X POST https://localhost:5001/api/farms \
  -H "Content-Type: application/json" \
  -d '{"name":"Sunny Acres","location":"Denver, CO","contactEmail":"hi@sunny.com"}'

# List harvest batches for farm 1, status Available
curl "https://localhost:5001/api/harvest-batches?farmId=1&status=Available"

# Place an order with two batch lines
curl -X POST https://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{"restaurantId":1,"batches":[{"batchId":1,"quantityOrdered":10},{"batchId":3,"quantityOrdered":5}]}'

# Revenue report
curl https://localhost:5001/api/reports/farm-revenue
```

---

## Database Schema Overview

```
Farm ──< HarvestBatch >── Crop
                │
                └──< OrderBatch >── PurchaseOrder ──< Restaurant
                                         │
                                    DeliveryTrip
                                         │
                                       Driver
```

Key design decisions:
- **`OrderBatch`** is the M:N bridge between `PurchaseOrder` and `HarvestBatch` — each order can reference many batches.
- `HarvestBatch.QuantityRemaining` is decremented atomically inside a SQL transaction when an order is created.
- `OrderBatch.UnitPrice` snapshots the price at order time so future price changes do not affect historical orders.
- Status columns are stored as `NVARCHAR(20)` with `CHECK` constraints for readability and integrity.

---

## Error Responses

All errors return JSON in the form:

```json
{
  "error": "KeyNotFoundException",
  "message": "Farm 99 not found."
}
```

| HTTP Code | Cause |
|-----------|-------|
| 400 | Validation failure / bad input |
| 404 | Resource not found |
| 409 | Business rule conflict (e.g. insufficient batch stock) |
| 500 | Unexpected server error |
