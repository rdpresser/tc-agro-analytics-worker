# Setup completo do ambiente E2E (PowerShell)
# Usage: .\setup-e2e.ps1

$ErrorActionPreference = "Stop"

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "🚀 ANALYTICS WORKER - SETUP E2E TESTING" -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan

function Print-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Print-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Print-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

# 1. Verificar pré-requisitos
Write-Host ""
Write-Host "📋 Verificando pré-requisitos..." -ForegroundColor White
Write-Host "----------------------------------------------------------------------"

# Docker
if (!(Get-Command docker -ErrorAction SilentlyContinue)) {
    Print-Error "Docker não encontrado. Instale o Docker Desktop primeiro."
    exit 1
}
Print-Success "Docker instalado"

# Docker Compose
if (!(Get-Command docker-compose -ErrorAction SilentlyContinue)) {
    Print-Error "Docker Compose não encontrado."
    exit 1
}
Print-Success "Docker Compose instalado"

# .NET SDK
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Print-Error ".NET SDK não encontrado. Instale o .NET 10 SDK."
    exit 1
}
$dotnetVersion = dotnet --version
Print-Success ".NET SDK instalado (versão $dotnetVersion)"

# 2. Iniciar containers
Write-Host ""
Write-Host "🐳 Iniciando containers Docker..." -ForegroundColor White
Write-Host "----------------------------------------------------------------------"

docker-compose down -v 2>$null
docker-compose up -d

# Aguardar containers ficarem healthy
Write-Host "⏳ Aguardando containers ficarem prontos..."
Start-Sleep -Seconds 10

# Verificar status
$containers = docker-compose ps
if ($containers -match "Up \(healthy\)") {
    Print-Success "PostgreSQL e RabbitMQ iniciados com sucesso"
} else {
    Print-Warning "Containers podem não estar completamente prontos. Verifique: docker-compose ps"
}

# 3. Restaurar dependências
Write-Host ""
Write-Host "📦 Restaurando dependências .NET..." -ForegroundColor White
Write-Host "----------------------------------------------------------------------"

dotnet restore
Print-Success "Dependências restauradas"

# 4. Aplicar migrations
Write-Host ""
Write-Host "🗄️  Aplicando migrations no banco de dados..." -ForegroundColor White
Write-Host "----------------------------------------------------------------------"

Start-Sleep -Seconds 5  # Aguardar PostgreSQL estar 100% pronto

dotnet ef database update `
  --project src/Adapters/Outbound/TC.Agro.Analytics.Infrastructure `
  --startup-project src/Adapters/Inbound/TC.Agro.Analytics.Service

Print-Success "Migrations aplicadas"

# 5. Verificar tabelas
Write-Host ""
Write-Host "🔍 Verificando tabelas criadas..." -ForegroundColor White
Write-Host "----------------------------------------------------------------------"

docker exec tc-agro-postgres psql -U postgres -d tc-agro-analytics-db -c "\dn" 2>$null
docker exec tc-agro-postgres psql -U postgres -d tc-agro-analytics-db -c "\dt analytics.*" 2>$null

Print-Success "Schema e tabelas verificados"

# 6. Configurar RabbitMQ
Write-Host ""
Write-Host "🐰 Configurando RabbitMQ..." -ForegroundColor White
Write-Host "----------------------------------------------------------------------"

Start-Sleep -Seconds 5  # Aguardar RabbitMQ estar 100% pronto

# Criar exchange
try {
    docker exec tc-agro-rabbitmq rabbitmqadmin declare exchange `
      name=analytics.sensor.ingested `
      type=topic `
      durable=true 2>$null
} catch {
    Print-Warning "Exchange pode já existir"
}

# Criar queue
try {
    docker exec tc-agro-rabbitmq rabbitmqadmin declare queue `
      name=analytics.sensor.ingested.queue `
      durable=true 2>$null
} catch {
    Print-Warning "Queue pode já existir"
}

# Criar binding
try {
    docker exec tc-agro-rabbitmq rabbitmqadmin declare binding `
      source=analytics.sensor.ingested `
      destination=analytics.sensor.ingested.queue `
      routing_key="#" 2>$null
} catch {
    Print-Warning "Binding pode já existir"
}

Print-Success "RabbitMQ configurado"

# 7. Build da aplicação
Write-Host ""
Write-Host "🔨 Compilando aplicação..." -ForegroundColor White
Write-Host "----------------------------------------------------------------------"

dotnet build
Print-Success "Build concluído"

# 8. Executar testes unitários
Write-Host ""
Write-Host "🧪 Executando testes unitários..." -ForegroundColor White
Write-Host "----------------------------------------------------------------------"

dotnet test --no-build --verbosity minimal
Print-Success "Testes unitários passaram"

# 9. Resumo
Write-Host ""
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "✅ SETUP CONCLUÍDO COM SUCESSO!" -ForegroundColor Green
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Status dos Serviços:" -ForegroundColor White
Write-Host "----------------------------------------------------------------------"
docker-compose ps
Write-Host ""
Write-Host "🌐 URLs Importantes:" -ForegroundColor White
Write-Host "----------------------------------------------------------------------"
Write-Host "  RabbitMQ Management: http://localhost:15672 (guest/guest)"
Write-Host "  PostgreSQL:          localhost:5432 (postgres/postgres)"
Write-Host "  Analytics API:       http://localhost:5174 (quando iniciado)"
Write-Host ""
Write-Host "🚀 Próximos Passos:" -ForegroundColor White
Write-Host "----------------------------------------------------------------------"
Write-Host "  1. Iniciar aplicação:"
Write-Host "     dotnet run --project src/Adapters/Inbound/TC.Agro.Analytics.Service"
Write-Host ""
Write-Host "  2. Em outro terminal, publicar mensagem de teste:"
Write-Host "     python publish_message.py --scenario high-temp"
Write-Host ""
Write-Host "  3. Verificar logs da aplicação (terminal 1)"
Write-Host ""
Write-Host "  4. Consultar alertas criados:"
Write-Host "     curl http://localhost:5174/alerts/pending | jq"
Write-Host ""
Write-Host "  5. Verificar banco de dados:"
Write-Host "     docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db"
Write-Host "     SELECT * FROM analytics.alerts ORDER BY created_at DESC LIMIT 5;"
Write-Host ""
Write-Host "======================================================================" -ForegroundColor Cyan
