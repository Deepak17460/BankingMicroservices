# Banking Microservices Documentation

Complete documentation for the .NET microservices banking application with SOLID principles, Ocelot API Gateway, and custom Service Discovery.

| Document | Description |
|----------|-------------|
| [steps_run.md](./steps_run.md) | Local development setup - Install SDK, build, run services with .NET CLI |
| [docker.md](./docker.md) | **Recommended:** Docker Compose deployment (no .NET SDK required) |
| [api.md](./api.md) | Complete API reference with examples, Swagger endpoints, and architecture |

## Quick Start

**Docker (Recommended):**
```bash
docker compose up --build
```

**Local Development:**
```bash
dotnet run --project src/ServiceDiscovery
dotnet run --project src/ConfigurationService  
dotnet run --project src/CustomerManagementService
dotnet run --project src/AccountManagementService
dotnet run --project src/ApiGateway
```

## Key URLs

| Service | URL | Purpose |
|---------|-----|---------|
| **Service Discovery Dashboard** | `http://localhost:5003` | Real-time Eureka-style registry |
| **API Gateway** | `http://localhost:5010/swagger` | Main entry point with docs |
| **Customer API** | `http://localhost:5001/swagger` | Customer management |
| **Account API** | `http://localhost:5002/swagger` | Banking operations |

**Project overview:** [../README.md](../README.md)
