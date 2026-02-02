# 🚀 **QUICK START - TESTES E2E EM 5 MINUTOS**

Este guia mostra como executar os testes E2E completos de forma rápida e fácil.

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
