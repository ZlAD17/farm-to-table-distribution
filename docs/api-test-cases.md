# Farm-to-Table Distribution API — Scenario-Based Test Cases

> **Base URL:** `http://localhost:5000`  
> **Content-Type:** `application/json` (all POST / PUT requests)  
> **Tool:** curl (all examples copy-paste ready)

---

## Scenario Overview

The tests follow a realistic end-to-end storyline:

| Phase | What happens |
|-------|-------------|
| **1 — Farms** | Register the farms that supply produce |
| **2 — Crops** | Define the crop types those farms grow |
| **3 — Restaurants** | Register the restaurants that place orders |
| **4 — Drivers** | Register the delivery drivers |
| **5 — Harvest Batches** | Farms post available batches; filters are exercised |
| **6 — Orders** | Restaurants place multi-batch orders; stock guard tested |
| **7 — Delivery Trips** | Drivers are assigned; trips are completed |
| **8 — Reports** | Management runs all analytical queries |
| **9 — Edge Cases** | Validation failures, 404s, business-rule conflicts |

> IDs created in earlier phases are referenced in later phases exactly as returned by the API.

---

## Phase 1 — Farms

### 1.1 — Create farms (POST /api/farms)

**TC-F-01 — Create a valid farm**
```bash
curl -s -X POST http://localhost:5000/api/farms \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Green Valley Farm",
    "location": "Springfield, IL",
    "contactEmail": "contact@greenvalley.com",
    "contactPhone": "555-1001"
  }'
```
| Field | Expected |
|-------|----------|
| HTTP status | `201 Created` |
| `farmId` | auto-generated integer (e.g. `1`) |
| `name` | `"Green Valley Farm"` |
| `location` | `"Springfield, IL"` |
| `contactEmail` | `"contact@greenvalley.com"` |
| `contactPhone` | `"555-1001"` |
| `createdAt` | UTC timestamp, not null |

---

**TC-F-02 — Create a farm with no optional fields**
```bash
curl -s -X POST http://localhost:5000/api/farms \
  -H "Content-Type: application/json" \
  -d '{"name": "Silent Acres", "location": "Tucson, AZ"}'
```
| Field | Expected |
|-------|----------|
| HTTP status | `201 Created` |
| `contactEmail` | `null` |
| `contactPhone` | `null` |

---

**TC-F-03 — Create second and third farms (needed for later phases)**
```bash
# Farm 2
curl -s -X POST http://localhost:5000/api/farms \
  -H "Content-Type: application/json" \
  -d '{"name":"Sunrise Organics","location":"Portland, OR","contactEmail":"hello@sunriseorganics.com","contactPhone":"555-1002"}'

# Farm 3
curl -s -X POST http://localhost:5000/api/farms \
  -H "Content-Type: application/json" \
  -d '{"name":"Meadow Creek Ranch","location":"Austin, TX","contactEmail":"info@meadowcreek.com","contactPhone":"555-1003"}'
```
Expected: `201 Created` for each.

---

### 1.2 — Read farms (GET /api/farms)

**TC-F-04 — List all farms**
```bash
curl -s http://localhost:5000/api/farms
```
| Field | Expected |
|-------|----------|
| HTTP status | `200 OK` |
| Body | JSON array, length ≥ 3 |
| Order | Alphabetical by `name` |

---

**TC-F-05 — Get a single farm by ID**
```bash
curl -s http://localhost:5000/api/farms/1
```
| Field | Expected |
|-------|----------|
| HTTP status | `200 OK` |
| `farmId` | `1` |
| `name` | `"Green Valley Farm"` |

---

**TC-F-06 — Get a farm that does not exist**
```bash
curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/api/farms/9999
```
| Field | Expected |
|-------|----------|
| HTTP status | `404 Not Found` |
| Body | `{"message":"Farm 9999 not found."}` |

---

### 1.3 — Update farm (PUT /api/farms/{id})

**TC-F-07 — Update an existing farm**
```bash
curl -s -X PUT http://localhost:5000/api/farms/2 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Sunrise Organics Co.",
    "location": "Portland, OR",
    "contactEmail": "new@sunriseorganics.com",
    "contactPhone": "555-2222"
  }'
```
| Field | Expected |
|-------|----------|
| HTTP status | `200 OK` |
| `name` | `"Sunrise Organics Co."` |
| `contactEmail` | `"new@sunriseorganics.com"` |
| `createdAt` | unchanged (same as original) |

---

**TC-F-08 — Update a farm that does not exist**
```bash
curl -s -X PUT http://localhost:5000/api/farms/9999 \
  -H "Content-Type: application/json" \
  -d '{"name":"Ghost Farm","location":"Nowhere"}'
```
| Field | Expected |
|-------|----------|
| HTTP status | `404 Not Found` |

---

### 1.4 — Delete farm (DELETE /api/farms/{id})

