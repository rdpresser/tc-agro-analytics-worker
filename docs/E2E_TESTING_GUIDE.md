# 🧪 **GUIA COMPLETO - TESTES E2E COM RABBITMQ + POSTGRESQL**

**Objetivo:** Testar fluxo completo desde consumo de mensagem RabbitMQ até persistência no PostgreSQL.

---

## 📋 **PRÉ-REQUISITOS**

- ✅ Docker Desktop instalado e rodando
- ✅ .NET 10 SDK instalado
- ✅ Visual Studio Code ou Rider
- ✅ Git Bash ou PowerShell

---

## 🐳 **PASSO 1: CONFIGURAR DOCKER COMPOSE**

### **1.1 Criar docker-compose.yml na raiz do projeto:**

```yaml
version: '3.8'

services:
  # PostgreSQL Database
  postgres:
    image: postgres:16-alpine
    container_name: tc-agro-postgres
    environment:
      POSTGRES_DB: tc-agro-analytics-db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_HOST_AUTH_METHOD: trust
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
      - ./init-db.sql:/docker-entrypoint-initdb.d/init-db.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - tc-agro-network

  # RabbitMQ Message Broker
  rabbitmq:
    image: rabbitmq:4-management-alpine
    container_name: tc-agro-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
      RABBITMQ_DEFAULT_VHOST: /
    ports:
      - "5672:5672"   # AMQP port
      - "15672:15672" # Management UI
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - tc-agro-network

volumes:
  postgres-data:

networks:
  tc-agro-network:
    driver: bridge
```

### **1.2 Criar init-db.sql na raiz do projeto:**

```sql
-- Script de inicialização do banco de dados
-- Cria o schema analytics se não existir

CREATE SCHEMA IF NOT EXISTS analytics;

-- Grant permissions
GRANT ALL PRIVILEGES ON SCHEMA analytics TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA analytics TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA analytics TO postgres;

-- Configurações de performance
ALTER SYSTEM SET max_connections = 200;
ALTER SYSTEM SET shared_buffers = '256MB';
ALTER SYSTEM SET effective_cache_size = '1GB';
ALTER SYSTEM SET maintenance_work_mem = '64MB';
ALTER SYSTEM SET checkpoint_completion_target = 0.9;
ALTER SYSTEM SET wal_buffers = '16MB';
ALTER SYSTEM SET default_statistics_target = 100;
ALTER SYSTEM SET random_page_cost = 1.1;
ALTER SYSTEM SET effective_io_concurrency = 200;
ALTER SYSTEM SET work_mem = '4MB';
ALTER SYSTEM SET min_wal_size = '1GB';
ALTER SYSTEM SET max_wal_size = '4GB';

-- Log para debug
\echo 'Database tc-agro-analytics-db initialized successfully!'
```

### **1.3 Iniciar containers:**

```bash
# Na raiz do projeto (onde está docker-compose.yml)
docker-compose up -d

# Verificar se os containers estão rodando
docker-compose ps

# Saída esperada:
# NAME                  STATUS         PORTS
# tc-agro-postgres      Up (healthy)   0.0.0.0:5432->5432/tcp
# tc-agro-rabbitmq      Up (healthy)   0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
```

### **1.4 Verificar logs:**

```bash
# PostgreSQL
docker-compose logs postgres

# RabbitMQ
docker-compose logs rabbitmq

# Ambos em tempo real
docker-compose logs -f
```

---

## 🗄️ **PASSO 2: CONFIGURAR BANCO DE DADOS**

### **2.1 Aplicar Migrations:**

```bash
# Restaurar dependências
dotnet restore

# Aplicar migrations
dotnet ef database update \
  --project src/Adapters/Outbound/TC.Agro.Analytics.Infrastructure \
  --startup-project src/Adapters/Inbound/TC.Agro.Analytics.Service

# Saída esperada:
# Build succeeded.
# Applying migration '20260201211711_AddAlertsReadModel'...
# Done.
```

### **2.2 Verificar tabelas criadas:**

