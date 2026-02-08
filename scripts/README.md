# TC Agro Analytics - Docker Infrastructure Scripts

Scripts para gerenciar a infraestrutura do serviço Analytics Worker usando Docker Compose.

## 📋 Pré-requisitos

- Docker Desktop instalado e rodando
- Docker Compose v2.0+

## 🚀 Serviços Disponíveis

### Serviços Principais
- **PostgreSQL 16** (porta 5432) - Banco de dados principal
- **RabbitMQ 4** (portas 5672, 15672) - Message broker

### Serviços Opcionais
- **Redis 7** (porta 6379) - Cache distribuído
- **Grafana** (porta 3000) - Monitoramento e dashboards
- **pgAdmin 4** (porta 5050) - Administração do PostgreSQL

## 🎯 Como Usar

### Iniciar Serviços

**Windows:**
```bash
cd scripts
start-services.bat
```

**Linux/Mac:**
```bash
cd scripts
chmod +x start-services.sh
./start-services.sh
```

**Docker Compose direto:**
```bash
cd scripts
docker-compose up -d
```

### Parar Serviços

**Windows:**
```bash
stop-services.bat
```

**Linux/Mac:**
```bash
./stop-services.sh
```

**Docker Compose direto:**
```bash
docker-compose down
```

### Parar e Remover Volumes (⚠️ Apaga todos os dados)

```bash
docker-compose down -v
```

## 🔍 Monitoramento

### Ver Logs de Todos os Serviços
```bash
docker-compose logs -f
```

### Ver Logs de um Serviço Específico
```bash
docker-compose logs -f tc-agro-postgres
docker-compose logs -f tc-agro-rabbitmq
docker-compose logs -f tc-agro-redis
```

### Verificar Status dos Serviços
```bash
docker-compose ps
```

### Verificar Health Checks
```bash
docker ps --format "table {{.Names}}\t{{.Status}}"
```

## 🔗 URLs de Acesso

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| PostgreSQL | `localhost:5432` | user: `postgres`, password: `postgres` |
| RabbitMQ AMQP | `localhost:5672` | user: `guest`, password: `guest` |
| RabbitMQ Management | http://localhost:15672 | user: `guest`, password: `guest` |
| Redis | `localhost:6379` | (sem autenticação) |
| Grafana | http://localhost:3000 | user: `admin`, password: `admin` |
| pgAdmin | http://localhost:5050 | email: `admin@tcagro.com`, password: `admin` |

## 🗄️ Connection Strings

### PostgreSQL (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=tc_agro_analytics;Username=postgres;Password=postgres"
  }
}
```

### RabbitMQ (appsettings.json)
```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  }
}
```

### Redis (appsettings.json)
```json
{
  "Redis": {
    "Configuration": "localhost:6379"
  }
}
```

## 🔧 Comandos Úteis

### Recriar um Serviço Específico
```bash
docker-compose up -d --force-recreate tc-agro-postgres
```

### Executar Migrations no PostgreSQL
```bash
# Dentro do diretório do projeto
dotnet ef database update --project src/Adapters/Outbound/TC.Agro.Analytics.Infrastructure
```

### Acessar PostgreSQL via CLI
```bash
docker exec -it tc-agro-postgres psql -U postgres -d tc_agro_analytics
```

### Acessar RabbitMQ via CLI
```bash
docker exec -it tc-agro-rabbitmq rabbitmqctl status
```

### Backup do PostgreSQL
```bash
docker exec tc-agro-postgres pg_dump -U postgres tc_agro_analytics > backup.sql
```

### Restaurar Backup do PostgreSQL
```bash
docker exec -i tc-agro-postgres psql -U postgres tc_agro_analytics < backup.sql
```

## 📦 Volumes Persistentes

Os dados são armazenados em volumes Docker:

- `postgres_data` - Dados do PostgreSQL
- `rabbitmq_data` - Dados do RabbitMQ
- `rabbitmq_logs` - Logs do RabbitMQ
- `redis_data` - Dados do Redis
- `grafana_data` - Configurações do Grafana
- `pgadmin_data` - Configurações do pgAdmin

### Listar Volumes
```bash
docker volume ls | grep tc-agro
```

### Inspecionar Volume
```bash
docker volume inspect scripts_postgres_data
```

## 🐛 Troubleshooting

### Porta 5432 já está em uso
```bash
# Ver processo usando a porta
netstat -ano | findstr :5432   # Windows
lsof -i :5432                   # Linux/Mac

# Parar PostgreSQL local ou mudar porta no docker-compose.yml
ports:
  - "5433:5432"  # Porta externa alterada
```

### Container não inicia
```bash
# Ver logs detalhados
docker-compose logs tc-agro-postgres

# Recriar container
docker-compose down
docker-compose up -d --force-recreate
```

### Banco de dados não inicializa
```bash
# Remover volume e recriar
docker-compose down -v
docker-compose up -d
```

## 🔄 Atualizar Imagens

```bash
# Pull de novas versões
docker-compose pull

# Recriar containers com novas imagens
docker-compose up -d --force-recreate
```

## 📝 Notas

- **Desenvolvimento Local:** Todos os serviços estão configurados para ambiente de desenvolvimento
- **Produção:** Use variáveis de ambiente e secrets adequados
- **Performance:** Ajuste os recursos (CPU/RAM) no Docker Desktop se necessário
- **Segurança:** Altere as senhas padrão em ambientes não-locais

## 🤝 Contribuindo

Ao adicionar novos serviços:
1. Adicione no `docker-compose.yml`
2. Configure health checks
3. Atualize este README
4. Adicione variáveis de ambiente necessárias

## 📚 Referências

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
- [RabbitMQ Docker Image](https://hub.docker.com/_/rabbitmq)
- [Redis Docker Image](https://hub.docker.com/_/redis)
