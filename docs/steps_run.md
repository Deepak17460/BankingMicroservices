# Banking Microservices — Run & Test Guide

Step-by-step instructions to install, build, run, and test the solution on **WSL (Ubuntu)** or **Windows**.

**Related docs:** [README.md](../README.md) · [api.md](./api.md) · [Documentation index](./README.md)

---

## Prerequisites

| Tool | Purpose |
|------|---------|
| .NET SDK **8.0+** or **10.0+** | Build and run services |
| Git | Clone the repository |
| curl or Postman | Test APIs |
| Docker (optional) | Run all services with one command |

Check SDK:

```bash
dotnet --version
dotnet --list-sdks
```

---

## 1. Install .NET SDK on WSL (Ubuntu 24.04)

Skip this section if `dotnet --version` already works.

```bash
# Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET 10 SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# Verify
dotnet --version
dotnet --list-sdks
```

> **Tip:** Prefer `apt` over `snap install dotnet` on WSL (fewer certificate/path issues).

---

## 2. Get the project

```bash
cd "/mnt/c/Users/dekuma/OneDrive - ASSA ABLOY Group/Desktop/Microservices/BankingMicroservices"
```

Or clone from GitHub:

```bash
git clone <your-repo-url>
cd BankingMicroservices
```

**Performance:** Building on `/mnt/c/.../OneDrive` is slower. For faster builds, copy to WSL home:

```bash
cp -r "/mnt/c/Users/.../BankingMicroservices" ~/BankingMicroservices
cd ~/BankingMicroservices
```

---

## 3. Build the solution

```bash
dotnet build BankingMicroservices.sln
```

If you see *"targets multiple frameworks"*, either:

- Pull latest code (`DefaultTargetFramework` is `net10.0`), or  
- Build/run with: `-f net10.0`

```bash
dotnet build BankingMicroservices.sln -f net10.0
```

**Build failed on Shared / ASP.NET types?** Ensure `src/Shared/Shared.csproj` uses `Microsoft.NET.Sdk.Web` (see latest `main`).

---

## 4. Run all services (5 terminals)

**Start in this order.** Wait until each terminal shows `Now listening on http://localhost:....`

### Terminal 1 — Service Discovery (port 5003) — start first

```bash
cd BankingMicroservices
dotnet run --project src/ServiceDiscovery/ServiceDiscovery.csproj
```

### Terminal 2 — Configuration Service (port 5004)

```bash
dotnet run --project src/ConfigurationService/ConfigurationService.csproj
```

### Terminal 3 — Customer Management (port 5001)

```bash
dotnet run --project src/CustomerManagementService/CustomerManagementService.csproj
```

### Terminal 4 — Account Management (port 5002)

```bash
dotnet run --project src/AccountManagementService/AccountManagementService.csproj
```

### Terminal 5 — API Gateway (port 5000)

```bash
dotnet run --project src/ApiGateway/ApiGateway.csproj
```

### Explicit framework (if needed)

```bash
dotnet run -f net10.0 --project src/ServiceDiscovery/ServiceDiscovery.csproj
dotnet run -f net10.0 --project src/ConfigurationService/ConfigurationService.csproj
dotnet run -f net10.0 --project src/CustomerManagementService/CustomerManagementService.csproj
dotnet run -f net10.0 --project src/AccountManagementService/AccountManagementService.csproj
dotnet run -f net10.0 --project src/ApiGateway/ApiGateway.csproj
```

### Port summary

| # | Service | URL |
|---|---------|-----|
| 1 | Service Discovery | http://localhost:5003 |
| 2 | Configuration | http://localhost:5004 |
| 3 | Customer Management | http://localhost:5001 |
| 4 | Account Management | http://localhost:5002 |
| 5 | **API Gateway** (use for testing) | **http://localhost:5000** |

---

## 5. Verify services are up

New terminal:

```bash
# Registry (Eureka-like)
curl -s http://localhost:5003/discover/customer-management

# Config
curl -s http://localhost:5004/config/customer-management

# Customers via gateway
curl -s http://localhost:5000/gateway/customers
```

---

## 6. Test with curl (gateway)

Replace `<customerId>` and `<accountId>` with GUIDs from responses.

### Create customer (auto-creates account, balance 0)

