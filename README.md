# Banking Microservices MVP

**API documentation:** [api.md](./api.md)

A .NET microservices banking demonstration (targets **.NET 10** and **.NET 8**) with in-memory storage, custom service discovery, centralized configuration, YARP API gateway, Polly resilience, Serilog logging, and global exception handling.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) **10.0** (recommended, e.g. `10.0.300`) or **8.0+**
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for containerized deployment)
- [Git](https://git-scm.com/) (optional, for version control)

This repo has **no `global.json` SDK pin**, so the newest installed SDK is used automatically.

| Your installed SDK | Framework used |
|--------------------|----------------|
| .NET 10.x (e.g. 10.0.300) | `net10.0` (default) |
| .NET 8.x only | `net8.0` |

Check installed SDKs:

```bash
dotnet --list-sdks
dotnet --version
```

## Troubleshooting

### "A compatible .NET SDK was not found"

Pull the latest code (this repo no longer ships a restrictive `global.json`). You need **.NET 8 or .NET 10 SDK** installed.

### Wrong `dotnet run` command

Do **not** use `dotnet run -BankingMicroservices`. Use `--project`:

```bash
cd BankingMicroservices
dotnet run --project src/ServiceDiscovery/ServiceDiscovery.csproj
```

Build the solution:

```bash
dotnet build BankingMicroservices.sln
```

Force .NET 10 (matches Visual Studio 2026 / SDK 10.0.300):

```bash
dotnet build -f net10.0
dotnet run -f net10.0 --project src/ServiceDiscovery/ServiceDiscovery.csproj
```

## Git

This repository is initialized and ready to use. From the `BankingMicroservices` folder:

```bash
git status
git add .
git commit -m "Initial commit: banking microservices MVP"
```

To connect a remote and push:

```bash
git remote add origin <your-repo-url>
git push -u origin main
```

Tracked files exclude build output (`bin/`, `obj/`), IDE folders, logs, and local secrets (see `.gitignore`).

## Architecture

```
                    +------------------+
                    |   API Gateway    |
                    |   (YARP) :5000   |
                    +--------+---------+
                             |
              +--------------+---------------+
              |                              |
    +---------v---------+          +---------v---------+
    | Customer Service  |          | Account Service   |
    |      :5001        |<-------->|      :5002        |
    +---------+---------+          +---------+---------+
              |                              |
              +--------------+---------------+
                             |
              +--------------v---------------+
              |     Service Discovery        |
              |           :5003              |
              +--------------+---------------+
                             |
              +--------------v---------------+
              |   Configuration Service      |
              |           :5004              |
              +------------------------------+

Inter-service calls resolve peer URLs via Service Discovery (no hardcoded service URLs in business logic).
Bootstrap URLs for discovery and configuration are supplied via appsettings / environment variables only.
```

## Service Port Summary

| Service | Port | Base URL |
|---------|------|----------|
| Api Gateway | 5000 | http://localhost:5000 |
| Customer Management | 5001 | http://localhost:5001 |
| Account Management | 5002 | http://localhost:5002 |
| Service Discovery | 5003 | http://localhost:5003 |
| Configuration Service | 5004 | http://localhost:5004 |

## How to Run

### Option 1: dotnet run (local)

Start infrastructure services first, then business services, then the gateway.

```bash
cd BankingMicroservices

# Terminal 1 - Service Discovery (start first)
dotnet run --project src/ServiceDiscovery/ServiceDiscovery.csproj

# Terminal 2 - Configuration Service
dotnet run --project src/ConfigurationService/ConfigurationService.csproj

# Terminal 3 - Customer Management
dotnet run --project src/CustomerManagementService/CustomerManagementService.csproj

# Terminal 4 - Account Management
dotnet run --project src/AccountManagementService/AccountManagementService.csproj

# Terminal 5 - API Gateway
dotnet run --project src/ApiGateway/ApiGateway.csproj
```

Or build once:

```bash
dotnet build BankingMicroservices.sln
```

### Option 2: Docker Compose

```bash
cd BankingMicroservices
docker-compose up --build
```

Stop with `docker-compose down`.

## Sample API Calls

### Service Discovery

```bash
# Register a service (done automatically on startup by each service)
curl -X POST http://localhost:5003/register \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"customer-management\",\"url\":\"http://localhost:5001\",\"lastHeartbeat\":\"2026-05-20T12:00:00Z\"}"

# Discover a service URL
curl http://localhost:5003/discover/customer-management
```

### Configuration Service

```bash
curl http://localhost:5004/config/customer-management
curl http://localhost:5004/config/account-management
```

### Customer Management (direct)

```bash
# Create customer (auto-creates account with zero balance)
curl -X POST http://localhost:5001/api/customers \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"Jane Doe\",\"email\":\"jane@bank.com\",\"phone\":\"555-0100\",\"address\":\"123 Main St\"}"

# List all customers
curl http://localhost:5001/api/customers

# Get customer by id (replace {id} with a GUID from create response)
curl http://localhost:5001/api/customers/{id}

# Update customer
curl -X PUT http://localhost:5001/api/customers/{id} \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"Jane Smith\",\"email\":\"jane.smith@bank.com\",\"phone\":\"555-0101\",\"address\":\"456 Oak Ave\"}"

# Delete customer and associated account
curl -X DELETE http://localhost:5001/api/customers/{id}
```

### Account Management (direct)

```bash
# Deposit
curl -X POST http://localhost:5002/api/accounts/deposit \
  -H "Content-Type: application/json" \
  -d "{\"customerId\":\"{customerId}\",\"amount\":500.00}"

# Withdraw
curl -X POST http://localhost:5002/api/accounts/withdraw \
  -H "Content-Type: application/json" \
  -d "{\"customerId\":\"{customerId}\",\"amount\":100.00}"

# Get account with customer details (replace {accountId})
curl http://localhost:5002/api/accounts/{accountId}

# Delete account by customer id
curl -X DELETE http://localhost:5002/api/accounts/customer/{customerId}
```

### API Gateway (proxied)

```bash
# Create customer via gateway
curl -X POST http://localhost:5000/gateway/customers \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"John Doe\",\"email\":\"john@bank.com\",\"phone\":\"555-0200\",\"address\":\"789 Pine Rd\"}"

# List customers via gateway
curl http://localhost:5000/gateway/customers

# Deposit via gateway
curl -X POST http://localhost:5000/gateway/accounts/deposit \
  -H "Content-Type: application/json" \
  -d "{\"customerId\":\"{customerId}\",\"amount\":250.00}"

# Withdraw via gateway
curl -X POST http://localhost:5000/gateway/accounts/withdraw \
  -H "Content-Type: application/json" \
  -d "{\"customerId\":\"{customerId}\",\"amount\":50.00}"

# Get account via gateway
curl http://localhost:5000/gateway/accounts/{accountId}
```

## Solution Structure

```
BankingMicroservices/
├── BankingMicroservices.sln
├── docker-compose.yml
├── docker/
│   ├── Dockerfile.api-gateway
│   ├── Dockerfile.account
│   ├── Dockerfile.configuration
│   ├── Dockerfile.customer
│   └── Dockerfile.service-discovery
└── src/
    ├── Shared/
    ├── ApiGateway/
    ├── ServiceDiscovery/
    ├── ConfigurationService/
    ├── CustomerManagementService/
    └── AccountManagementService/
```

## Features

- **In-memory storage** using `ConcurrentDictionary` (no external databases)
- **Service discovery** with registration, discovery, and 30-second stale cleanup
- **Centralized configuration** fetched on startup per service
- **Heartbeat** every 10 seconds from all services
- **Multi-target** `net10.0` + `net8.0` via `Directory.Build.props`
- **Polly** retry (3 attempts, 2s delay) and circuit breaker (5 failures, 30s break)
- **Serilog** structured console logging
- **Global exception middleware** returning RFC 7807 `ProblemDetails`
- **YARP** reverse proxy at `/gateway/customers/**` and `/gateway/accounts/**`

## Notes

- All inter-service business calls resolve peer service URLs through Service Discovery.
- Bootstrap URLs (`ServiceDiscovery`, `ConfigurationService`, own `ServiceUrl`) are the only addresses configured directly (appsettings / environment).
- Start Service Discovery and Configuration Service before Customer and Account services when running locally.
