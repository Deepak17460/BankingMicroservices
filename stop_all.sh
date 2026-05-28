#!/bin/bash

# Banking Microservices - Stop All Services
# Gracefully stops all running microservices

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo -e "${CYAN}=================================================${NC}"
echo -e "${CYAN}    Banking Microservices Stop Script           ${NC}"
echo -e "${CYAN}=================================================${NC}"

# Function to stop service by PID file
stop_service_by_pid() {
    local service_name="$1"
    local pid_file="logs/${service_name,,}/${service_name,,}.pid"
    local legacy_pid_file="logs/${service_name,,}.pid"
    
    # Check new structure first, then fallback to legacy
    if [ -f "$pid_file" ]; then
        local pid=$(cat "$pid_file")
        
        if kill -0 "$pid" 2>/dev/null; then
            echo -e "${YELLOW}Stopping $service_name (PID: $pid)...${NC}"
            kill "$pid"
            sleep 2
            
            # Check if still running, force kill if necessary
            if kill -0 "$pid" 2>/dev/null; then
                echo -e "${YELLOW}Force killing $service_name...${NC}"
                kill -9 "$pid" 2>/dev/null
            fi
            
            echo -e "${GREEN}✓ $service_name stopped${NC}"
        else
            echo -e "${BLUE}$service_name was not running${NC}"
        fi
        
        rm -f "$pid_file"
    elif [ -f "$legacy_pid_file" ]; then
        # Handle legacy pid file location
        local pid=$(cat "$legacy_pid_file")
        
        if kill -0 "$pid" 2>/dev/null; then
            echo -e "${YELLOW}Stopping $service_name (PID: $pid) [legacy location]...${NC}"
            kill "$pid"
            sleep 2
            
            if kill -0 "$pid" 2>/dev/null; then
                echo -e "${YELLOW}Force killing $service_name...${NC}"
                kill -9 "$pid" 2>/dev/null
            fi
            
            echo -e "${GREEN}✓ $service_name stopped${NC}"
        else
            echo -e "${BLUE}$service_name was not running${NC}"
        fi
        
        rm -f "$legacy_pid_file"
    else
        echo -e "${BLUE}No PID file found for $service_name${NC}"
    fi
}

# Function to stop services by name pattern
stop_by_process_name() {
    echo -e "${YELLOW}Looking for dotnet processes...${NC}"
    
    # Get PIDs of dotnet processes running our services
    local pids=$(pgrep -f "dotnet.*run.*project.*src/" 2>/dev/null || true)
    
    if [ -n "$pids" ]; then
        echo -e "${YELLOW}Found running services, stopping...${NC}"
        echo "$pids" | while read -r pid; do
            if [ -n "$pid" ]; then
                local process_info=$(ps -p "$pid" -o cmd= 2>/dev/null || echo "Unknown")
                echo -e "${YELLOW}Stopping PID $pid: ${process_info}${NC}"
                kill "$pid" 2>/dev/null || true
            fi
        done
        
        sleep 3
        
        # Force kill any remaining processes
        local remaining=$(pgrep -f "dotnet.*run.*project.*src/" 2>/dev/null || true)
        if [ -n "$remaining" ]; then
            echo -e "${YELLOW}Force killing remaining processes...${NC}"
            echo "$remaining" | while read -r pid; do
                if [ -n "$pid" ]; then
                    kill -9 "$pid" 2>/dev/null || true
                fi
            done
        fi
    else
        echo -e "${BLUE}No running services found${NC}"
    fi
}

# Main stop function
main() {
    cd "$PROJECT_ROOT"
    
    echo -e "${CYAN}=== Stopping Services ===${NC}"
    
    # Try to stop by PID files first (more graceful)
    if [ -d "logs" ]; then
        stop_service_by_pid "API Gateway"
        stop_service_by_pid "Account Management"  
        stop_service_by_pid "Customer Management"
        stop_service_by_pid "Configuration Service"
        stop_service_by_pid "Service Discovery"
    fi
    
    # Fallback: stop by process name pattern
    stop_by_process_name
    
    # Clean up log directory
    if [ -d "logs" ]; then
        # Clean up new structure PID files
        find logs/ -name "*.pid" -type f -delete 2>/dev/null
        echo -e "${BLUE}Cleaned up PID files${NC}"
    fi
    
    echo -e "\n${GREEN}=================================================${NC}"
    echo -e "${GREEN}    All services stopped successfully!          ${NC}"
    echo -e "${GREEN}=================================================${NC}"
    
    # Verify ports are free
    echo -e "\n${CYAN}Verifying ports are available:${NC}"
    local ports=(5001 5002 5003 5004 5010)
    
    for port in "${ports[@]}"; do
        if ! lsof -i :$port >/dev/null 2>&1; then
            echo -e "${GREEN}✓ Port $port is available${NC}"
        else
            echo -e "${YELLOW}⚠ Port $port still in use${NC}"
        fi
    done
}

# Show usage if help requested
if [[ "$1" == "--help" || "$1" == "-h" ]]; then
    echo "Banking Microservices Stop Script"
    echo ""
    echo "Usage:"
    echo "  ./stop_all.sh                Stop all services"
    echo "  ./stop_all.sh --help         Show this help"
    echo ""
    echo "This script will:"
    echo "  1. Gracefully stop all services using PID files"
    echo "  2. Force kill any remaining dotnet processes"
    echo "  3. Clean up PID files and logs"
    echo "  4. Verify ports are available"
    echo ""
    exit 0
fi

# Run main function
main