# 🚀 **QUICK START - E2E TESTS IN 5 MINUTES**

This guide shows how to run complete E2E tests quickly and easily.

---

## ⚡ **OPTION 1: AUTOMATIC (RECOMMENDED)**

### **Windows:**
```powershell
# Execute PowerShell script
.\scripts\setup-e2e.ps1
```

### **Linux/Mac:**
```bash
# Make script executable
chmod +x scripts/setup-e2e.sh

# Execute
./scripts/setup-e2e.sh
```

**What the script does:**
- ✅ Verifies prerequisites
- ✅ Starts Docker containers (PostgreSQL + RabbitMQ)
- ✅ Applies migrations
- ✅ Configures RabbitMQ (exchanges, queues, bindings)
- ✅ Compiles application
- ✅ Runs unit tests

**Time:** ~2-3 minutes

---

## 🎯 **OPTION 2: MANUAL (STEP BY STEP)**

### **Step 1: Start Docker**
```bash
docker-compose up -d
```

### **Step 2: Wait (10 seconds)**
```bash
# Wait for containers to be ready
sleep 10

# Check status
docker-compose ps

# Expected output:
# NAME                  STATUS         PORTS
# tc-agro-postgres      Up (healthy)   0.0.0.0:5432->5432/tcp
# tc-agro-rabbitmq      Up (healthy)   0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
```

### **Step 3: Apply Migrations**
```bash
# Navigate to project root
cd C:\FIAP\Hackathon\tc-agro-solutions\services\analytics-worker

# Apply EF Core migrations
dotnet ef database update \
  --project src/Adapters/Outbound/TC.Agro.Analytics.Infrastructure \
  --startup-project src/Adapters/Inbound/TC.Agro.Analytics.Service

# Expected output:
# Build succeeded.
# Applying migration '20260201_InitialCreate'...
# Applying migration '20260201_AddSensorSnapshots'...
# Applying migration '20260201_AddOwnerSnapshots'...
# Done.
```

### **Step 4: Configure RabbitMQ**

