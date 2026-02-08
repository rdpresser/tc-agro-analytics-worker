#!/bin/bash

# TC Agro Analytics - Database Setup Verification
# Verifies if the database was created correctly

echo "🔍 Checking PostgreSQL connection..."

# Check if container is running
if ! docker ps | grep -q tc-agro-postgres; then
    echo "❌ Error: PostgreSQL container is not running!"
    echo "Run: docker-compose up -d tc-agro-postgres"
    exit 1
fi

echo "✅ PostgreSQL container is running"

# Check database existence
echo ""
echo "🔍 Checking if database 'tc_agro_analytics' exists..."

DB_EXISTS=$(docker exec tc-agro-postgres psql -U postgres -tAc "SELECT 1 FROM pg_database WHERE datname='tc_agro_analytics'")

if [ "$DB_EXISTS" = "1" ]; then
    echo "✅ Database 'tc_agro_analytics' exists!"
else
    echo "❌ Database 'tc_agro_analytics' does NOT exist!"
    echo ""
    echo "Creating database..."
    docker exec tc-agro-postgres psql -U postgres -c "CREATE DATABASE tc_agro_analytics;"
    echo "✅ Database created!"
fi

# Show database info
echo ""
echo "📊 Database Information:"
docker exec tc-agro-postgres psql -U postgres -c "\l tc_agro_analytics"

# Show extensions
echo ""
echo "🔧 Installed Extensions:"
docker exec tc-agro-postgres psql -U postgres -d tc_agro_analytics -c "\dx"

# Test connection
echo ""
echo "🔌 Testing connection..."
docker exec tc-agro-postgres psql -U postgres -d tc_agro_analytics -c "SELECT version();"

echo ""
echo "✅ Database setup verification complete!"
echo ""
echo "Connection String:"
echo "Host=localhost;Port=5432;Database=tc_agro_analytics;Username=postgres;Password=postgres"