```bash
# Conectar no PostgreSQL via docker
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db

# No psql, executar:
\dn  # Listar schemas
\dt analytics.*  # Listar tabelas do schema analytics

# Saída esperada:
# Schema   | Name         | Type  | Owner
# ---------+--------------+-------+--------
# analytics| alerts       | table | postgres
# analytics| mt_events    | table | postgres (Marten)
# analytics| mt_streams   | table | postgres (Marten)
# analytics| mt_doc_outbox| table | postgres (Marten)

# Verificar índices
\di analytics.*

# Saída esperada: 8+ índices na tabela alerts

# Sair do psql
\q
```

### **2.3 Inserir dados de teste (opcional):**

```bash
# Copiar arquivo SQL para dentro do container
docker cp test-data-e2e.sql tc-agro-postgres:/tmp/

# Executar script
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db -f /tmp/test-data-e2e.sql

# Verificar dados
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db -c "SELECT COUNT(*) FROM analytics.alerts;"

# Saída esperada: 10 (ou 0 se não executou o script)
```

---

## 🐰 **PASSO 3: CONFIGURAR RABBITMQ**

### **3.1 Acessar Management UI:**

```
URL: http://localhost:15672
User: guest
Password: guest
```

### **3.2 Criar Exchange e Queues manualmente:**

**Opção A: Via Management UI:**

1. **Exchanges Tab** → **Add a new exchange**
   - Name: `analytics.sensor.ingested`
   - Type: `topic`
   - Durability: `Durable`
   - Click **Add exchange**

2. **Queues Tab** → **Add a new queue**
   - Name: `analytics.sensor.ingested.queue`
   - Durability: `Durable`
   - Click **Add queue**

3. **Bindings** → **Add binding from this queue**
   - From exchange: `analytics.sensor.ingested`
   - Routing key: `#` (aceita tudo)
   - Click **Bind**

**Opção B: Via CLI (dentro do container):**

```bash
# Entrar no container RabbitMQ
docker exec -it tc-agro-rabbitmq bash

# Criar exchange
rabbitmqadmin declare exchange \
  name=analytics.sensor.ingested \
  type=topic \
  durable=true

# Criar queue
rabbitmqadmin declare queue \
  name=analytics.sensor.ingested.queue \
  durable=true

# Criar binding
rabbitmqadmin declare binding \
  source=analytics.sensor.ingested \
  destination=analytics.sensor.ingested.queue \
  routing_key="#"

# Verificar
rabbitmqadmin list exchanges
rabbitmqadmin list queues
rabbitmqadmin list bindings

# Sair do container
exit
```

### **3.3 Verificar configuração:**

No Management UI:
- **Exchanges** → Deve ter `analytics.sensor.ingested`
- **Queues** → Deve ter `analytics.sensor.ingested.queue` com binding

---

## ⚙️ **PASSO 4: CONFIGURAR APLICAÇÃO**

### **4.1 Atualizar appsettings.Development.json:**

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Marten": "Information",
        "Wolverine": "Information"
      }
    }
  },
  "Database": {
    "Postgres": {
      "Host": "localhost",
      "Port": 5432,
      "Database": "tc-agro-analytics-db",
      "UserName": "postgres",
      "Password": "postgres"
    }
  },
  "Messaging": {
    "RabbitMQ": {
      "Host": "localhost",
      "Port": 5672,
      "UserName": "guest",
      "Password": "guest"
    }
  },
  "AlertThresholds": {
    "MaxTemperature": 30.0,
    "MinSoilMoisture": 25.0,
    "MinBatteryLevel": 20.0
  }
}
```

### **4.2 Verificar Program.cs:**

```csharp
// Certifique-se de que Wolverine está configurado
// Se NÃO estiver, você precisa adicionar:

builder.Host.UseWolverine(opts =>
{
    // Configurar RabbitMQ
    opts.UseRabbitMq(rabbitMq =>
    {
        var config = builder.Configuration.GetSection("Messaging:RabbitMQ");
        rabbitMq.HostName = config["Host"];
        rabbitMq.Port = int.Parse(config["Port"]);
        rabbitMq.UserName = config["UserName"];
        rabbitMq.Password = config["Password"];
    });

    // Configurar listener para a fila
    opts.ListenToRabbitQueue("analytics.sensor.ingested.queue");
    
    // Configurar Marten Outbox
    opts.UseMartenOutbox();
});
```

**⚠️ IMPORTANTE:** Se Wolverine NÃO estiver configurado no Program.cs, você precisará adicionar essa configuração!

---

## 🚀 **PASSO 5: EXECUTAR APLICAÇÃO**

### **5.1 Build:**

```bash
dotnet build
```

### **5.2 Executar aplicação:**

```bash
# Com logs detalhados
dotnet run --project src/Adapters/Inbound/TC.Agro.Analytics.Service

