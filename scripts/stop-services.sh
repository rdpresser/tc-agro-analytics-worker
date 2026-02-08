#!/bin/bash

# TC Agro Analytics - Docker Compose Shutdown Script
# Stops all infrastructure services

echo "🛑 Stopping TC Agro Analytics infrastructure..."

# Navigate to scripts directory
cd "$(dirname "$0")"

# Stop services
docker-compose down

echo ""
echo "✅ Services stopped successfully!"
echo ""
echo "💡 To also remove volumes (data will be lost): docker-compose down -v"
