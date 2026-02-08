# TC Agro Analytics - PowerShell Helper Functions
# Source this file: . .\docker-helpers.ps1

function Start-TCAgroServices {
    <#
    .SYNOPSIS
    Start all TC Agro Analytics infrastructure services
    #>
    Write-Host "🚀 Starting TC Agro Analytics services..." -ForegroundColor Green
    docker-compose up -d
    Get-TCAgroStatus
}

function Start-TCAgroMinimal {
    <#
    .SYNOPSIS
    Start only essential services (PostgreSQL & RabbitMQ)
    #>
    Write-Host "🚀 Starting minimal services..." -ForegroundColor Green
    docker-compose -f docker-compose.minimal.yml up -d
    docker-compose -f docker-compose.minimal.yml ps
}

function Stop-TCAgroServices {
    <#
    .SYNOPSIS
    Stop all TC Agro Analytics services
    #>
    Write-Host "🛑 Stopping TC Agro Analytics services..." -ForegroundColor Yellow
    docker-compose down
    Write-Host "✅ Services stopped!" -ForegroundColor Green
}

function Remove-TCAgroVolumes {
    <#
    .SYNOPSIS
    Stop services and remove all volumes (⚠️  data will be lost)
    #>
    $confirmation = Read-Host "⚠️  This will delete all data! Are you sure? (yes/no)"
    if ($confirmation -eq 'yes') {
        Write-Host "🗑️  Removing services and volumes..." -ForegroundColor Red
        docker-compose down -v
        Write-Host "✅ Volumes removed!" -ForegroundColor Green
    } else {
        Write-Host "Cancelled." -ForegroundColor Yellow
    }
}

function Get-TCAgroLogs {
    <#
    .SYNOPSIS
    Show logs from all services
    #>
    docker-compose logs -f
}

function Get-TCAgroStatus {
    <#
    .SYNOPSIS
    Show status of all services
    #>
    Write-Host "`n📊 Service Status:" -ForegroundColor Blue
    docker-compose ps
    
    Write-Host "`n🔗 Service URLs:" -ForegroundColor Blue
    Write-Host "  PostgreSQL:          localhost:5432" -ForegroundColor Cyan
    Write-Host "  RabbitMQ AMQP:       localhost:5672" -ForegroundColor Cyan
    Write-Host "  RabbitMQ Management: http://localhost:15672" -ForegroundColor Cyan
    Write-Host "  Redis:               localhost:6379" -ForegroundColor Cyan
    Write-Host "  Grafana:             http://localhost:3000" -ForegroundColor Cyan
    Write-Host "  pgAdmin:             http://localhost:5050" -ForegroundColor Cyan
}

function Restart-TCAgroServices {
    <#
    .SYNOPSIS
    Restart all services
    #>
    Write-Host "🔄 Restarting services..." -ForegroundColor Yellow
    docker-compose restart
    Write-Host "✅ Services restarted!" -ForegroundColor Green
}

function Update-TCAgroImages {
    <#
    .SYNOPSIS
    Pull latest images and recreate containers
    #>
    Write-Host "📥 Pulling latest images..." -ForegroundColor Blue
    docker-compose pull
    Write-Host "🔨 Recreating containers..." -ForegroundColor Blue
    docker-compose up -d --force-recreate
    Write-Host "✅ Services updated!" -ForegroundColor Green
}

function Enter-PostgresShell {
    <#
    .SYNOPSIS
    Open PostgreSQL shell
    #>
    Write-Host "🐘 Connecting to PostgreSQL..." -ForegroundColor Blue
    docker exec -it tc-agro-postgres psql -U postgres -d tc_agro_analytics
}

function Backup-PostgresDatabase {
    <#
    .SYNOPSIS
    Backup PostgreSQL database
    #>
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFile = "backup_$timestamp.sql"
    Write-Host "💾 Backing up database to $backupFile..." -ForegroundColor Blue
    docker exec tc-agro-postgres pg_dump -U postgres tc_agro_analytics > $backupFile
    Write-Host "✅ Backup created: $backupFile" -ForegroundColor Green
}

function Restore-PostgresDatabase {
    <#
    .SYNOPSIS
    Restore PostgreSQL database from backup
    .PARAMETER BackupFile
    Path to the backup SQL file
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$BackupFile
    )
    
    if (-not (Test-Path $BackupFile)) {
        Write-Host "❌ Error: Backup file not found: $BackupFile" -ForegroundColor Red
        return
    }
    
    Write-Host "🔄 Restoring database from $BackupFile..." -ForegroundColor Yellow
    Get-Content $BackupFile | docker exec -i tc-agro-postgres psql -U postgres tc_agro_analytics
    Write-Host "✅ Database restored!" -ForegroundColor Green
}

function Get-RabbitMQStatus {
    <#
    .SYNOPSIS
    Show RabbitMQ status
    #>
    Write-Host "🐰 RabbitMQ Status:" -ForegroundColor Blue
    docker exec tc-agro-rabbitmq rabbitmqctl status
}

function Get-RabbitMQQueues {
    <#
    .SYNOPSIS
    List RabbitMQ queues
    #>
    Write-Host "📬 RabbitMQ Queues:" -ForegroundColor Blue
    docker exec tc-agro-rabbitmq rabbitmqctl list_queues
}

function Enter-RedisShell {
    <#
    .SYNOPSIS
    Open Redis CLI
    #>
    Write-Host "🔴 Connecting to Redis..." -ForegroundColor Blue
    docker exec -it tc-agro-redis redis-cli
}

