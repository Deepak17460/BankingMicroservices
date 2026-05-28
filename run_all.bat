@echo off
setlocal enabledelayedexpansion

REM Banking Microservices - Run All Services (Windows)
REM Starts all 5 microservices in the correct order

title Banking Microservices Startup

echo ===============================================
echo     Banking Microservices Startup Script        
echo ===============================================
echo Project Root: %~dp0
echo.

REM Check if .NET is available
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK not found!
    echo Please install .NET 8.0+ SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo [OK] .NET SDK found: %DOTNET_VERSION%
echo.

REM Create logs directory
if not exist "logs" mkdir logs

REM Clean up any existing processes (optional - uncomment if needed)
REM taskkill /f /im dotnet.exe >nul 2>&1

echo === Starting Infrastructure Services ===
echo.

REM 1. Service Discovery (start first)
echo [1/5] Starting Service Discovery on port 5003...
start "Service Discovery" cmd /k "cd /d %~dp0 && dotnet run --project src/ServiceDiscovery/ServiceDiscovery.csproj"
timeout /t 8 /nobreak >nul

REM 2. Configuration Service  
echo [2/5] Starting Configuration Service on port 5004...
start "Configuration Service" cmd /k "cd /d %~dp0 && dotnet run --project src/ConfigurationService/ConfigurationService.csproj"
timeout /t 6 /nobreak >nul

echo.
echo === Starting Business Services ===
echo.

REM 3. Customer Management Service
echo [3/5] Starting Customer Management on port 5001...
start "Customer Management" cmd /k "cd /d %~dp0 && dotnet run --project src/CustomerManagementService/CustomerManagementService.csproj"
timeout /t 5 /nobreak >nul

REM 4. Account Management Service
echo [4/5] Starting Account Management on port 5002...
start "Account Management" cmd /k "cd /d %~dp0 && dotnet run --project src/AccountManagementService/AccountManagementService.csproj"
timeout /t 5 /nobreak >nul

echo.
echo === Starting API Gateway ===
echo.

REM 5. API Gateway (start last)
echo [5/5] Starting API Gateway on port 5010...
start "API Gateway" cmd /k "cd /d %~dp0 && dotnet run --project src/ApiGateway/ApiGateway.csproj"

echo.
echo ===============================================
echo     ALL SERVICES STARTED SUCCESSFULLY!    
echo ===============================================
echo.
echo Service URLs:
echo   • Service Discovery Dashboard: http://localhost:5003
echo   • API Gateway Swagger:         http://localhost:5010/swagger
echo   • Customer Service Swagger:    http://localhost:5001/swagger
echo   • Account Service Swagger:     http://localhost:5002/swagger
echo   • Configuration Service:       http://localhost:5004/swagger
echo.
echo Quick Test:
echo   curl http://localhost:5010/gateway/customers
echo.
echo Wait 30-60 seconds for all services to register...

REM Wait a bit then open Service Discovery dashboard
timeout /t 15 /nobreak >nul
echo Opening Service Discovery Dashboard...
start http://localhost:5003

echo.
echo All services are starting in separate windows.
echo Close this window when you want to stop all services.
echo.
pause