```bash
curl -s -X POST http://localhost:5000/gateway/customers \
  -H "Content-Type: application/json" \
  -d '{"name":"Jane Doe","email":"jane@bank.com","phone":"555-0100","address":"123 Main St"}'
```

Copy `data.id` → **customerId**.

### List customers

```bash
curl -s http://localhost:5000/gateway/customers
```

### Deposit $500

```bash
curl -s -X POST http://localhost:5000/gateway/accounts/deposit \
  -H "Content-Type: application/json" \
  -d '{"customerId":"<customerId>","amount":500}'
```

Copy `data.id` → **accountId**; check `data.balance` is `500`.

### Withdraw $100

```bash
curl -s -X POST http://localhost:5000/gateway/accounts/withdraw \
  -H "Content-Type: application/json" \
  -d '{"customerId":"<customerId>","amount":100}'
```

Expect `balance`: `400`.

### Get account (with customer details)

```bash
curl -s http://localhost:5000/gateway/accounts/<accountId>
```

### Update customer

```bash
curl -s -X PUT http://localhost:5000/gateway/customers/<customerId> \
  -H "Content-Type: application/json" \
  -d '{"name":"Jane Smith","email":"jane.smith@bank.com","phone":"555-0101","address":"456 Oak Ave"}'
```

### Delete customer (+ account)

```bash
curl -s -X DELETE http://localhost:5000/gateway/customers/<customerId>
```

### Pretty-print JSON (no jq required)

```bash
curl -s http://localhost:5000/gateway/customers | python3 -m json.tool
```

---

## 7. Test with Postman

**Base URL:** `http://localhost:5000`

| Action | Method | URL |
|--------|--------|-----|
| Create customer | POST | `/gateway/customers` |
| List customers | GET | `/gateway/customers` |
| Get customer | GET | `/gateway/customers/{id}` |
| Update customer | PUT | `/gateway/customers/{id}` |
| Delete customer | DELETE | `/gateway/customers/{id}` |
| Deposit | POST | `/gateway/accounts/deposit` |
| Withdraw | POST | `/gateway/accounts/withdraw` |
| Get account | GET | `/gateway/accounts/{accountId}` |

**Deposit body (raw JSON):**

```json
{
  "customerId": "d4e7425f-7244-43bb-9c0a-82a7b1ea2c80",
  "amount": 500
}
```

**Withdraw body:**

```json
{
  "customerId": "d4e7425f-7244-43bb-9c0a-82a7b1ea2c80",
  "amount": 100
}
```

Full API details: [api.md](./api.md)

**Service Discovery (no UI):**

```text
GET http://localhost:5003/discover/customer-management
GET http://localhost:5003/discover/account-management
```

---

## 8. Run with Docker (alternative)

From `BankingMicroservices` folder:

```bash
docker-compose up --build
```

Stop:

```bash
docker-compose down
```

Same ports: 5000–5004. Test with gateway: `http://localhost:5000/gateway/customers`

---

## 9. Troubleshooting

| Problem | Solution |
|---------|----------|
| `dotnet: command not found` | Install SDK (Section 1) |
| Multiple frameworks — specify `--framework` | Use `-f net10.0` or pull latest repo |
| `Connection refused` on gateway | Start all 5 services; Discovery + Config first |
| Customer create fails | Ensure ports 5003, 5004, 5001, 5002 are running |
| Deposit 404 / customer not found | Use valid `customerId` from create/list |
| Insufficient balance | Deposit before withdraw |
| Postman cannot connect | Use `http://localhost:5000` (not https); services in WSL |
| Slow build on OneDrive | Copy project to `~/BankingMicroservices` |
| `jq` not found | Use raw JSON or `python3 -m json.tool` |

---

## 10. Quick checklist

- [ ] `dotnet build` succeeds  
- [ ] Terminal 1: Service Discovery on 5003  
- [ ] Terminal 2: Configuration on 5004  
- [ ] Terminal 3: Customer on 5001  
- [ ] Terminal 4: Account on 5002  
- [ ] Terminal 5: Gateway on 5000  
- [ ] `GET /gateway/customers` works  
- [ ] Create → Deposit → Withdraw → Get account  

---

## 11. Stop services

Press `Ctrl+C` in each terminal, or for Docker: `docker-compose down`.