**TC-F-09 — Delete a farm**
```bash
# First create a throwaway farm
FARM_ID=$(curl -s -X POST http://localhost:5000/api/farms \
  -H "Content-Type: application/json" \
  -d '{"name":"Delete Me Farm","location":"Nowhere, NA"}' | python -c "import sys,json; print(json.load(sys.stdin)['farmId'])")

curl -s -o /dev/null -w "%{http_code}" -X DELETE http://localhost:5000/api/farms/$FARM_ID
```
| Field | Expected |
|-------|----------|
| HTTP status | `204 No Content` |
| Body | empty |

---

**TC-F-10 — Delete a farm that does not exist**
```bash
curl -s -X DELETE http://localhost:5000/api/farms/9999
```
| Field | Expected |
|-------|----------|
| HTTP status | `404 Not Found` |

---

### 1.5 — Validation failures (Farms)

**TC-F-11 — Missing required `name`**
```bash
curl -s -X POST http://localhost:5000/api/farms \
  -H "Content-Type: application/json" \
  -d '{"location":"Somewhere, TX"}'
```
| Field | Expected |
|-------|----------|
| HTTP status | `400 Bad Request` |
| Body | validation errors object mentioning `Name` |

---

**TC-F-12 — Missing required `location`**
```bash
curl -s -X POST http://localhost:5000/api/farms \
  -H "Content-Type: application/json" \
  -d '{"name":"Nameless Farm"}'
```
| Expected | `400 Bad Request` mentioning `Location` |
|---------|----------------------------------------|

---

**TC-F-13 — `name` exceeds 150 characters**
```bash
curl -s -X POST http://localhost:5000/api/farms \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"$(python -c "print('A'*151)")\",\"location\":\"Somewhere\"}"
```
| Expected | `400 Bad Request` |
|---------|-------------------|

---

## Phase 2 — Crops

### 2.1 — Create crops (POST /api/crops)

**TC-C-01 — Create all crop types needed for the scenario**
```bash
curl -s -X POST http://localhost:5000/api/crops \
  -H "Content-Type: application/json" \
  -d '{"name":"Tomatoes","unit":"kg"}'

curl -s -X POST http://localhost:5000/api/crops \
  -H "Content-Type: application/json" \
  -d '{"name":"Lettuce","unit":"kg"}'

curl -s -X POST http://localhost:5000/api/crops \
  -H "Content-Type: application/json" \
  -d '{"name":"Carrots","unit":"kg"}'

curl -s -X POST http://localhost:5000/api/crops \
  -H "Content-Type: application/json" \
  -d '{"name":"Strawberries","unit":"kg"}'

curl -s -X POST http://localhost:5000/api/crops \
  -H "Content-Type: application/json" \
  -d '{"name":"Corn","unit":"dozen"}'
```
| Field | Expected |
|-------|----------|
| HTTP status | `201 Created` for each |
| `cropId` | sequential integers 1–5 |
| `unit` | exactly as provided |

---

**TC-C-02 — List all crops**
```bash
curl -s http://localhost:5000/api/crops
```
| Field | Expected |
|-------|----------|
| HTTP status | `200 OK` |
| Count | 5 crops |
| Order | alphabetical by `name` |

---

**TC-C-03 — Create crop with missing `name`**
```bash
curl -s -X POST http://localhost:5000/api/crops \
  -H "Content-Type: application/json" \
  -d '{"unit":"kg"}'
```
| Expected | `400 Bad Request` |
|---------|-------------------|

---

**TC-C-04 — Create crop with missing `unit`**
```bash
curl -s -X POST http://localhost:5000/api/crops \
  -H "Content-Type: application/json" \
  -d '{"name":"Peppers"}'
```
| Expected | `400 Bad Request` |
|---------|-------------------|

---

## Phase 3 — Restaurants

### 3.1 — Create restaurants (POST /api/restaurants)

**TC-R-01 — Create three restaurants**
```bash
curl -s -X POST http://localhost:5000/api/restaurants \
  -H "Content-Type: application/json" \
  -d '{"name":"The Garden Table","address":"123 Elm St, Chicago, IL","contactEmail":"orders@gardentable.com","contactPhone":"555-2001"}'

curl -s -X POST http://localhost:5000/api/restaurants \
  -H "Content-Type: application/json" \
  -d '{"name":"Farm Bistro","address":"456 Oak Ave, Portland, OR","contactEmail":"info@farmbistro.com","contactPhone":"555-2002"}'

curl -s -X POST http://localhost:5000/api/restaurants \
  -H "Content-Type: application/json" \
  -d '{"name":"Root & Branch","address":"789 Pine Rd, Austin, TX","contactEmail":"hello@rootandbranch.com","contactPhone":"555-2003"}'
```
| Field | Expected |
|-------|----------|
| HTTP status | `201 Created` each |
| `restaurantId` | 1, 2, 3 |