function Get-TCAgroHealth {
    <#
    .SYNOPSIS
    Check health status of all services
    #>
    Write-Host "🏥 Health Check:" -ForegroundColor Blue
    docker ps --format "table {{.Names}}`t{{.Status}}"
}

function Get-TCAgroStats {
    <#
    .SYNOPSIS
    Show resource usage of containers
    #>
    Write-Host "📈 Resource Usage:" -ForegroundColor Blue
    docker stats --no-stream --format "table {{.Name}}`t{{.CPUPerc}}`t{{.MemUsage}}"
}

function Show-TCAgroHelp {
    <#
    .SYNOPSIS
    Show available commands
    #>
    Write-Host "`nTC Agro Analytics - Docker Helper Functions`n" -ForegroundColor Blue
    Write-Host "Available commands:" -ForegroundColor Green
    Write-Host "  Start-TCAgroServices       - Start all services" -ForegroundColor Yellow
    Write-Host "  Start-TCAgroMinimal        - Start only PostgreSQL & RabbitMQ" -ForegroundColor Yellow
    Write-Host "  Stop-TCAgroServices        - Stop all services" -ForegroundColor Yellow
    Write-Host "  Remove-TCAgroVolumes       - Stop and remove volumes" -ForegroundColor Yellow
    Write-Host "  Get-TCAgroLogs             - Show logs" -ForegroundColor Yellow
    Write-Host "  Get-TCAgroStatus           - Show service status" -ForegroundColor Yellow
    Write-Host "  Restart-TCAgroServices     - Restart services" -ForegroundColor Yellow
    Write-Host "  Update-TCAgroImages        - Update images" -ForegroundColor Yellow
    Write-Host "  Enter-PostgresShell        - PostgreSQL shell" -ForegroundColor Yellow
    Write-Host "  Test-DatabaseSetup         - Verify database setup" -ForegroundColor Yellow
    Write-Host "  Backup-PostgresDatabase    - Backup database" -ForegroundColor Yellow
    Write-Host "  Restore-PostgresDatabase   - Restore database" -ForegroundColor Yellow
    Write-Host "  Get-RabbitMQStatus         - RabbitMQ status" -ForegroundColor Yellow
    Write-Host "  Get-RabbitMQQueues         - List queues" -ForegroundColor Yellow
    Write-Host "  Enter-RedisShell           - Redis CLI" -ForegroundColor Yellow
    Write-Host "  Get-TCAgroHealth           - Health check" -ForegroundColor Yellow
    Write-Host "  Get-TCAgroStats            - Resource usage" -ForegroundColor Yellow
    Write-Host "`nUsage: . .\docker-helpers.ps1`n" -ForegroundColor Cyan
}

function Test-DatabaseSetup {
    <#
    .SYNOPSIS
    Verify PostgreSQL database setup
    #>
    Write-Host "🔍 Checking PostgreSQL connection..." -ForegroundColor Blue

    # Check if container is running
    $running = docker ps --filter "name=tc-agro-postgres" --format "{{.Names}}"
    if (-not $running) {
        Write-Host "❌ Error: PostgreSQL container is not running!" -ForegroundColor Red
        Write-Host "Run: Start-TCAgroServices" -ForegroundColor Yellow
        return
    }

    Write-Host "✅ PostgreSQL container is running" -ForegroundColor Green

    # Check database existence
    Write-Host "`n🔍 Checking if database 'tc_agro_analytics' exists..." -ForegroundColor Blue
    $dbExists = docker exec tc-agro-postgres psql -U postgres -tAc "SELECT 1 FROM pg_database WHERE datname='tc_agro_analytics'"

    if ($dbExists -eq "1") {
        Write-Host "✅ Database 'tc_agro_analytics' exists!" -ForegroundColor Green
    } else {
        Write-Host "❌ Database 'tc_agro_analytics' does NOT exist!" -ForegroundColor Red
        Write-Host "`nCreating database..." -ForegroundColor Yellow
        docker exec tc-agro-postgres psql -U postgres -c "CREATE DATABASE tc_agro_analytics;"
        docker exec tc-agro-postgres psql -U postgres -d tc_agro_analytics -c "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";"
        docker exec tc-agro-postgres psql -U postgres -d tc_agro_analytics -c "CREATE EXTENSION IF NOT EXISTS \"pg_trgm\";"
        Write-Host "✅ Database created with extensions!" -ForegroundColor Green
    }

    # Show database info
    Write-Host "`n📊 Database Information:" -ForegroundColor Blue
    docker exec tc-agro-postgres psql -U postgres -c "\l tc_agro_analytics"

    # Show extensions
    Write-Host "`n🔧 Installed Extensions:" -ForegroundColor Blue
    docker exec tc-agro-postgres psql -U postgres -d tc_agro_analytics -c "\dx"

    # Test connection
    Write-Host "`n🔌 Testing connection..." -ForegroundColor Blue
    docker exec tc-agro-postgres psql -U postgres -d tc_agro_analytics -c "SELECT version();"

    Write-Host "`n✅ Database setup verification complete!" -ForegroundColor Green
    Write-Host "`nConnection String:" -ForegroundColor Cyan
    Write-Host "Host=localhost;Port=5432;Database=tc_agro_analytics;Username=postgres;Password=postgres" -ForegroundColor White
}

# Show help on load
Show-TCAgroHelp
