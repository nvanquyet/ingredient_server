#!/bin/bash

# Docker Migration Helper Script
# Run migrations inside Docker container

set -e

CONTAINER_NAME="ingredientserver-app"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

print_success() {
    echo -e "${GREEN}✓${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

# Check if container is running
check_container() {
    if ! docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_error "Container '$CONTAINER_NAME' is not running"
        echo "Please start the container first: docker compose up -d"
        exit 1
    fi
}

# Show usage
show_usage() {
    cat << EOF
Docker Migration Helper Script

Usage: $0 [command] [options]

Commands:
    add <name>              Create a new migration inside Docker container
    remove [name]           Remove migration inside Docker container
    list                    List all migrations
    update                  Apply pending migrations to database
    status                  Show migration status
    exec <command>          Execute custom dotnet ef command
    help                    Show this help message

Examples:
    $0 add AddNewTable
    $0 update
    $0 list
    $0 exec migrations list

EOF
}

# Main script
check_container

case "${1:-help}" in
    add)
        if [ -z "$2" ]; then
            print_error "Migration name is required"
            echo "Usage: $0 add <migration-name>"
            exit 1
        fi
        print_info "Creating migration '$2' in container..."
        docker exec -it "$CONTAINER_NAME" dotnet ef migrations add "$2" \
            --project /app \
            --startup-project /app/API
        print_success "Migration '$2' created successfully"
        ;;
    
    remove)
        if [ -z "$2" ]; then
            print_info "Removing last migration..."
            docker exec -it "$CONTAINER_NAME" dotnet ef migrations remove \
                --project /app \
                --startup-project /app/API
        else
            print_info "Removing migration: $2"
            docker exec -it "$CONTAINER_NAME" dotnet ef migrations remove "$2" \
                --project /app \
                --startup-project /app/API
        fi
        print_success "Migration removed successfully"
        ;;
    
    list)
        print_info "Listing migrations..."
        docker exec "$CONTAINER_NAME" dotnet ef migrations list \
            --project /app \
            --startup-project /app/API || print_error "Failed to list migrations"
        ;;
    
    status)
        print_info "Checking migration status..."
        docker exec "$CONTAINER_NAME" dotnet ef migrations list \
            --project /app \
            --startup-project /app/API || print_error "Failed to check status"
        ;;
    
    update)
        print_info "Applying migrations to database..."
        docker exec "$CONTAINER_NAME" dotnet ef database update \
            --project /app \
            --startup-project /app/API
        print_success "Database updated successfully"
        ;;
    
    exec)
        shift
        if [ -z "$1" ]; then
            print_error "Command is required"
            echo "Usage: $0 exec <dotnet ef command>"
            exit 1
        fi
        print_info "Executing: dotnet ef $*"
        docker exec -it "$CONTAINER_NAME" dotnet ef "$@" \
            --project /app \
            --startup-project /app/API
        ;;
    
    help|--help|-h)
        show_usage
        ;;
    
    *)
        print_error "Unknown command: $1"
        echo
        show_usage
        exit 1
        ;;
esac

