# Banking Microservices — API Reference

All banking operations should go through the **API Gateway** when possible. Infrastructure endpoints (Service Discovery, Configuration) are called directly or by services at startup.

**Base URLs (local development)**

| Service | Base URL |
|---------|----------|
| API Gateway (recommended) | `http://localhost:5000` |
| Customer Management | `http://localhost:5001` |
| Account Management | `http://localhost:5002` |
| Service Discovery | `http://localhost:5003` |
| Configuration Service | `http://localhost:5004` |

**Common headers**

| Header | Value | When |
|--------|--------|------|
| `Content-Type` | `application/json` | All requests with a body |

---

## Response formats

### Success wrapper (`Customer` & `Account` services)

Most customer and account endpoints return:

```json
{
  "success": true,
  "data": { },
  "message": "optional message"
}
```

### Error format (RFC 7807 Problem Details)

Errors return `application/problem+json`:

```json
{
  "type": null,
  "title": "Customer Not Found",
  "status": 404,
  "detail": "Customer with id '...' was not found.",
  "instance": "/api/customers/..."
}
```

| HTTP Status | Title | When |
|-------------|--------|------|
| 400 | Bad Request / Insufficient Balance | Invalid input, amount ≤ 0, insufficient funds |
| 404 | Customer Not Found / Account Not Found | Unknown GUID |
| 503 | Service Unavailable | Peer service not in registry |
| 500 | Internal Server Error | Unexpected error |

---

## API Gateway (port 5000)

YARP reverse proxy. Gateway paths are rewritten to backend `/api/...` routes.

| Gateway path | Proxied to |
|--------------|------------|
| `/gateway/customers/**` | Customer Service `/api/customers/**` |
| `/gateway/accounts/**` | Account Service `/api/accounts/**` |

Use these URLs in **Postman** and external clients.

---

## Customer APIs

### Via Gateway

| Method | URL | Description |
|--------|-----|-------------|
| `POST` | `/gateway/customers` | Create customer (+ auto-create account) |
| `GET` | `/gateway/customers` | List all customers |
| `GET` | `/gateway/customers/{id}` | Get customer by ID |
| `PUT` | `/gateway/customers/{id}` | Update customer |
| `DELETE` | `/gateway/customers/{id}` | Delete customer and linked account |

**Full URLs:** `http://localhost:5000/gateway/customers[...]`

### Direct (Customer Service — port 5001)

| Method | URL | Description |
|--------|-----|-------------|
| `POST` | `/api/customers` | Create customer |
| `GET` | `/api/customers` | List all customers |
| `GET` | `/api/customers/{id}` | Get customer by ID |
| `PUT` | `/api/customers/{id}` | Update customer |
| `DELETE` | `/api/customers/{id}` | Delete customer and linked account |

**Full URLs:** `http://localhost:5001/api/customers[...]`

---

### Create customer

`POST /gateway/customers` (or `POST /api/customers` on port 5001)

**Request body**

```json
{
  "name": "Jane Doe",
  "email": "jane@bank.com",
  "phone": "555-0100",
  "address": "123 Main St"
}
```

| Field | Type | Required |
|-------|------|----------|
| `name` | string | Yes |
| `email` | string | Yes |
| `phone` | string | Yes |
| `address` | string | Yes |

**Response:** `201 Created`

```json
{
  "success": true,
  "data": {
    "id": "d4e7425f-7244-43bb-9c0a-82a7b1ea2c80",
    "name": "Jane Doe",
    "email": "jane@bank.com",
    "phone": "555-0100",
    "address": "123 Main St",
    "createdAt": "2026-05-20T19:00:20.1335084Z"
  },
  "message": null
}
```

**Side effect:** Account Management Service creates a **Checking** account with **balance 0** for the new customer (via Service Discovery).

---

### List customers

`GET /gateway/customers`

**Response:** `200 OK`

```json
{
  "success": true,
  "data": [
    {
      "id": "cf31bd94-2489-448d-80fd-208ef6ba6607",
      "name": "Jane Doe",
      "email": "jane@bank.com",
      "phone": "555-0100",
      "address": "123 Main St",
      "createdAt": "2026-05-20T18:59:34.6183451Z"
    }
  ],
  "message": null
}
```

---

### Get customer by ID

`GET /gateway/customers/{id}`

| Parameter | Type | Location |
|-----------|------|----------|
| `id` | GUID | Path |

**Response:** `200 OK` — `data` is a single `CustomerDto`.

**Errors:** `404` if customer does not exist.

---

### Update customer

`PUT /gateway/customers/{id}`

**Request body**

```json
{
  "name": "Jane Smith",
  "email": "jane.smith@bank.com",
  "phone": "555-0101",
  "address": "456 Oak Ave"
}
```