# Saída esperada:
# info: Wolverine.Runtime.WolverineRuntime[0]
#       Wolverine messaging service is starting
# info: Wolverine.RabbitMQ.RabbitMqTransport[0]
#       Connected to RabbitMQ at localhost:5672
# info: Wolverine.Runtime.WolverineRuntime[0]
#       Listening to queue 'analytics.sensor.ingested.queue'
# info: Microsoft.Hosting.Lifetime[0]
#       Application started. Press Ctrl+C to shut down.
# info: Microsoft.Hosting.Lifetime[0]
#       Hosting environment: Development
# info: Microsoft.Hosting.Lifetime[0]
#       Content root path: C:\path\to\project
```

### **5.3 Verificar Health Check:**

```bash
# Em outro terminal
curl http://localhost:5174/health

# Saída esperada:
{
  "status": "Healthy",
  "timestamp": "2026-02-01T10:00:00Z",
  "service": "Analytics Worker Service"
}
```

---

## 📨 **PASSO 6: PUBLICAR MENSAGEM DE TESTE NO RABBITMQ**

### **6.1 Via Management UI:**

1. Acesse **Queues** → `analytics.sensor.ingested.queue`
2. Clique em **Publish message**
3. **Payload:** Cole o JSON abaixo
4. **Properties:** Deixe padrão
5. Clique **Publish message**

**Payload de Teste:**

```json
{
  "EventId": "12345678-1234-1234-1234-123456789012",
  "AggregateId": "87654321-4321-4321-4321-210987654321",
  "OccurredOn": "2026-02-01T10:00:00Z",
  "EventName": "SensorIngestedIntegrationEvent",
  "RelatedIds": null,
  "SensorId": "SENSOR-TEST-001",
  "PlotId": "ae57f8d7-d491-4899-bb39-30124093e683",
  "Time": "2026-02-01T09:55:00Z",
  "Temperature": 42.5,
  "Humidity": 65.0,
  "SoilMoisture": 35.0,
  "Rainfall": 2.5,
  "BatteryLevel": 85.0
}
```

**📌 IMPORTANTE:** 
- `Temperature: 42.5` está **ACIMA** do threshold (30°C em dev) → Deve gerar alerta!
- `SoilMoisture: 35.0` está **OK** (acima de 25%) → Não gera alerta
- `BatteryLevel: 85.0` está **OK** (acima de 20%) → Não gera alerta

### **6.2 Via Python Script (alternativa):**

```python
# publish_message.py
import pika
import json
from datetime import datetime, timezone

# Conectar no RabbitMQ
connection = pika.BlockingConnection(
    pika.ConnectionParameters('localhost', 5672, '/', 
                              pika.PlainCredentials('guest', 'guest'))
)
channel = connection.channel()

# Mensagem de teste
message = {
    "EventId": "12345678-1234-1234-1234-123456789012",
    "AggregateId": "87654321-4321-4321-4321-210987654321",
    "OccurredOn": datetime.now(timezone.utc).isoformat(),
    "EventName": "SensorIngestedIntegrationEvent",
    "RelatedIds": None,
    "SensorId": "SENSOR-TEST-001",
    "PlotId": "ae57f8d7-d491-4899-bb39-30124093e683",
    "Time": datetime.now(timezone.utc).isoformat(),
    "Temperature": 42.5,
    "Humidity": 65.0,
    "SoilMoisture": 35.0,
    "Rainfall": 2.5,
    "BatteryLevel": 85.0
}

# Publicar
channel.basic_publish(
    exchange='',
    routing_key='analytics.sensor.ingested.queue',
    body=json.dumps(message),
    properties=pika.BasicProperties(
        delivery_mode=2,  # make message persistent
        content_type='application/json'
    )
)

print(" [x] Mensagem enviada!")
connection.close()
```

```bash
# Instalar dependência
pip install pika

