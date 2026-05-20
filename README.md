# Banking Microservices MVP

A .NET microservices banking demo with in-memory storage, custom service discovery, centralized configuration, a YARP API gateway, Polly resilience, and Serilog logging.

## Documentation

| Guide | Link |
|-------|------|
| **Run & test** (WSL, Windows, Postman) | [docs/steps_run.md](docs/steps_run.md) |
| **Docker** (Compose, one command) | [docs/docker.md](docs/docker.md) |
| **API reference** (all endpoints) | [docs/api.md](docs/api.md) |
| **Docs index** | [docs/README.md](docs/README.md) |

## Quick start

**Prerequisites:** [.NET SDK](https://dotnet.microsoft.com/download) 8.0+ or 10.0+ · optional [Docker](https://www.docker.com/products/docker-desktop/)

```bash
cd BankingMicroservices
dotnet build BankingMicroservices.sln
```

Start **5 terminals** in order (see [docs/steps_run.md](docs/steps_run.md) for details):

```bash
dotnet run --project src/ServiceDiscovery/ServiceDiscovery.csproj      # :5003
dotnet run --project src/ConfigurationService/ConfigurationService.csproj  # :5004
dotnet run --project src/CustomerManagementService/CustomerManagementService.csproj  # :5001
dotnet run --project src/AccountManagementService/AccountManagementService.csproj      # :5002
dotnet run --project src/ApiGateway/ApiGateway.csproj                  # :5000
```

**Or Docker** (see [docs/docker.md](docs/docker.md)):

```bash
docker compose up --build
```

**Smoke test** (gateway):

```bash
curl -s http://localhost:5000/gateway/customers
curl -s -X POST http://localhost:5000/gateway/customers \
  -H "Content-Type: application/json" \
  -d '{"name":"Jane Doe","email":"jane@bank.com","phone":"555-0100","address":"123 Main St"}'
```

## Services & ports

| Service | Port | Role |
|---------|------|------|
| API Gateway | 5000 | Public entry — `/gateway/customers`, `/gateway/accounts` |
| Customer Management | 5001 | Customer CRUD |
| Account Management | 5002 | Deposits, withdrawals, balances |
| Service Discovery | 5003 | Registry (Eureka-like, API only) |
| Configuration | 5004 | Central config per service |

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
              |     Service Discovery :5003  |
              +--------------+---------------+
                             |
              +--------------v---------------+
              |   Configuration Service :5004|
              +------------------------------+
```

Inter-service calls use **Service Discovery** (no hardcoded peer URLs in business logic).

## Solution structure

```
BankingMicroservices/
├── BankingMicroservices.sln
├── README.md
├── docker-compose.yml
├── Directory.Build.props
├── docs/
│   ├── README.md          # Documentation index
│   ├── steps_run.md       # How to run & test (local)
│   ├── docker.md          # Docker Compose guide
│   └── api.md             # API reference
├── docker/
│   ├── Dockerfile.api-gateway
│   ├── Dockerfile.account
│   ├── Dockerfile.configuration
│   ├── Dockerfile.customer
│   └── Dockerfile.service-discovery
└── src/
    ├── Shared/                    # DTOs, middleware, Polly, discovery client
    ├── ApiGateway/                # YARP reverse proxy
    ├── ServiceDiscovery/          # Custom registry
    ├── ConfigurationService/      # Central config API
    ├── CustomerManagementService/
    └── AccountManagementService/
```

## Features

- In-memory `ConcurrentDictionary` storage (no database)
- Custom service discovery with heartbeat and stale cleanup
- Centralized configuration loaded on startup
- YARP gateway, Polly retry + circuit breaker, Serilog, ProblemDetails errors
- Multi-target **net10.0** / **net8.0** (no `global.json` SDK pin)

## SDK note

| Installed SDK | Framework used |
|---------------|----------------|
| .NET 10.x | `net10.0` (default) |
| .NET 8.x only | `net8.0` |

If `dotnet run` asks for a framework: `dotnet run -f net10.0 --project src/...`


See `.gitignore` for excluded build artifacts and secrets.
