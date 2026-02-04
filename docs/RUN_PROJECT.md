# 🚀 **EXECUTAR PROJETO - GUIA RÁPIDO**

## ✅ **PRÉ-REQUISITOS CONCLUÍDOS:**

- ✅ Docker Compose rodando (PostgreSQL + RabbitMQ)
- ✅ Migrations aplicadas
- ✅ RabbitMQ configurado (exchange, queue, binding)
- ✅ Configuração atualizada para Docker

---

## 🎯 **EXECUTAR APLICAÇÃO**

### **Terminal 1: Aplicação**

```powershell
# Na raiz do projeto
dotnet run --project src/Adapters/Inbound/TC.Agro.Analytics.Service
```

**✅ Saída esperada:**
```
info: Wolverine.Runtime.WolverineRuntime[0]
      Wolverine messaging service is starting
info: Wolverine.RabbitMQ.RabbitMqTransport[0]
      Connected to RabbitMQ at localhost:5672
info: Wolverine.Runtime.WolverineRuntime[0]
      Listening to queue 'analytics.sensor.ingested.queue'
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5174
```

**❌ Se der erro de conexão RabbitMQ:**
```powershell
# Verificar se RabbitMQ está rodando
docker-compose ps

# Restartar se necessário
docker-compose restart rabbitmq
```

---

## 📨 **PUBLICAR MENSAGEM DE TESTE**

### **Terminal 2: Publicar mensagem**

```powershell
# Instalar dependências Python (primeira vez)
pip install -r requirements.txt

# Publicar mensagem de teste - Alta Temperatura
python publish_message.py --scenario high-temp
```

**Cenários disponíveis:**
```powershell
--scenario high-temp      # 🌡️  Temperatura 42.5°C (gera alerta)
--scenario low-soil       # 💧 Umidade 15% (gera alerta)
--scenario low-battery    # 🔋 Bateria 10% (gera alerta)
--scenario multiple       # ⚠️  3 alertas simultâneos
--scenario ok             # ✅ Sem alertas (valores normais)
```

---

## 📊 **VERIFICAR RESULTADOS**

### **1. Logs da aplicação (Terminal 1):**

Você deve ver:
```
info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedHandler[0]
      Processing SensorIngestedIntegrationEvent for Sensor SENSOR-TEST-001, Plot ae57f8d7-d491-4899-bb39-30124093e683

warn: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedHandler[0]
      High temperature alert triggered for Sensor SENSOR-TEST-001. Temperature: 42.5°C (Threshold: 30.0°C)

info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedHandler[0]
      Sensor reading processed successfully for Sensor SENSOR-TEST-001, Plot ae57f8d7-d491-4899-bb39-30124093e683
```

### **2. API (Terminal 2):**

```powershell
# Health check
curl http://localhost:5174/health

# Ver alertas pendentes
curl http://localhost:5174/alerts/pending

# Ver histórico de um plot
curl "http://localhost:5174/alerts/history/ae57f8d7-d491-4899-bb39-30124093e683?days=30"

# Ver status do plot
curl "http://localhost:5174/alerts/status/ae57f8d7-d491-4899-bb39-30124093e683"
```

### **3. Banco de Dados:**

```powershell
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

### **4. RabbitMQ Management UI:**

```
URL: http://localhost:15672
User: guest
Password: guest
```

**Verificar:**
- **Queues** → `analytics.sensor.ingested.queue` → Messages = 0 (consumidas)
- **Connections** → Deve ter conexão ativa da aplicação

---

## 🎯 **RESULTADO ESPERADO:**

```
╔══════════════════════════════════════════════════════════╗
║               TESTE E2E BEM-SUCEDIDO! ✅                ║
╠══════════════════════════════════════════════════════════╣
║                                                          ║
║  1. Aplicação iniciou ............................ ✅  ║
║  2. Conectou no PostgreSQL ....................... ✅  ║
║  3. Conectou no RabbitMQ ......................... ✅  ║
║  4. Mensagem publicada ........................... ✅  ║
║  5. Mensagem consumida ........................... ✅  ║
║  6. Domain Events persistidos .................... ✅  ║
║  7. Alertas criados .............................. ✅  ║
║  8. API retorna dados ............................ ✅  ║
║                                                          ║
║  FLUXO COMPLETO VALIDADO! 🎉                            ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝
```

---

## 🐛 **TROUBLESHOOTING**

### **Erro: "Cannot connect to PostgreSQL"**
```powershell
docker-compose restart postgres
```

### **Erro: "Cannot connect to RabbitMQ"**
```powershell
docker-compose restart rabbitmq
```

### **Erro: "Queue not found"**
```powershell
# Verificar via Management UI
# http://localhost:15672 → Queues

# Criar manualmente se necessário (já está no script)
```

### **Aplicação não consome mensagens:**
```powershell
# Verificar se Wolverine está conectado
# Logs devem mostrar: "Listening to queue 'analytics.sensor.ingested.queue'"

# Se não mostrar, verificar appsettings.Development.json
# Seção Messaging:RabbitMQ
```

---

## 📝 **PRÓXIMOS TESTES:**

1. ✅ Testar todos os 5 cenários
2. ✅ Publicar mensagem duplicada (mesmo AggregateId)
3. ✅ Publicar múltiplas mensagens rapidamente
4. ✅ Derrubar PostgreSQL e ver retry
5. ✅ Derrubar RabbitMQ e ver retry

---

**Status:** ✅ PRONTO PARA EXECUTAR  
**Tempo estimado:** ~5 minutos para primeiro teste