**Response:** `200 OK` — updated `CustomerDto` in `data`.

**Errors:** `404` if customer does not exist.

---

### Delete customer

`DELETE /gateway/customers/{id}`

**Response:** `200 OK`

```json
{
  "success": true,
  "data": null,
  "message": "Customer and associated account deleted."
}
```

**Side effect:** Deletes the customer’s account in Account Management Service.

**Errors:** `404` if customer does not exist.

---

## Account APIs

### Via Gateway

| Method | URL | Description |
|--------|-----|-------------|
| `POST` | `/gateway/accounts` | Create account for customer (internal use) |
| `POST` | `/gateway/accounts/deposit` | Deposit money |
| `POST` | `/gateway/accounts/withdraw` | Withdraw money |
| `GET` | `/gateway/accounts/{id}` | Get account with customer details |
| `DELETE` | `/gateway/accounts/customer/{customerId}` | Delete account by customer ID |

**Full URLs:** `http://localhost:5000/gateway/accounts[...]`

### Direct (Account Service — port 5002)

| Method | URL | Description |
|--------|-----|-------------|
| `POST` | `/api/accounts` | Create account |
| `POST` | `/api/accounts/deposit` | Deposit |
| `POST` | `/api/accounts/withdraw` | Withdraw |
| `GET` | `/api/accounts/{id}` | Get account with customer |
| `DELETE` | `/api/accounts/customer/{customerId}` | Delete by customer ID |

**Full URLs:** `http://localhost:5002/api/accounts[...]`

---

### Create account

`POST /gateway/accounts`

Normally invoked automatically when a customer is created. Can be called manually.

**Request body**

```json
{
  "customerId": "d4e7425f-7244-43bb-9c0a-82a7b1ea2c80"
}
```

**Response:** `201 Created`

```json
{
  "success": true,
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "customerId": "d4e7425f-7244-43bb-9c0a-82a7b1ea2c80",
    "balance": 0,
    "accountType": "Checking",
    "createdAt": "2026-05-20T19:00:21.0000000Z"
  },
  "message": null
}
```

**Validation:** Customer must exist (HTTP call to Customer Service via Service Discovery).

**Note:** If an account already exists for the customer, returns the existing account.

---

### Deposit

`POST /gateway/accounts/deposit`

**Request body**

```json
{
  "customerId": "d4e7425f-7244-43bb-9c0a-82a7b1ea2c80",
  "amount": 500
}
```

| Field | Type | Required | Rules |
|-------|------|----------|--------|
| `customerId` | GUID | Yes | Must exist |
| `amount` | decimal | Yes | Must be > 0 |

**Response:** `200 OK`

```json
{
  "success": true,
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "customerId": "d4e7425f-7244-43bb-9c0a-82a7b1ea2c80",
    "balance": 500,
    "accountType": "Checking",
    "createdAt": "2026-05-20T19:00:21.0000000Z"
  },
  "message": "Deposit successful."
}
```

**Errors**

| Status | Reason |
|--------|--------|
| 404 | Customer or account not found |
| 400 | Amount ≤ 0 |

---

### Withdraw

`POST /gateway/accounts/withdraw`

**Request body**

```json
{
  "customerId": "d4e7425f-7244-43bb-9c0a-82a7b1ea2c80",
  "amount": 100
}
```

| Field | Type | Required | Rules |
|-------|------|----------|--------|
| `customerId` | GUID | Yes | Must exist |
| `amount` | decimal | Yes | Must be > 0; must not exceed balance |

**Response:** `200 OK` — same shape as deposit; `message`: `"Withdrawal successful."`

**Errors**

| Status | Reason |
|--------|--------|
| 404 | Customer or account not found |
| 400 | Amount ≤ 0 or **insufficient balance** |

---

### Get account by ID (with customer)

`GET /gateway/accounts/{id}`

| Parameter | Type | Location |
|-----------|------|----------|
| `id` | GUID | Path (account ID, not customer ID) |

**Response:** `200 OK`

```json
{
  "success": true,
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "customerId": "d4e7425f-7244-43bb-9c0a-82a7b1ea2c80",
    "balance": 400,
    "accountType": "Checking",
    "createdAt": "2026-05-20T19:00:21.0000000Z",
    "customer": {
      "id": "d4e7425f-7244-43bb-9c0a-82a7b1ea2c80",
      "name": "Jane Doe",
      "email": "jane@bank.com",
      "phone": "555-0100",
      "address": "123 Main St",
      "createdAt": "2026-05-20T19:00:20.1335084Z"
    }
  },
  "message": null
}
```

**Errors:** `404` if account does not exist.

---

### Delete account by customer ID

`DELETE /gateway/accounts/customer/{customerId}`

**Response:** `200 OK`

```json
{
  "success": true,
  "data": null,
  "message": "Account deleted."
}
```

