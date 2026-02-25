# Script to remove all Supabase references from README files
# Author: GitHub Copilot
# Date: 2025-02-01

Write-Host "🔄 Removing Supabase references from README files..." -ForegroundColor Cyan

$files = @("README.md", "README_EN.md")
$replacements = @(
    @{
        Old = "- **PostgreSQL 16+** - Relational database (Supabase cloud)"
        New = "- **PostgreSQL 16+** - Relational database"
    },
    @{
        Old = "    E -->|Create/Update| F[(PostgreSQL<br/>Supabase)]"
        New = "    E -->|Create/Update| F[(PostgreSQL<br/>Database)]"
    },
    @{
        Old = @"
#### Production (Cloud)
- **Supabase PostgreSQL** - Managed database in the cloud
- **CloudAMQP** - Managed RabbitMQ (or other provider)
"@
        New = @"
#### Production (Cloud)
- **PostgreSQL** - Managed database in the cloud (Azure Database, AWS RDS, or other provider)
- **RabbitMQ** - Managed message broker (CloudAMQP, Azure Service Bus, or other provider)
"@
    },
    @{
        Old = '    "DefaultConnection": "Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=${SUPABASE_DB_PASSWORD};SSL Mode=Require"'
        New = '    "DefaultConnection": "Host=your-db-server.com;Port=5432;Database=tc-agro-analytics-db;Username=postgres;Password=${DB_PASSWORD};SSL Mode=Require"'
    }
)

foreach ($file in $files) {
    $filePath = Join-Path $PSScriptRoot "..\$file"
    
    if (Test-Path $filePath) {
        Write-Host "📝 Processing $file..." -ForegroundColor Yellow
        
        # Read file content
        $content = Get-Content -Path $filePath -Raw -Encoding UTF8
        
        # Apply all replacements
        $changesMade = 0
        foreach ($replacement in $replacements) {
            if ($content.Contains($replacement.Old)) {
                $content = $content.Replace($replacement.Old, $replacement.New)
                $changesMade++
            }
        }
        
        if ($changesMade -gt 0) {
            # Write back to file
            $content | Set-Content -Path $filePath -Encoding UTF8 -NoNewline
            Write-Host "   ✅ $changesMade replacement(s) applied to $file" -ForegroundColor Green
        } else {
            Write-Host "   ℹ️  No changes needed in $file" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ⚠️  File not found: $file" -ForegroundColor Red
    }
}

Write-Host "`n✨ Done! All Supabase references have been removed." -ForegroundColor Green
Write-Host "📋 Changed files: $($files -join ', ')" -ForegroundColor Cyan