---

**TC-R-02 — List all restaurants**
```bash
curl -s http://localhost:5000/api/restaurants
```
| Expected | `200 OK`, array of 3 |
|---------|----------------------|

---

**TC-R-03 — Get restaurant by ID**
```bash
curl -s http://localhost:5000/api/restaurants/1
```
| Field | Expected |
|-------|----------|
| HTTP status | `200 OK` |
| `name` | `"Farm Bistro"` *(alphabetical order — id may vary)* |

---

**TC-R-04 — Get restaurant that does not exist**
```bash
curl -s http://localhost:5000/api/restaurants/9999
```
| Expected | `404 Not Found` with `message` field |
|---------|--------------------------------------|

---

**TC-R-05 — Update restaurant contact info**
```bash
curl -s -X PUT http://localhost:5000/api/restaurants/1 \
  -H "Content-Type: application/json" \
  -d '{"name":"The Garden Table","address":"123 Elm St, Chicago, IL","contactEmail":"newemail@gardentable.com","contactPhone":"555-9999"}'
```
| Field | Expected |
|-------|----------|
| HTTP status | `200 OK` |
| `contactEmail` | `"newemail@gardentable.com"` |

---

**TC-R-06 — Delete a restaurant**
```bash
REST_ID=$(curl -s -X POST http://localhost:5000/api/restaurants \
  -H "Content-Type: application/json" \
  -d '{"name":"Temp Cafe","address":"1 Test St"}' | python -c "import sys,json; print(json.load(sys.stdin)['restaurantId'])")

curl -s -o /dev/null -w "%{http_code}" -X DELETE http://localhost:5000/api/restaurants/$REST_ID
```
| Expected | `204 No Content` |
|---------|-----------------|

---

**TC-R-07 — Missing required `address`**
```bash
curl -s -X POST http://localhost:5000/api/restaurants \
  -H "Content-Type: application/json" \
  -d '{"name":"No Address Cafe"}'
```
| Expected | `400 Bad Request` |
|---------|-------------------|

---

## Phase 4 — Drivers

### 4.1 — Create drivers (POST /api/drivers)

**TC-D-01 — Create three drivers**
```bash
curl -s -X POST http://localhost:5000/api/drivers \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Alice Johnson","phone":"555-3001","licensePlate":"IL-ABC-123"}'

curl -s -X POST http://localhost:5000/api/drivers \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Bob Martinez","phone":"555-3002","licensePlate":"OR-XYZ-789"}'

curl -s -X POST http://localhost:5000/api/drivers \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Carol Smith","phone":"555-3003","licensePlate":"TX-DEF-456"}'
```
| Field | Expected |
|-------|----------|
| HTTP status | `201 Created` each |
| `driverId` | 1, 2, 3 |
| `createdAt` | not null |

---

**TC-D-02 — Create driver with no optional fields**
```bash
curl -s -X POST http://localhost:5000/api/drivers \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Dave No-Phone"}'
```
| Expected | `201 Created`, `phone: null`, `licensePlate: null` |
|---------|---------------------------------------------------|

---

**TC-D-03 — List all drivers**
```bash
curl -s http://localhost:5000/api/drivers
```
| Expected | `200 OK`, array ≥ 3, ordered by `fullName` |
|---------|---------------------------------------------|

---

**TC-D-04 — Get driver by ID**
```bash
curl -s http://localhost:5000/api/drivers/1
```
| Expected | `200 OK` with correct `fullName` |
|---------|----------------------------------|

---

**TC-D-05 — Get driver that does not exist**
```bash
curl -s http://localhost:5000/api/drivers/9999
```
| Expected | `404 Not Found` |
|---------|-----------------|

---

**TC-D-06 — Update driver license plate**
```bash
curl -s -X PUT http://localhost:5000/api/drivers/1 \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Alice Johnson","phone":"555-3001","licensePlate":"IL-NEW-999"}'
```
| Field | Expected |
|-------|----------|
| HTTP status | `200 OK` |
| `licensePlate` | `"IL-NEW-999"` |

---

**TC-D-07 — Delete a driver**
```bash
DRIVER_ID=$(curl -s -X POST http://localhost:5000/api/drivers \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Temp Driver"}' | python -c "import sys,json; print(json.load(sys.stdin)['driverId'])")

curl -s -o /dev/null -w "%{http_code}" -X DELETE http://localhost:5000/api/drivers/$DRIVER_ID
```
| Expected | `204 No Content` |
|---------|-----------------|

---

**TC-D-08 — Missing required `fullName`**
```bash
curl -s -X POST http://localhost:5000/api/drivers \
  -H "Content-Type: application/json" \
  -d '{"phone":"555-0000"}'
```
| Expected | `400 Bad Request` |
|---------|-------------------|