# Executar
python publish_message.py
```

### **6.3 Via curl (alternativa - usando RabbitMQ API):**

```bash
curl -u guest:guest -X POST \
  http://localhost:15672/api/exchanges/%2F/analytics.sensor.ingested/publish \
  -H 'Content-Type: application/json' \
  -d '{
    "properties": {
      "delivery_mode": 2,
      "content_type": "application/json"
    },
    "routing_key": "sensor.data",
    "payload": "{\"EventId\":\"12345678-1234-1234-1234-123456789012\",\"AggregateId\":\"87654321-4321-4321-4321-210987654321\",\"OccurredOn\":\"2026-02-01T10:00:00Z\",\"EventName\":\"SensorIngestedIntegrationEvent\",\"SensorId\":\"SENSOR-TEST-001\",\"PlotId\":\"ae57f8d7-d491-4899-bb39-30124093e683\",\"Time\":\"2026-02-01T09:55:00Z\",\"Temperature\":42.5,\"Humidity\":65.0,\"SoilMoisture\":35.0,\"Rainfall\":2.5,\"BatteryLevel\":85.0}",
    "payload_encoding": "string"
  }'
```

---

## ✅ **PASSO 7: VERIFICAR PROCESSAMENTO**

### **7.1 Verificar logs da aplicação:**

No terminal onde a aplicação está rodando, você deve ver:

```
info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedHandler[0]
      Processing SensorIngestedIntegrationEvent for Sensor SENSOR-TEST-001, Plot ae57f8d7-d491-4899-bb39-30124093e683

warn: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedHandler[0]
      High temperature alert triggered for Sensor SENSOR-TEST-001. Temperature: 42.5°C (Threshold: 30.0°C)

info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedHandler[0]
      Sensor reading processed successfully for Sensor SENSOR-TEST-001, Plot ae57f8d7-d491-4899-bb39-30124093e683
```

### **7.2 Verificar RabbitMQ Management UI:**

1. Acesse **Queues** → `analytics.sensor.ingested.queue`
2. **Messages** → Deve estar **0** (mensagem foi consumida)
3. **Message rates** → Deve mostrar "Ack" (acknowledge)

### **7.3 Verificar banco de dados - Event Store:**

```bash
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db

# No psql:
SELECT 
  id, 
  type, 
  stream_id,
  data->>'SensorId' as sensor,
  data->>'Temperature' as temp,
  timestamp
FROM analytics.mt_events
ORDER BY timestamp DESC
LIMIT 5;

# Saída esperada:
# id  | type                         | stream_id               | sensor          | temp | timestamp
# ----+------------------------------+-------------------------+-----------------+------+-------------------
# ... | sensor_reading_created       | sensor-reading-87654... | SENSOR-TEST-001 | 42.5 | 2026-02-01 10:00
# ... | high_temperature_detected    | sensor-reading-87654... | SENSOR-TEST-001 | 42.5 | 2026-02-01 10:00

\q
```

### **7.4 Verificar banco de dados - Read Model (Alerts):**

```bash
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db

# No psql:
SELECT 
  id,
  sensor_id,
  alert_type,
  message,
  status,
  severity,
  value,
  threshold,
  created_at
FROM analytics.alerts
ORDER BY created_at DESC
LIMIT 5;

# Saída esperada:
# id   | sensor_id       | alert_type       | message                        | status  | severity | value | threshold | created_at
# -----+-----------------+------------------+--------------------------------+---------+----------+-------+-----------+-------------------
# ...  | SENSOR-TEST-001 | HighTemperature  | High temperature detected: 42.5°C | Pending | High     | 42.5  | 30.0      | 2026-02-01 10:00

\q
```

---

## 🌐 **PASSO 8: TESTAR API REST**

### **8.1 Testar /health:**

```bash
curl http://localhost:5174/health
```

### **8.2 Testar /alerts/pending:**

```bash
curl http://localhost:5174/alerts/pending | jq

