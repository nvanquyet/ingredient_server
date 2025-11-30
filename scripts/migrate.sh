#!/bin/bash

# Migration Helper Script for Ingredient Server
# Usage: ./scripts/migrate.sh [command] [options]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_DIR="$PROJECT_ROOT/IngredientServer"
API_DIR="$PROJECT_DIR/API"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Helper functions
print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

print_success() {
    echo -e "${GREEN}✓${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

# Check if running in Docker
is_docker() {
    [ -f /.dockerenv ] || [ -n "$DOCKER_CONTAINER" ]
}

# Run migration command
run_migration() {
    local command=$1
    shift
    
    if is_docker; then
        print_info "Running in Docker container..."
        dotnet ef migrations $command --project "$PROJECT_DIR" --startup-project "$API_DIR" "$@"
    else
        print_info "Running locally..."
        if command -v dotnet &> /dev/null; then
            dotnet ef migrations $command --project "$PROJECT_DIR" --startup-project "$API_DIR" "$@"
        else
            print_error "dotnet CLI not found. Please install .NET SDK or run this script inside Docker container."
            exit 1
        fi
    fi
}

# Show usage
show_usage() {
    cat << EOF
Migration Helper Script for Ingredient Server

Usage: $0 [command] [options]

Commands:
    add <name>              Create a new migration with the specified name
    remove [name]           Remove the last migration (or specified migration)
    list                    List all migrations
    status                  Show migration status
    update                  Apply pending migrations to database
    script                  Generate SQL script for migrations
    help                    Show this help message

Examples:
    $0 add AddNewTable
    $0 update
    $0 list
    $0 status

Note: If running locally, ensure .NET SDK is installed.
      If running in Docker, the script will detect and use the container environment.

EOF
}

# Main script
case "${1:-help}" in
    add)
        if [ -z "$2" ]; then
            print_error "Migration name is required"
            echo "Usage: $0 add <migration-name>"
            exit 1
        fi
        print_info "Creating new migration: $2"
        run_migration "add" "$2"
        print_success "Migration '$2' created successfully"
        ;;
    
    remove)
        if [ -z "$2" ]; then
            print_warning "Removing last migration..."
            run_migration "remove"
        else
            print_warning "Removing migration: $2"
            run_migration "remove" "$2"
        fi
        print_success "Migration removed successfully"
        ;;
    
    list)
        print_info "Listing all migrations:"
        if is_docker; then
            dotnet ef migrations list --project "$PROJECT_DIR" --startup-project "$API_DIR" 2>/dev/null || print_warning "No migrations found or database not accessible"
        else
            if command -v dotnet &> /dev/null; then
                dotnet ef migrations list --project "$PROJECT_DIR" --startup-project "$API_DIR" 2>/dev/null || print_warning "No migrations found or database not accessible"
            else
                print_error "dotnet CLI not found"
                exit 1
            fi
        fi
        ;;
    
    status)
        print_info "Checking migration status..."
        if is_docker; then
            dotnet ef migrations list --project "$PROJECT_DIR" --startup-project "$API_DIR" 2>/dev/null || print_warning "Cannot check status - database may not be accessible"
        else
            if command -v dotnet &> /dev/null; then
                dotnet ef migrations list --project "$PROJECT_DIR" --startup-project "$API_DIR" 2>/dev/null || print_warning "Cannot check status - database may not be accessible"
            else
                print_error "dotnet CLI not found"
                exit 1
            fi
        fi
        ;;
    
    update)
        print_info "Applying pending migrations to database..."
        if is_docker; then
            dotnet ef database update --project "$PROJECT_DIR" --startup-project "$API_DIR"
        else
            if command -v dotnet &> /dev/null; then
                print_warning "Updating database locally..."
                print_warning "Make sure database connection string is correct in appsettings.json"
                read -p "Continue? (y/n) " -n 1 -r
                echo
                if [[ $REPLY =~ ^[Yy]$ ]]; then
                    dotnet ef database update --project "$PROJECT_DIR" --startup-project "$API_DIR"
                else
                    print_info "Cancelled"
                    exit 0
                fi
            else
                print_error "dotnet CLI not found"
                exit 1
            fi
        fi
        print_success "Database updated successfully"
        ;;
    
    script)
        OUTPUT_FILE="${2:-migration_script.sql}"
        print_info "Generating SQL script to: $OUTPUT_FILE"
        run_migration "script" --output "$OUTPUT_FILE"
        print_success "SQL script generated: $OUTPUT_FILE"
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

