# 📬 Banking Microservices - Postman Collections

Complete Postman collections for testing the Banking Microservices API with automatic environment management and comprehensive test workflows.

## 📁 Files Overview

| File | Purpose | Description |
|------|---------|-------------|
| `Banking_Microservices.postman_collection.json` | **Main Collection** | Complete API collection with all endpoints |
| `Banking_Microservices.postman_environment.json` | **Environment** | URL variables and auto-managed IDs |
| `Banking_Test_Workflow.postman_collection.json` | **Test Workflow** | Automated end-to-end testing |
| `README.md` | **Documentation** | This guide |

## 🚀 Quick Start

### 1. Import Collections

**In Postman:**
1. Click **Import** button
2. Drag & drop all `.json` files or click **Upload Files**
3. Select all 3 files and import

### 2. Set Environment

1. Click environment dropdown (top right)
2. Select **"Banking Microservices Environment"**
3. Verify URLs are correct:
   - API Gateway: `http://localhost:5010`
   - Service Discovery: `http://localhost:5003`

### 3. Start Services

**Docker (Recommended):**
```bash
docker compose up --build
```

**Local Development:**
```bash
# 5 separate terminals:
dotnet run --project src/ServiceDiscovery
dotnet run --project src/ConfigurationService  
dotnet run --project src/CustomerManagementService
dotnet run --project src/AccountManagementService
dotnet run --project src/ApiGateway
```

### 4. Verify Setup

1. Open **Service Discovery Dashboard**: http://localhost:5003
2. Check all 5 services are **healthy** and **registered**
3. Run the **"Get Service Registry (JSON)"** request in Postman

## 📋 Collection Details

### 🏦 Main Collection: "Banking Microservices API"

**Organized by functionality:**

#### **🏦 API Gateway (Recommended)**
- **Customer Operations**: Create, Read, Update, Delete customers
- **Account Operations**: Deposit, Withdraw, Get account details

#### **🔍 Service Discovery & Infrastructure**  
- Real-time dashboard access
- Service registry API
- Service discovery endpoints
- Statistics and monitoring

#### **⚙️ Configuration Service**
- Get configuration for each service
- Centralized settings management

#### **🔧 Direct Service Access (Advanced)**
- Bypass API Gateway for debugging
- Direct microservice calls

#### **📊 Health & Monitoring**
- Health check endpoints for all services
- System status verification

#### **📖 Swagger Documentation**
- Links to Swagger UI for each service
- API documentation access

### 🧪 Test Workflow: "Banking Test Workflow"

**Automated 8-step test sequence:**

1. **Create Test Customer** → Auto-saves `customerId`
2. **Initial Deposit** → Auto-saves `accountId`  
3. **Check Account Balance** → Verifies $500 balance
4. **Withdraw Money** → Reduces balance to $300
5. **Verify Final Balance** → Confirms transaction
6. **Update Customer Info** → Tests customer updates
7. **Test Insufficient Balance** → Validates error handling
8. **Cleanup** → Deletes customer and clears variables

**To run the workflow:**
1. Select **"Banking Test Workflow"** collection
2. Click **"Run collection"** (▶️ button)
3. Click **"Run Banking Test Workflow"**
4. Watch all tests pass! ✅

## 🔧 Environment Variables

**Auto-managed variables:**
- `customerId`: Set automatically when creating customers
- `accountId`: Set automatically during deposits

**Service URLs:**
- `baseUrl`: http://localhost:5010 (API Gateway)
- `serviceDiscoveryUrl`: http://localhost:5003
- `configurationUrl`: http://localhost:5004
- `customerServiceUrl`: http://localhost:5001
- `accountServiceUrl`: http://localhost:5002

## 📊 Testing Scenarios

### Basic Banking Flow
1. **Create Customer** → New customer with auto-created account
2. **Deposit Money** → Add funds to account  
3. **Withdraw Money** → Remove funds (with balance validation)
4. **Check Balance** → Verify current balance
5. **Update Profile** → Modify customer information
6. **Delete Customer** → Remove customer and associated account

### Error Testing
- **Insufficient Balance**: Try to withdraw more than available
- **Invalid Customer**: Use non-existent customer ID
- **Invalid Amount**: Use negative or zero amounts
- **Service Discovery**: Test service unavailability scenarios

### Infrastructure Testing
- **Service Health**: Check all services are running
- **Service Discovery**: Verify service registration
- **Configuration**: Test centralized configuration
- **API Documentation**: Access Swagger for each service

## 🎯 Sample Data

**Customer Creation:**
```json
{
  "name": "John Doe",
  "email": "john.doe@bankingcorp.com", 
  "phone": "+1-555-0123",
  "address": "123 Banking Street, Finance City, FC 12345"
}
```

**Deposit/Withdrawal:**
```json
{
  "customerId": "{{customerId}}",
  "amount": 500.00
}
```

## 🚨 Troubleshooting

| Problem | Solution |
|---------|----------|
| **Requests fail with connection error** | Verify all services are running with `docker compose up` |
| **Environment variables not working** | Select "Banking Microservices Environment" in dropdown |
| **Customer ID not auto-saved** | Check "Create Customer" request has the test script enabled |
| **Service Discovery shows 0 services** | Wait 30 seconds for services to register, refresh dashboard |
| **Port conflicts** | Stop local `dotnet run` processes before using Docker |

## 📖 Additional Resources

- **API Documentation**: [../docs/api.md](../docs/api.md)
- **Docker Setup**: [../docs/docker.md](../docs/docker.md)  
- **Local Development**: [../docs/steps_run.md](../docs/steps_run.md)
- **Service Discovery Dashboard**: http://localhost:5003
- **Project README**: [../README.md](../README.md)

## 🎉 Quick Demo Script

**For presentations or demos:**

1. **Show Dashboard**: Open http://localhost:5003 → Show 5 healthy services
2. **Create Customer**: Run "Create Customer" → Customer ID auto-saved
3. **Deposit Money**: Run "Deposit Money" → Balance becomes $1000
4. **Check Balance**: Run "Get Account Details" → Show customer + account info
5. **Withdraw**: Run "Withdraw Money" → Balance decreases  
6. **Error Demo**: Try withdrawing $10,000 → Show error handling
7. **Run Full Test**: Execute "Banking Test Workflow" → All tests pass

**Total demo time: ~5 minutes** ⏱️

---

*Banking Microservices with .NET Core, Ocelot API Gateway, and Custom Service Discovery* 🏦