# Saída esperada:
{
  "alerts": [
    {
      "id": "...",
      "sensorId": "SENSOR-TEST-001",
      "plotId": "ae57f8d7-d491-4899-bb39-30124093e683",
      "alertType": "HighTemperature",
      "message": "High temperature detected: 42.5°C",
      "status": "Pending",
      "severity": "High",
      "value": 42.5,
      "threshold": 30.0,
      "createdAt": "2026-02-01T10:00:00Z"
    }
  ],
  "totalCount": 1
}
```

### **8.3 Testar /alerts/history/{plotId}:**

```bash
curl "http://localhost:5174/alerts/history/ae57f8d7-d491-4899-bb39-30124093e683?days=30" | jq

# Saída esperada: Lista de alertas daquele plot
```

### **8.4 Testar /alerts/status/{plotId}:**

```bash
curl "http://localhost:5174/alerts/status/ae57f8d7-d491-4899-bb39-30124093e683" | jq

# Saída esperada:
{
  "plotId": "ae57f8d7-d491-4899-bb39-30124093e683",
  "overallStatus": "Warning",
  "pendingAlertsCount": 1,
  "totalAlertsLast24Hours": 1,
  "totalAlertsLast7Days": 1,
  "alertsByType": {
    "HighTemperature": 1
  },
  "alertsBySeverity": {
    "High": 1
  },
  "mostRecentAlert": {
    "id": "...",
    "sensorId": "SENSOR-TEST-001",
    ...
  }
}
```

---

## 🧪 **PASSO 9: TESTAR CENÁRIOS ADICIONAIS**

### **Cenário 1: Low Soil Moisture Alert**

```json
{
  "EventId": "22222222-2222-2222-2222-222222222222",
  "AggregateId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "OccurredOn": "2026-02-01T10:05:00Z",
  "EventName": "SensorIngestedIntegrationEvent",
  "SensorId": "SENSOR-TEST-002",
  "PlotId": "ae57f8d7-d491-4899-bb39-30124093e683",
  "Time": "2026-02-01T10:00:00Z",
  "Temperature": 25.0,
  "Humidity": 45.0,
  "SoilMoisture": 15.0,
  "Rainfall": 0.0,
  "BatteryLevel": 90.0
}
```

**Resultado esperado:**
- ✅ Alerta de `LowSoilMoisture` gerado (15% < 25%)
- ✅ Severity: High ou Critical (dependendo da diferença)

### **Cenário 2: Battery Low Warning**

```json
{
  "EventId": "33333333-3333-3333-3333-333333333333",
  "AggregateId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "OccurredOn": "2026-02-01T10:10:00Z",
  "EventName": "SensorIngestedIntegrationEvent",
  "SensorId": "SENSOR-TEST-003",
  "PlotId": "ae57f8d7-d491-4899-bb39-30124093e683",
  "Time": "2026-02-01T10:05:00Z",
  "Temperature": 28.0,
  "Humidity": 60.0,
  "SoilMoisture": 30.0,
  "Rainfall": 1.0,
  "BatteryLevel": 10.0
}
```

**Resultado esperado:**
- ✅ Alerta de `LowBattery` gerado (10% < 20%)
- ✅ Severity: High ou Critical

### **Cenário 3: Mensagem Duplicada**

Publique a MESMA mensagem do Cenário 1 novamente (mesmo AggregateId).

**Resultado esperado:**
- ✅ Mensagem é consumida
- ⚠️ Log: "Duplicate event detected"
- ❌ NÃO cria novo registro no Event Store
- ❌ NÃO cria novo alerta

### **Cenário 4: Múltiplos Alertas**

```json
{
  "EventId": "44444444-4444-4444-4444-444444444444",
  "AggregateId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
  "OccurredOn": "2026-02-01T10:15:00Z",
  "EventName": "SensorIngestedIntegrationEvent",
  "SensorId": "SENSOR-TEST-004",
  "PlotId": "ae57f8d7-d491-4899-bb39-30124093e683",
  "Time": "2026-02-01T10:10:00Z",
  "Temperature": 45.0,
  "Humidity": 40.0,
  "SoilMoisture": 12.0,
  "Rainfall": 0.0,
  "BatteryLevel": 8.0
}
```

**Resultado esperado:**
- ✅ **3 ALERTAS** gerados simultaneamente:
  - HighTemperature (45°C > 30°C)
  - LowSoilMoisture (12% < 25%)
  - LowBattery (8% < 20%)

---

## 📊 **PASSO 10: VALIDAÇÃO COMPLETA**

### **Checklist de Validação:**

- [ ] **Docker Containers:**
  - [ ] PostgreSQL rodando e saudável
  - [ ] RabbitMQ rodando e saudável
  
- [ ] **Banco de Dados:**
  - [ ] Schema `analytics` criado
  - [ ] Tabela `analytics.alerts` existe
  - [ ] Tabelas Marten (`mt_events`, `mt_streams`, `mt_doc_outbox`) existem
  - [ ] 8+ índices na tabela alerts
  
- [ ] **RabbitMQ:**
  - [ ] Exchange `analytics.sensor.ingested` criado
  - [ ] Queue `analytics.sensor.ingested.queue` criada
  - [ ] Binding configurado
  
- [ ] **Aplicação:**
  - [ ] Build sem erros
  - [ ] Aplicação inicia sem erros
  - [ ] Conecta no PostgreSQL
  - [ ] Conecta no RabbitMQ
  - [ ] Escuta a fila corretamente
  
- [ ] **Processamento:**
  - [ ] Mensagem é consumida da fila
  - [ ] Domain Events são persistidos no Event Store
  - [ ] Alertas são projetados para Read Model
  - [ ] Integration Events são publicados (Outbox)
  
- [ ] **API:**
  - [ ] `/health` retorna Healthy
  - [ ] `/alerts/pending` retorna alertas
  - [ ] `/alerts/history/{plotId}` retorna histórico
  - [ ] `/alerts/status/{plotId}` retorna agregações
  
- [ ] **Logs:**
  - [ ] Logs de processamento aparecem
  - [ ] Logs de alertas aparecem
  - [ ] Sem erros críticos

---

## 🐛 **TROUBLESHOOTING**

### **Problema: Aplicação não conecta no RabbitMQ**

```bash
# Verificar se RabbitMQ está rodando
docker-compose ps rabbitmq

