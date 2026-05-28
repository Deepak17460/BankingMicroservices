#!/bin/bash

# Banking Microservices - Run All Services
# Starts all 5 microservices in the correct order with proper delays

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Project root directory
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo -e "${CYAN}=================================================${NC}"
echo -e "${CYAN}    Banking Microservices Startup Script        ${NC}"
echo -e "${CYAN}=================================================${NC}"
echo -e "${YELLOW}Project Root: ${PROJECT_ROOT}${NC}"
echo ""

# Function to check if .NET is available
check_dotnet() {
    if ! command -v dotnet &> /dev/null; then
        echo -e "${RED}Error: .NET SDK not found!${NC}"
        echo -e "${YELLOW}Please install .NET 8.0+ SDK from: https://dotnet.microsoft.com/download${NC}"
        exit 1
    fi
    
    local dotnet_version=$(dotnet --version)
    echo -e "${GREEN}✓ .NET SDK found: ${dotnet_version}${NC}"
}

# Function to start a service in background
start_service() {
    local service_name="$1"
    local project_path="$2" 
    local port="$3"
    local delay="$4"
    
    if [ "$delay" -gt 0 ]; then
        echo -e "${YELLOW}Waiting ${delay} seconds before starting ${service_name}...${NC}"
        sleep $delay
    fi
    
    echo -e "${GREEN}Starting ${service_name} on port ${port}...${NC}"
    
    # Create service-specific log directory
    local service_log_dir="logs/${service_name,,}"
    mkdir -p "$service_log_dir"
    
    cd "$PROJECT_ROOT"
    nohup dotnet run --project "$project_path" > "$service_log_dir/${service_name,,}.log" 2>&1 &
    local pid=$!
    
    echo $pid > "$service_log_dir/${service_name,,}.pid"
    echo -e "${BLUE}→ ${service_name} started with PID: ${pid}${NC}"
}

# Function to wait for service to be ready
wait_for_service() {
    local service_name="$1"
    local url="$2"
    local max_attempts="${3:-30}"  # Default 30, but allow override
    local attempt=0
    
    echo -e "${YELLOW}Waiting for ${service_name} to be ready...${NC}"
    
    while [ $attempt -lt $max_attempts ]; do
        if curl -s -f "$url/health" > /dev/null 2>&1; then
            echo -e "${GREEN}✓ ${service_name} is ready!${NC}"
            return 0
        fi
        
        attempt=$((attempt + 1))
        echo -n "."
        sleep 2
        
        # Show debug info every 15 attempts (30 seconds)
        if [ $((attempt % 15)) -eq 0 ] && [ $attempt -lt $max_attempts ]; then
            echo -e "\n${BLUE}Debug: Checking ${url}/health (attempt ${attempt}/${max_attempts})${NC}"
            curl -s "$url/health" 2>&1 | head -3 | while read line; do
                echo -e "${BLUE}  Response: $line${NC}"
            done
        fi
    done
    
    local timeout_seconds=$((max_attempts * 2))
    echo -e "${RED}✗ ${service_name} failed to start within ${timeout_seconds} seconds${NC}"
    return 1
}