**Errors:** `404` if no account for that customer.

---

## Service Discovery (port 5003)

Lightweight registry (Eureka-like concept, **no web UI**). Services register on startup and send a heartbeat every **10 seconds**. Entries with no heartbeat for **30+ seconds** are removed.

### Register service

`POST /register`

**Request body**

```json
{
  "name": "customer-management",
  "url": "http://localhost:5001",
  "lastHeartbeat": "2026-05-20T19:00:00Z"
}
```

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Service name (see table below) |
| `url` | string | Base URL of the service |
| `lastHeartbeat` | datetime (UTC) | Last heartbeat timestamp |

**Response:** `200 OK`

```json
{
  "message": "Service 'customer-management' registered."
}
```

---

### Discover service by name

`GET /discover/{serviceName}`

**Response:** `200 OK`

```json
{
  "name": "customer-management",
  "url": "http://localhost:5001",
  "lastHeartbeat": "2026-05-20T19:05:00Z"
}
```

**Response:** `404 Not Found` if service is unknown or stale.

**Registered service names**

| `serviceName` | Description |
|---------------|-------------|
| `customer-management` | Customer API |
| `account-management` | Account API |
| `api-gateway` | YARP gateway |
| `service-discovery` | This registry |
| `configuration-service` | Central config |

---

## Configuration Service (port 5004)

Central in-memory configuration. Other services fetch config on startup.

### Get configuration

`GET /config/{serviceName}`

**Response:** `200 OK`

```json
{
  "settings": {
    "AccountServiceName": "account-management"
  }
}
```

**Example URLs**

| Service | URL |
|---------|-----|
| Customer Management | `http://localhost:5004/config/customer-management` |
| Account Management | `http://localhost:5004/config/account-management` |
| API Gateway | `http://localhost:5004/config/api-gateway` |

**Configured keys**

| `serviceName` | Settings |
|---------------|----------|
| `customer-management` | `AccountServiceName` → `account-management` |
| `account-management` | `CustomerServiceName` → `customer-management` |
| `api-gateway` | `CustomerServiceName`, `AccountServiceName` |
| `service-discovery` | (empty) |
| `configuration-service` | (empty) |

**Response:** `404` if `serviceName` is not defined.

---

## Postman quick reference

| Action | Method | URL |
|--------|--------|-----|
| Create customer | `POST` | `http://localhost:5000/gateway/customers` |
| List customers | `GET` | `http://localhost:5000/gateway/customers` |
| Get customer | `GET` | `http://localhost:5000/gateway/customers/{id}` |
| Update customer | `PUT` | `http://localhost:5000/gateway/customers/{id}` |
| Delete customer | `DELETE` | `http://localhost:5000/gateway/customers/{id}` |
| Deposit | `POST` | `http://localhost:5000/gateway/accounts/deposit` |
| Withdraw | `POST` | `http://localhost:5000/gateway/accounts/withdraw` |
| Get account | `GET` | `http://localhost:5000/gateway/accounts/{accountId}` |
| Discover service | `GET` | `http://localhost:5003/discover/customer-management` |
| Get config | `GET` | `http://localhost:5004/config/customer-management` |

---

## Typical flow (sequence)

```
1. POST /gateway/customers          → customerId, account created (balance 0)
2. POST /gateway/accounts/deposit   → balance increased, accountId in response
3. POST /gateway/accounts/withdraw  → balance decreased
4. GET  /gateway/accounts/{id}      → account + customer details
5. PUT  /gateway/customers/{id}     → update profile
6. DELETE /gateway/customers/{id}   → remove customer + account
```

---

## curl examples

```bash
# Create customer
curl -s -X POST http://localhost:5000/gateway/customers \
  -H "Content-Type: application/json" \
  -d '{"name":"Jane Doe","email":"jane@bank.com","phone":"555-0100","address":"123 Main St"}'

# Deposit
curl -s -X POST http://localhost:5000/gateway/accounts/deposit \
  -H "Content-Type: application/json" \
  -d '{"customerId":"<customerId>","amount":500}'

# Withdraw
curl -s -X POST http://localhost:5000/gateway/accounts/withdraw \
  -H "Content-Type: application/json" \
  -d '{"customerId":"<customerId>","amount":100}'

# Discover service
curl -s http://localhost:5003/discover/customer-management
```

---

## Architecture notes

- All **inter-service** calls resolve peer URLs through **Service Discovery** (no hardcoded `localhost:5001` in business logic).
- **Bootstrap** URLs for discovery (`5003`) and configuration (`5004`) are set in `appsettings.json` / environment variables only.
- Storage is **in-memory** (`ConcurrentDictionary`); data is lost when a service restarts.
- There is **no authentication** in this MVP; do not expose publicly without adding security.
