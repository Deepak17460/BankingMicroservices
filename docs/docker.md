# Docker — Run the Banking Microservices Stack

Run all five services with **one command** using Docker Compose. No .NET SDK required on the host (only Docker).

**See also:** [steps_run.md](./steps_run.md) (local `dotnet run`) · [api.md](./api.md) (endpoints) · [README.md](../README.md)

---

## Do you need Docker?

| Situation | Recommendation |
|-----------|----------------|
| You already run with `dotnet run` | Optional — keep using that for daily dev |
| Teammate has no .NET SDK | Use Docker |
| Demo / assignment submission | Docker gives a single, repeatable start |
| Production deployment | Docker is a starting point; add orchestration (K8s, etc.) later |

The repo already includes `docker-compose.yml` and Dockerfiles under `docker/`. **Nothing extra to dockerize.**

---

## Prerequisites

1. Install [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/Mac) or Docker Engine (Linux).
2. **WSL users:** Enable *Settings → Resources → WSL integration* for your distro.
3. Verify:

```bash
docker --version
docker compose version
```

---

## Quick start

From the repository root (`BankingMicroservices` folder):

```bash
cd path/to/BankingMicroservices
docker compose up --build
```

- First run **builds images** (may take 5–15 minutes).
- Logs from all services appear in one terminal.
- When ready, test: `http://localhost:5010/gateway/customers`

**Stop and remove containers:**

```bash
docker compose down
```

**Rebuild after code changes:**

```bash
docker compose up --build
```

---

## What gets started

| Container | Port | Image built from | Swagger Docs | Dashboard |
|-----------|------|------------------|--------------|-----------|
| `banking-api-gateway` | 5010 | `docker/Dockerfile.api-gateway` | http://localhost:5010/swagger | - |
| `customer-service` | 5001 | `docker/Dockerfile.customer-service` | http://localhost:5001/swagger | - |
| `account-service` | 5002 | `docker/Dockerfile.account-service` | http://localhost:5002/swagger | - |
| `service-discovery` | 5003 | `docker/Dockerfile.service-discovery` | http://localhost:5003/swagger | http://localhost:5003 |
| `configuration-service` | 5004 | `docker/Dockerfile.configuration-service` | http://localhost:5004/swagger | - |

Startup order (via `depends_on` with health checks):

```
service-discovery → configuration-service → customer-service → account-service → banking-api-gateway
```

**Health Check Features:**
- All services include health check endpoints  
- Docker waits for dependencies to be healthy before starting dependent services
- Services register automatically with Service Discovery on startup

Inside Docker, services talk to each other by **container name** (e.g. `http://customer-service:5001`). Your machine uses **localhost** on the mapped ports.

---

## Test the stack

### Browser or Postman

| Action | Method | URL |
|--------|--------|-----|
| **Service Discovery Dashboard** | GET | `http://localhost:5003` |
| List customers | GET | `http://localhost:5010/gateway/customers` |
| Create customer | POST | `http://localhost:5010/gateway/customers` |
| Deposit | POST | `http://localhost:5010/gateway/accounts/deposit` |
| Withdraw | POST | `http://localhost:5010/gateway/accounts/withdraw` |

**Create customer (Postman / curl):**

```bash
curl -s -X POST http://localhost:5010/gateway/customers \
  -H "Content-Type: application/json" \
  -d '{"name":"Jane Doe","email":"jane@bank.com","phone":"555-0100","address":"123 Main St"}'
```

**Deposit** (replace `customerId`):

```bash
curl -s -X POST http://localhost:5010/gateway/accounts/deposit \
  -H "Content-Type: application/json" \
  -d '{"customerId":"<customerId>","amount":500}'
```

Full API list: [api.md](./api.md)

### Infrastructure & Monitoring

```bash
# Service Discovery Dashboard (NEW!)
curl -s http://localhost:5003

# Service Registry API
curl -s http://localhost:5003/api/registry

# Traditional Discovery Endpoints  
curl -s http://localhost:5003/discover/customer-management
curl -s http://localhost:5004/config/customer-management
```

---

## Run in the background

```bash
docker compose up --build -d
```

View logs:

```bash
docker compose logs -f
docker compose logs -f api-gateway
```

Stop:

```bash
docker compose down
```

---

## Useful commands

| Command | Purpose |
|---------|---------|
| `docker compose ps` | List running containers |
| `docker compose logs -f <service>` | Follow logs for one service |
| `docker compose build --no-cache` | Force full rebuild |
| `docker compose down -v` | Stop and remove volumes (if any added later) |
| `docker image ls` | List built images |

---

## Configuration in Docker

`docker-compose.yml` sets:

- `ASPNETCORE_ENVIRONMENT=Docker` → loads `appsettings.Docker.json` per service
- `Bootstrap__ServiceDiscoveryUrl`, `Bootstrap__ConfigurationServiceUrl`, `Bootstrap__ServiceUrl` → internal Docker hostnames

Gateway routes (Ocelot) point to:

- `http://customer-service:5001`  
- `http://account-service:5002`

---

## Troubleshooting

| Problem | What to try |
|---------|-------------|
| `docker: command not found` | Install Docker Desktop; restart terminal |
| WSL: cannot connect to Docker | Enable WSL integration in Docker Desktop |
| Port already in use (5001–5004, 5010) | Stop local `dotnet run` instances or change ports in `docker-compose.yml` |
| Build fails / timeout | Run from a local disk path (not slow sync folder); retry `docker compose build --no-cache` |
| `Connection refused` right after start | Wait 30–60s for all services to register; retry request |
| Customer create fails | Check logs: `docker compose logs customer-management` |
| Changes not reflected | Run `docker compose up --build` again |
| Out of disk space | `docker system prune` (removes unused images/containers) |

---

## Docker vs local `dotnet run`

| | Docker Compose | `dotnet run` (5 terminals) |
|--|----------------|---------------------------|
| Needs .NET SDK | No | Yes |
| Startup | One command | Five processes |
| Code change | Rebuild image | Restart project |
| Data persistence | In-memory (lost on container remove) | In-memory (lost on process stop) |
| Best for | Sharing, demos, CI | Active development |

For day-to-day coding, use [steps_run.md](./steps_run.md). For packaging and sharing, use this guide.

---

## Project layout (Docker files)

```
BankingMicroservices/
├── docker-compose.yml
└── docker/
    ├── Dockerfile.api-gateway
    ├── Dockerfile.account
    ├── Dockerfile.customer
    ├── Dockerfile.configuration
    └── Dockerfile.service-discovery
```

Each Dockerfile:

1. Builds with **.NET 10 SDK** image  
2. Publishes for `net10.0`  
3. Runs on **ASP.NET 10** runtime image  
4. Exposes the service port (5000–5004)

---

## Checklist

- [ ] Docker installed and running  
- [ ] `docker compose up --build` completes without errors  
- [ ] `curl http://localhost:5010/gateway/customers` returns JSON  
- [ ] Service Discovery dashboard loads at `http://localhost:5003`  
- [ ] Create customer → deposit → withdraw works via gateway  
- [ ] `docker compose down` stops everything  