**Option A: Via Management UI (http://localhost:15672):**
1. Login: guest/guest
2. **Exchanges** → Add new exchanges:
   - `farm-events` (topic, durable)
   - `sensor-readings` (topic, durable)
   - `analytics-events` (topic, durable)
3. **Queues** → Add new queues:
   - `analytics.sensor.reading.queue` (durable)
   - `analytics.sensor.snapshot.queue` (durable)
   - `analytics.owner.snapshot.queue` (durable)
4. **Bindings** (from exchange to queue with routing key `#`):
   - `sensor-readings` → `analytics.sensor.reading.queue`
   - `farm-events` → `analytics.sensor.snapshot.queue`
   - `farm-events` → `analytics.owner.snapshot.queue`

**Option B: Via CLI (automatic):**
```bash
# Run RabbitMQ setup script
python scripts/setup-rabbitmq.py
```

---

## 🧪 **RUN COMPLETE E2E TEST**

### **Terminal 1: Start Application**
```bash
dotnet run --project src/Adapters/Inbound/TC.Agro.Analytics.Service
```

**Wait to see:**
```
info: Wolverine.Runtime.WolverineRuntime[0]
      Wolverine messaging service is starting
info: Wolverine.RabbitMQ.RabbitMqTransport[0]
      Connected to RabbitMQ at localhost:5672
info: Wolverine.Runtime.WolverineRuntime[0]
      Listening to queues:
        - analytics.sensor.reading.queue
        - analytics.sensor.snapshot.queue
        - analytics.owner.snapshot.queue
info: Microsoft.Hosting.Lifetime[0]
      Application started
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5174
```

### **Terminal 2: Publish Test Message**

**Scenario 1: High Temperature (should trigger alert)**
```bash
python scripts/publish_test_message.py --scenario high-temp
```

**Other scenarios:**
```bash
python scripts/publish_test_message.py --scenario low-soil      # Low soil moisture
python scripts/publish_test_message.py --scenario low-battery   # Low battery
python scripts/publish_test_message.py --scenario multiple      # 3 simultaneous alerts
python scripts/publish_test_message.py --scenario normal        # No alerts (normal values)
```

### **Terminal 1: Check Logs**

You should see:
```
info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedInHandler[0]
      Processing SensorReadingIntegrationEvent for Sensor 550e8400-e29b-41d4-a716-446655440001

info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedInHandler[0]
      Alert created: Type=HighTemperature, Severity=Critical, 
      Value=42.5°C, Threshold=35.0°C

info: TC.Agro.Analytics.Service.Services.AlertHubNotifier[0]
      Real-time notification sent via SignalR to subscribed clients

info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedInHandler[0]
      Sensor reading processed successfully
```

### **Terminal 2: Check API**

```bash
# View pending alerts
curl http://localhost:5174/api/alerts/pending | jq

# View alert history for sensor
curl "http://localhost:5174/api/alerts/history/550e8400-e29b-41d4-a716-446655440001?days=30" | jq

# View sensor status
curl "http://localhost:5174/api/alerts/status/550e8400-e29b-41d4-a716-446655440001" | jq

# Expected response example:
{
  "sensorId": "550e8400-e29b-41d4-a716-446655440001",
  "sensorLabel": "Sensor Test 001",
  "plotName": "Plot A",
  "propertyName": "Farm XYZ",
  "ownerName": "John Doe",
  "status": "Active",
  "pendingAlertCount": 1,
  "criticalAlertCount": 1,
  "lastAlertDate": "2025-02-01T15:30:00Z"
}
```

### **Check Database**

```bash
# Connect to PostgreSQL
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db

# View alerts
SELECT id, sensor_id, type, severity, status, message, value, threshold, created_at 
FROM analytics.alerts 
ORDER BY created_at DESC LIMIT 5;

# View sensor snapshots
SELECT id, label, plot_name, property_name, status, is_active 
FROM analytics.sensor_snapshots 
ORDER BY created_at DESC LIMIT 5;

# Exit
\q
```

### **Check SignalR (Optional)**

```bash
# Open in browser: http://localhost:5174/signalr-test.html
# Click "Connect" button
# You should see real-time alerts appearing when messages are published
```

---

## ✅ **VALIDATION CHECKLIST**

After running tests, verify:

- [ ] **Docker:** Containers `tc-agro-postgres` and `tc-agro-rabbitmq` running (healthy)
- [ ] **PostgreSQL:** Tables `analytics.alerts`, `analytics.sensor_snapshots`, `analytics.owner_snapshots` created
- [ ] **RabbitMQ:** Exchanges and Queues configured
  - [ ] Exchange `sensor-readings` exists
  - [ ] Exchange `farm-events` exists  
  - [ ] Exchange `analytics-events` exists
  - [ ] Queue `analytics.sensor.reading.queue` exists
  - [ ] Queue `analytics.sensor.snapshot.queue` exists
  - [ ] Queue `analytics.owner.snapshot.queue` exists
- [ ] **Application:** Started without errors and connected to PostgreSQL + RabbitMQ
- [ ] **Message:** Consumed from queue (message count = 0 in RabbitMQ UI)
- [ ] **Alerts:** Created in `analytics.alerts` table
- [ ] **Snapshots:** Created/updated in snapshot tables
- [ ] **SignalR:** Real-time notifications working (if tested)
- [ ] **API:** Endpoints return correct data
- [ ] **Logs:** No critical errors

---

## 🐛 **COMMON PROBLEMS**

### **Error: "Cannot connect to PostgreSQL"**
```bash
# Restart PostgreSQL
docker-compose restart postgres

# Wait 10 seconds
sleep 10

# Check health
docker-compose ps
```

### **Error: "Cannot connect to RabbitMQ"**
```bash
# Restart RabbitMQ
docker-compose restart rabbitmq

# Wait 10 seconds
sleep 10

# Check connections
docker exec tc-agro-rabbitmq rabbitmqctl list_connections
```

### **Error: "Queue not found"**
```bash
# Check if queue exists
docker exec tc-agro-rabbitmq rabbitmqctl list_queues

# If not exists, create manually via Management UI
# Or run setup script:
python scripts/setup-rabbitmq.py
```

### **Error: "Migration already applied"**
```bash
# Clean and recreate database
docker-compose down -v
docker-compose up -d
sleep 10

# Apply migrations again
dotnet ef database update \
  --project src/Adapters/Outbound/TC.Agro.Analytics.Infrastructure \
  --startup-project src/Adapters/Inbound/TC.Agro.Analytics.Service
```

### **Application not consuming messages:**
```bash
# 1. Check Wolverine connection in logs
#    Should show: "Listening to queue 'analytics.sensor.reading.queue'"

# 2. Verify appsettings.Development.json
#    Section: Messaging:RabbitMQ

# 3. Check RabbitMQ connections
docker exec tc-agro-rabbitmq rabbitmqctl list_connections

# 4. Check queue bindings
# Go to http://localhost:15672 → Queues → analytics.sensor.reading.queue → Bindings
```

---

## 🎯 **EXPECTED RESULT**

```
╔══════════════════════════════════════════════════════════╗
║            E2E TEST SUCCESSFUL! ✅                       ║
╠══════════════════════════════════════════════════════════╣
║                                                          ║
║  1. Message published to RabbitMQ ................. ✅  ║
║  2. Message consumed by application ............... ✅  ║
║  3. Alert aggregate created ....................... ✅  ║
║  4. Alerts persisted in database .................. ✅  ║
║  5. SignalR notification sent ..................... ✅  ║
║  6. Integration events published .................. ✅  ║
║  7. API returns correct data ...................... ✅  ║
║                                                          ║
║  COMPLETE FLOW VALIDATED! 🎉                            ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝
```

---

## 📝 **NEXT STEPS**

1. ✅ Test all 5 scenarios (high-temp, low-soil, low-battery, multiple, normal)
2. ✅ Test alert lifecycle (Pending → Acknowledged → Resolved)
3. ✅ Test duplicates (publish same message twice)
4. ✅ Test concurrency (publish multiple messages rapidly)
5. ✅ Test SignalR real-time notifications
6. ✅ See full documentation: [E2E_TESTING_GUIDE.md](E2E_TESTING_GUIDE.md)
7. ✅ See run guide: [RUN_PROJECT.md](RUN_PROJECT.md)

---

## 📊 **PERFORMANCE TESTING**

### **Load Test: Multiple Sensors**
```bash
# Publish 100 messages rapidly
for i in {1..100}; do
  python scripts/publish_test_message.py --scenario high-temp &
done
wait

# Check processing time in logs
# Check alert count
curl http://localhost:5174/api/alerts/pending | jq length
```

### **Concurrency Test: Same Sensor**
```bash
# Publish 10 messages for same sensor simultaneously
for i in {1..10}; do
  python scripts/publish_test_message.py --scenario high-temp --sensor-id "TEST-001" &
done
wait

# Should handle concurrency correctly (no duplicates)
```

---

**Total Time:** ~5 minutes  
**Difficulty:** Easy  
**Status:** ✅ Ready to use  
**Last Updated:** February 2025  
**Version:** 2.0


---

## ⚡ **OPÇÃO 1: AUTOMÁTICO (RECOMENDADO)**

### **Windows:**
```powershell
# Execute o script PowerShell
.\setup-e2e.ps1
```

### **Linux/Mac:**
```bash
# Torne o script executável
chmod +x setup-e2e.sh

# Execute
./setup-e2e.sh
```

**O que o script faz:**
- ✅ Verifica pré-requisitos
- ✅ Inicia containers Docker (PostgreSQL + RabbitMQ)
- ✅ Aplica migrations
- ✅ Configura RabbitMQ (exchange, queue, binding)
- ✅ Compila aplicação
- ✅ Executa testes unitários

**Tempo:** ~2-3 minutos

---

## 🎯 **OPÇÃO 2: MANUAL (PASSO A PASSO)**

### **Passo 1: Iniciar Docker**
```bash
docker-compose up -d
```

### **Passo 2: Aguardar (10 segundos)**
```bash
# Aguardar containers ficarem prontos
sleep 10

# Verificar status
docker-compose ps
```

### **Passo 3: Aplicar Migrations**
```bash
dotnet ef database update \
  --project src/Adapters/Outbound/TC.Agro.Analytics.Infrastructure \
  --startup-project src/Adapters/Inbound/TC.Agro.Analytics.Service
```

### **Passo 4: Configurar RabbitMQ**

**Via Management UI (http://localhost:15672):**
1. Login: guest/guest
2. **Exchanges** → Add new exchange:
   - Name: `analytics.sensor.ingested`
   - Type: `topic`
   - Durability: `Durable`
3. **Queues** → Add new queue:
   - Name: `analytics.sensor.ingested.queue`
   - Durability: `Durable`
4. **Queues** → `analytics.sensor.ingested.queue` → **Bindings**:
   - From exchange: `analytics.sensor.ingested`
   - Routing key: `#`

---

## 🧪 **EXECUTAR TESTE E2E COMPLETO**

### **Terminal 1: Iniciar Aplicação**
```bash
dotnet run --project src/Adapters/Inbound/TC.Agro.Analytics.Service
```

**Aguarde ver:**
```
info: Wolverine messaging service is starting
info: Connected to RabbitMQ at localhost:5672
info: Listening to queue 'analytics.sensor.ingested.queue'
info: Application started
```

### **Terminal 2: Publicar Mensagem de Teste**

**Cenário 1: Alta Temperatura (deve gerar alerta)**
```bash
python publish_message.py --scenario high-temp
```

**Outros cenários:**
```bash
python publish_message.py --scenario low-soil      # Baixa umidade
python publish_message.py --scenario low-battery   # Bateria baixa
python publish_message.py --scenario multiple      # 3 alertas simultâneos
python publish_message.py --scenario ok            # Sem alertas
```

### **Terminal 1: Verificar Logs**

Você deve ver:
```
info: Processing SensorIngestedIntegrationEvent for Sensor SENSOR-TEST-001
warn: High temperature alert triggered. Temperature: 42.5°C
info: Sensor reading processed successfully
```

### **Terminal 2: Verificar API**

```bash
# Ver alertas pendentes
curl http://localhost:5174/alerts/pending | jq

# Ver histórico do plot
curl "http://localhost:5174/alerts/history/ae57f8d7-d491-4899-bb39-30124093e683?days=30" | jq

# Ver status do plot
curl "http://localhost:5174/alerts/status/ae57f8d7-d491-4899-bb39-30124093e683" | jq
```

### **Verificar Banco de Dados**

```bash
# Conectar no PostgreSQL
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db

# Ver eventos (Event Store)
SELECT id, type, data->>'SensorId' as sensor, data->>'Temperature' as temp 
FROM analytics.mt_events 
ORDER BY timestamp DESC LIMIT 5;

# Ver alertas (Read Model)
SELECT sensor_id, alert_type, message, severity, value, threshold, created_at 
FROM analytics.alerts 
ORDER BY created_at DESC LIMIT 5;

# Sair
\q
```

---

## ✅ **CHECKLIST DE VALIDAÇÃO**

Após executar os testes, verifique:

- [ ] **Docker:** Containers `tc-agro-postgres` e `tc-agro-rabbitmq` rodando
- [ ] **PostgreSQL:** Tabelas `analytics.alerts` e `analytics.mt_events` criadas
- [ ] **RabbitMQ:** Exchange e Queue configurados
- [ ] **Aplicação:** Iniciou sem erros e conectou em PostgreSQL + RabbitMQ
- [ ] **Mensagem:** Foi consumida da fila (count = 0 no RabbitMQ UI)
- [ ] **Event Store:** Eventos persistidos em `mt_events`
- [ ] **Read Model:** Alertas criados em `analytics.alerts`
- [ ] **API:** Endpoints retornam dados corretos
- [ ] **Logs:** Sem erros críticos

---

## 🐛 **PROBLEMAS COMUNS**

### **Erro: "Cannot connect to PostgreSQL"**
```bash
# Restartar PostgreSQL
docker-compose restart postgres

# Aguardar 10 segundos
sleep 10
```

### **Erro: "Cannot connect to RabbitMQ"**
```bash
# Restartar RabbitMQ
docker-compose restart rabbitmq

# Aguardar 10 segundos
sleep 10
```

### **Erro: "Queue not found"**
```bash
# Verificar se queue existe
docker exec tc-agro-rabbitmq rabbitmqadmin list queues

# Se não existir, criar manualmente via Management UI
# http://localhost:15672
```

### **Erro: "Migration already applied"**
```bash
# Limpar e recriar banco
docker-compose down -v
docker-compose up -d
sleep 10
dotnet ef database update
```

---

## 🎯 **RESULTADO ESPERADO**

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

## 📝 **PRÓXIMOS PASSOS**

1. ✅ Testar todos os 5 cenários
2. ✅ Verificar duplicatas (publicar mesma mensagem 2x)
3. ✅ Testar concorrência (publicar várias mensagens rapidamente)
4. ✅ Ver documentação completa: [E2E_TESTING_GUIDE.md](E2E_TESTING_GUIDE.md)

---

**Tempo total:** ~5 minutos  
**Dificuldade:** Fácil  
**Status:** ✅ Pronto para usar
