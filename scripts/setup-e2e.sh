#!/bin/bash
# Setup completo do ambiente E2E
# Usage: bash setup-e2e.sh

set -e

echo "======================================================================"
echo "🚀 ANALYTICS WORKER - SETUP E2E TESTING"
echo "======================================================================"

# Cores para output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Função para printar com cor
print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

# 1. Verificar pré-requisitos
echo ""
echo "📋 Verificando pré-requisitos..."
echo "----------------------------------------------------------------------"

# Docker
if ! command -v docker &> /dev/null; then
    print_error "Docker não encontrado. Instale o Docker Desktop primeiro."
    exit 1
fi
print_success "Docker instalado"

# Docker Compose
if ! command -v docker-compose &> /dev/null; then
    print_error "Docker Compose não encontrado."
    exit 1
fi
print_success "Docker Compose instalado"

# .NET SDK
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK não encontrado. Instale o .NET 10 SDK."
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
print_success ".NET SDK instalado (versão $DOTNET_VERSION)"

# 2. Iniciar containers
echo ""
echo "🐳 Iniciando containers Docker..."
echo "----------------------------------------------------------------------"

docker-compose down -v 2>/dev/null || true
docker-compose up -d

# Aguardar containers ficarem healthy
echo "⏳ Aguardando containers ficarem prontos..."
sleep 10

# Verificar status
if docker-compose ps | grep -q "Up (healthy)"; then
    print_success "PostgreSQL e RabbitMQ iniciados com sucesso"
else
    print_warning "Containers podem não estar completamente prontos. Verifique: docker-compose ps"
fi

# 3. Restaurar dependências
echo ""
echo "📦 Restaurando dependências .NET..."
echo "----------------------------------------------------------------------"

dotnet restore
print_success "Dependências restauradas"

# 4. Aplicar migrations
echo ""
echo "🗄️  Aplicando migrations no banco de dados..."
echo "----------------------------------------------------------------------"

sleep 5  # Aguardar PostgreSQL estar 100% pronto

dotnet ef database update \
  --project src/Adapters/Outbound/TC.Agro.Analytics.Infrastructure \
  --startup-project src/Adapters/Inbound/TC.Agro.Analytics.Service

print_success "Migrations aplicadas"

# 5. Verificar tabelas
echo ""
echo "🔍 Verificando tabelas criadas..."
echo "----------------------------------------------------------------------"

docker exec tc-agro-postgres psql -U postgres -d tc-agro-analytics-db -c "\dn" 2>/dev/null
docker exec tc-agro-postgres psql -U postgres -d tc-agro-analytics-db -c "\dt analytics.*" 2>/dev/null

print_success "Schema e tabelas verificados"

# 6. Configurar RabbitMQ
echo ""
echo "🐰 Configurando RabbitMQ..."
echo "----------------------------------------------------------------------"

sleep 5  # Aguardar RabbitMQ estar 100% pronto

# Criar exchange
docker exec tc-agro-rabbitmq rabbitmqadmin declare exchange \
  name=analytics.sensor.ingested \
  type=topic \
  durable=true 2>/dev/null || print_warning "Exchange pode já existir"

# Criar queue
docker exec tc-agro-rabbitmq rabbitmqadmin declare queue \
  name=analytics.sensor.ingested.queue \
  durable=true 2>/dev/null || print_warning "Queue pode já existir"

# Criar binding
docker exec tc-agro-rabbitmq rabbitmqadmin declare binding \
  source=analytics.sensor.ingested \
  destination=analytics.sensor.ingested.queue \
  routing_key="#" 2>/dev/null || print_warning "Binding pode já existir"

print_success "RabbitMQ configurado"

# 7. Build da aplicação
echo ""
echo "🔨 Compilando aplicação..."
echo "----------------------------------------------------------------------"

dotnet build
print_success "Build concluído"

# 8. Executar testes unitários
echo ""
echo "🧪 Executando testes unitários..."
echo "----------------------------------------------------------------------"

dotnet test --no-build --verbosity minimal
print_success "Testes unitários passaram"

# 9. Resumo
echo ""
echo "======================================================================"
echo "✅ SETUP CONCLUÍDO COM SUCESSO!"
echo "======================================================================"
echo ""
echo "📊 Status dos Serviços:"
echo "----------------------------------------------------------------------"
docker-compose ps
echo ""
echo "🌐 URLs Importantes:"
echo "----------------------------------------------------------------------"
echo "  RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "  PostgreSQL:          localhost:5432 (postgres/postgres)"
echo "  Analytics API:       http://localhost:5174 (quando iniciado)"
echo ""
echo "🚀 Próximos Passos:"
echo "----------------------------------------------------------------------"
echo "  1. Iniciar aplicação:"
echo "     dotnet run --project src/Adapters/Inbound/TC.Agro.Analytics.Service"
echo ""
echo "  2. Em outro terminal, publicar mensagem de teste:"
echo "     python publish_message.py --scenario high-temp"
echo ""
echo "  3. Verificar logs da aplicação (terminal 1)"
echo ""
echo "  4. Consultar alertas criados:"
echo "     curl http://localhost:5174/alerts/pending | jq"
echo ""
echo "  5. Verificar banco de dados:"
echo "     docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db"
echo "     SELECT * FROM analytics.alerts ORDER BY created_at DESC LIMIT 5;"
echo ""
echo "======================================================================"