# Function to cleanup on exit
cleanup() {
    echo -e "\n${YELLOW}Stopping all services...${NC}"
    
    if [ -d "logs" ]; then
        for service_dir in logs/*/; do
            if [ -d "$service_dir" ]; then
                for pid_file in "$service_dir"*.pid; do
                    if [ -f "$pid_file" ]; then
                        local pid=$(cat "$pid_file")
                        local service_name=$(basename "$pid_file" .pid)
                        
                        if kill -0 "$pid" 2>/dev/null; then
                            echo -e "${YELLOW}Stopping $service_name (PID: $pid)...${NC}"
                            kill "$pid"
                            sleep 1
                            
                            # Force kill if still running
                            if kill -0 "$pid" 2>/dev/null; then
                                kill -9 "$pid" 2>/dev/null
                            fi
                        fi
                        
                        rm -f "$pid_file"
                    fi
                done
            fi
        done
        
        # Also check for legacy pid files in logs root
        for pid_file in logs/*.pid; do
            if [ -f "$pid_file" ]; then
                local pid=$(cat "$pid_file")
                local service_name=$(basename "$pid_file" .pid)
                
                if kill -0 "$pid" 2>/dev/null; then
                    echo -e "${YELLOW}Stopping $service_name (PID: $pid)...${NC}"
                    kill "$pid"
                    sleep 1
                    
                    if kill -0 "$pid" 2>/dev/null; then
                        kill -9 "$pid" 2>/dev/null
                    fi
                fi
                
                rm -f "$pid_file"
            fi
        done
    fi
    
    echo -e "${GREEN}All services stopped.${NC}"
    exit 0
}

# Set up signal handlers
trap cleanup SIGINT SIGTERM

# Main execution
main() {
    echo -e "${CYAN}=== Pre-flight Checks ===${NC}"
    check_dotnet
    
    # Create logs directory
    mkdir -p logs
    
    # Clean up any existing PID files
    rm -f logs/*.pid
    
    echo -e "\n${CYAN}=== Starting Infrastructure Services ===${NC}"
    
    # 1. Service Discovery (must start first)
    start_service "Service Discovery" "src/ServiceDiscovery/ServiceDiscovery.csproj" "5003" 0
    wait_for_service "Service Discovery" "http://localhost:5003"
    
    # 2. Configuration Service
    start_service "Configuration Service" "src/ConfigurationService/ConfigurationService.csproj" "5004" 3
    wait_for_service "Configuration Service" "http://localhost:5004"
    
    echo -e "\n${CYAN}=== Starting Business Services ===${NC}"
    
    # 3. Customer Management Service
    start_service "Customer Management" "src/CustomerManagementService/CustomerManagementService.csproj" "5001" 2
    wait_for_service "Customer Management" "http://localhost:5001"
    
    # 4. Account Management Service  
    start_service "Account Management" "src/AccountManagementService/AccountManagementService.csproj" "5002" 2
    wait_for_service "Account Management" "http://localhost:5002"
    
    echo -e "\n${CYAN}=== Starting API Gateway ===${NC}"
    
    # 5. API Gateway (must start last)
    start_service "API Gateway" "src/ApiGateway/ApiGateway.csproj" "5010" 2
    
    # Give API Gateway extra time to initialize Ocelot
    echo -e "${YELLOW}Giving API Gateway extra time for Ocelot initialization...${NC}"
    sleep 5
    
    wait_for_service "API Gateway" "http://localhost:5010" 60  # 120 seconds timeout for complex Ocelot startup
    
    echo -e "\n${GREEN}=================================================${NC}"
    echo -e "${GREEN}    🎉 ALL SERVICES STARTED SUCCESSFULLY! 🎉    ${NC}"
    echo -e "${GREEN}=================================================${NC}"
    echo ""
    echo -e "${CYAN}Service URLs:${NC}"
    echo -e "${BLUE}  • Service Discovery Dashboard: http://localhost:5003${NC}"
    echo -e "${BLUE}  • API Gateway Swagger:         http://localhost:5010/swagger${NC}"
    echo -e "${BLUE}  • Customer Service Swagger:    http://localhost:5001/swagger${NC}"
    echo -e "${BLUE}  • Account Service Swagger:     http://localhost:5002/swagger${NC}"
    echo -e "${BLUE}  • Configuration Service:       http://localhost:5004/swagger${NC}"
    echo ""
    echo -e "${CYAN}Automated Test:${NC}"
    echo -e "${BLUE}  curl http://localhost:5010/gateway/customers${NC}"
    echo ""
    echo -e "${CYAN}Logs Location: ${PROJECT_ROOT}/logs/${NC}"
    echo -e "${YELLOW}Press Ctrl+C to stop all services...${NC}"
    echo ""
    
    # Keep script running and show live service status
    while true; do
        echo -e "${CYAN}=== Service Status Check $(date) ===${NC}"
        
        services=("Service Discovery:5003" "Configuration:5004" "Customer:5001" "Account:5002" "API Gateway:5010")
        all_healthy=true
        
        for service_info in "${services[@]}"; do
            IFS=':' read -r service_name port <<< "$service_info"
            
            if curl -s -f "http://localhost:$port/health" > /dev/null 2>&1; then
                echo -e "${GREEN}✓ $service_name (port $port) - Healthy${NC}"
            else
                echo -e "${RED}✗ $service_name (port $port) - Not responding${NC}"
                all_healthy=false
            fi
        done
        
        if $all_healthy; then
            echo -e "${GREEN}All services are healthy! 🎉${NC}"
        else
            echo -e "${YELLOW}Some services may need attention. Check logs in ./logs/${NC}"
        fi
        
        echo ""
        sleep 30  # Check every 30 seconds
    done
}

# Show usage if help requested
if [[ "$1" == "--help" || "$1" == "-h" ]]; then
    echo "Banking Microservices Startup Script"
    echo ""
    echo "Usage:"
    echo "  ./run_all.sh                 Start all services"
    echo "  ./run_all.sh --help          Show this help"
    echo ""
    echo "Services started:"
    echo "  1. Service Discovery (port 5003) - Infrastructure"
    echo "  2. Configuration Service (port 5004) - Infrastructure"  
    echo "  3. Customer Management (port 5001) - Business Logic"
    echo "  4. Account Management (port 5002) - Business Logic"
    echo "  5. API Gateway (port 5010) - Entry Point"
    echo ""
    echo "Requirements:"
    echo "  - .NET 8.0+ SDK installed"
    echo "  - Ports 5001-5004, 5010 available"
    echo ""
    exit 0
fi

# Run main function
main