# Verificar logs
docker-compose logs rabbitmq

# Restartar RabbitMQ
docker-compose restart rabbitmq
```

### **Problema: Aplicação não conecta no PostgreSQL**

```bash
# Verificar se PostgreSQL está rodando
docker-compose ps postgres

# Testar conexão
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db -c "SELECT 1;"

# Restartar PostgreSQL
docker-compose restart postgres
```

### **Problema: Mensagem não é consumida**

```bash
# Verificar se a fila está bound ao exchange
# RabbitMQ Management UI → Queues → analytics.sensor.ingested.queue → Bindings

# Verificar se a aplicação está escutando
# Logs devem mostrar: "Listening to queue 'analytics.sensor.ingested.queue'"
```

### **Problema: Migrations não aplicam**

```bash
# Limpar e recriar banco
docker-compose down -v
docker-compose up -d
dotnet ef database update --project src/Adapters/Outbound/TC.Agro.Analytics.Infrastructure --startup-project src/Adapters/Inbound/TC.Agro.Analytics.Service
```

---

## 🎯 **RESULTADO ESPERADO FINAL:**

```
╔══════════════════════════════════════════════════════════╗
║               TESTE E2E BEM-SUCEDIDO! ✅                ║
╠══════════════════════════════════════════════════════════╣
║                                                          ║
║  1. Mensagem publicada no RabbitMQ ................ ✅  ║
║  2. Mensagem consumida pela aplicação ............. ✅  ║
║  3. Aggregate criado/atualizado ................... ✅  ║
║  4. Domain Events persistidos (Event Store) ....... ✅  ║
║  5. Alertas projetados (Read Model) ............... ✅  ║
║  6. Integration Events publicados (Outbox) ........ ✅  ║
║  7. API retorna dados corretos .................... ✅  ║
║                                                          ║
║  FLUXO COMPLETO VALIDADO! 🎉                            ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝
```

---

## 📝 **PRÓXIMOS PASSOS:**

1. ✅ Teste diferentes cenários de alertas
2. ✅ Teste duplicatas
3. ✅ Teste concorrência (múltiplas mensagens)
4. ✅ Teste failover (derrubar PostgreSQL e ver retry)
5. ✅ Teste performance (muitas mensagens)

---

**Criado por:** GitHub Copilot AI  
**Data:** 01/02/2025  
**Versão:** 1.0  
**Status:** ✅ PRONTO PARA USO
