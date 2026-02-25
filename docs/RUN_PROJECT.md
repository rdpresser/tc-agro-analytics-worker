# 🚀 **RUN PROJECT - QUICK GUIDE**

## ✅ **PREREQUISITES COMPLETED:**

- ✅ Docker Compose running (PostgreSQL + RabbitMQ)
- ✅ Migrations applied
- ✅ RabbitMQ configured (exchange, queue, binding)
- ✅ Configuration updated for Docker

---

## 🎯 **RUN APPLICATION**

### **Terminal 1: Application**

```powershell
# From project root
dotnet run --project src/Adapters/Inbound/TC.Agro.Analytics.Service
```

**✅ Expected output:**
```
info: Wolverine.Runtime.WolverineRuntime[0]
      Wolverine messaging service is starting
info: Wolverine.RabbitMQ.RabbitMqTransport[0]
      Connected to RabbitMQ at localhost:5672
info: Wolverine.Runtime.WolverineRuntime[0]
      Listening to queue 'analytics.sensor.reading.queue'
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5174
```

**❌ If RabbitMQ connection error occurs:**
```powershell
# Check if RabbitMQ is running
docker-compose ps

# Restart if necessary
docker-compose restart rabbitmq
```

---

## 📨 **PUBLISH TEST MESSAGE**

### **Option 1: Using curl (Simple test)**

```powershell
# Publish a high temperature sensor reading (triggers alert)
curl -X POST http://localhost:5672/api/publish `
  -H "Content-Type: application/json" `
  -d '{
    "sensorId": "550e8400-e29b-41d4-a716-446655440001",
    "ownerId": "650e8400-e29b-41d4-a716-446655440001",
    "plotId": "750e8400-e29b-41d4-a716-446655440001",
    "timestamp": "2025-02-01T15:30:00Z",
    "temperature": 42.5,
    "humidity": 65.0,
    "soilMoisture": 45.0,
    "rainfall": 0.0,
    "batteryLevel": 85.0
  }'
```

### **Option 2: Using Python script (Multiple scenarios)**

```powershell
# Install Python dependencies (first time only)
pip install pika

# Publish test message - High Temperature
python scripts/publish_test_message.py --scenario high-temp
```

**Available scenarios:**
```powershell
--scenario high-temp      # 🌡️  Temperature 42.5°C (triggers alert)
--scenario low-soil       # 💧 Soil Moisture 15% (triggers alert)
--scenario low-battery    # 🔋 Battery 10% (triggers alert)
--scenario multiple       # ⚠️  3 simultaneous alerts
--scenario normal         # ✅ No alerts (normal values)
```

---

## 📊 **VERIFY RESULTS**

### **1. Application Logs (Terminal 1):**

You should see:
```
info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedInHandler[0]
      Processing SensorReadingIntegrationEvent for Sensor 550e8400-e29b-41d4-a716-446655440001

warn: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedInHandler[0]
      Alert created: HighTemperature for Sensor 550e8400-e29b-41d4-a716-446655440001. 
      Temperature: 42.5°C (Threshold: 35.0°C)

info: TC.Agro.Analytics.Service.Services.AlertHubNotifier[0]
      Real-time notification sent via SignalR for alert type: HighTemperature

info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedInHandler[0]
      Sensor reading processed successfully for Sensor 550e8400-e29b-41d4-a716-446655440001
```

### **2. REST API (Terminal 2):**

```powershell
# Health check
curl http://localhost:5174/health

# View pending alerts
curl http://localhost:5174/api/alerts/pending | jq

# View alert history for a sensor
curl "http://localhost:5174/api/alerts/history/550e8400-e29b-41d4-a716-446655440001?days=30" | jq

# View sensor status
curl "http://localhost:5174/api/alerts/status/550e8400-e29b-41d4-a716-446655440001" | jq

# Acknowledge an alert (replace {id} with actual alert ID)
curl -X POST "http://localhost:5174/api/alerts/{id}/acknowledge" `
  -H "Content-Type: application/json" `
  -d '{"userId": "650e8400-e29b-41d4-a716-446655440001"}' | jq

# Resolve an alert (replace {id} with actual alert ID)
curl -X POST "http://localhost:5174/api/alerts/{id}/resolve" `
  -H "Content-Type: application/json" `
  -d '{
    "userId": "650e8400-e29b-41d4-a716-446655440001",
    "resolutionNotes": "Temperature normalized after irrigation"
  }' | jq
```

### **3. SignalR Hub (Terminal 3 - Optional):**

```powershell
# Test SignalR connection (requires signalr-test.html)
# Open in browser: http://localhost:5174/signalr-test.html

# Or use SignalR client library to connect to:
# ws://localhost:5174/dashboard/alertshub
```

### **4. Database:**

```powershell
# Connect to PostgreSQL
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db

# View alerts (Read Model)
SELECT id, sensor_id, type, severity, status, message, value, threshold, created_at 
FROM analytics.alerts 
ORDER BY created_at DESC LIMIT 10;

# View sensor snapshots
SELECT id, label, plot_name, property_name, status, is_active, created_at
FROM analytics.sensor_snapshots
ORDER BY created_at DESC LIMIT 10;

# View owner snapshots
SELECT id, first_name, last_name, email, is_active, created_at
FROM analytics.owner_snapshots
ORDER BY created_at DESC LIMIT 10;

# Exit
\q
```

### **5. RabbitMQ Management UI:**

```
URL: http://localhost:15672
User: guest
Password: guest
```

**Verify:**
- **Queues** → `analytics.sensor.reading.queue` → Messages = 0 (consumed)
- **Connections** → Should have active connection from application
- **Exchanges** → `farm-events`, `sensor-readings`, `analytics-events`

---

## 🎯 **EXPECTED RESULT:**

```
╔══════════════════════════════════════════════════════════╗
║            E2E TEST SUCCESSFUL! ✅                       ║
╠══════════════════════════════════════════════════════════╣
║                                                          ║
║  1. Application started .......................... ✅  ║
║  2. Connected to PostgreSQL ...................... ✅  ║
║  3. Connected to RabbitMQ ........................ ✅  ║
║  4. Message published ............................ ✅  ║
║  5. Message consumed ............................. ✅  ║
║  6. Alerts created ............................... ✅  ║
║  7. SignalR notification sent .................... ✅  ║
║  8. API returns data ............................. ✅  ║
║                                                          ║
║  COMPLETE FLOW VALIDATED! 🎉                            ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝
```

---

## 🧪 **ADDITIONAL TESTS**

### **Test Alert Lifecycle:**

```powershell
# 1. Create alert (high temperature reading)
python scripts/publish_test_message.py --scenario high-temp

# 2. Get alert ID from API
$alerts = curl http://localhost:5174/api/alerts/pending | ConvertFrom-Json
$alertId = $alerts[0].id

# 3. Acknowledge alert
curl -X POST "http://localhost:5174/api/alerts/$alertId/acknowledge" `
  -H "Content-Type: application/json" `
  -d '{"userId": "650e8400-e29b-41d4-a716-446655440001"}'

# 4. Resolve alert
curl -X POST "http://localhost:5174/api/alerts/$alertId/resolve" `
  -H "Content-Type: application/json" `
  -d '{
    "userId": "650e8400-e29b-41d4-a716-446655440001",
    "resolutionNotes": "Issue resolved"
  }'

# 5. Verify status changed
curl "http://localhost:5174/api/alerts/history/550e8400-e29b-41d4-a716-446655440001" | jq
```

### **Test Multiple Sensors:**

```powershell
# Publish readings from multiple sensors
for ($i=1; $i -le 5; $i++) {
    python scripts/publish_test_message.py --scenario high-temp --sensor-id "SENSOR-$i"
    Start-Sleep -Seconds 2
}

# View all pending alerts
curl http://localhost:5174/api/alerts/pending | jq
```

---

## 🐛 **TROUBLESHOOTING**

### **Error: "Cannot connect to PostgreSQL"**
```powershell
# Check container status
docker-compose ps

# Restart PostgreSQL
docker-compose restart postgres

# Wait for health check
Start-Sleep -Seconds 10

# Check logs
docker-compose logs postgres
```

### **Error: "Cannot connect to RabbitMQ"**
```powershell
# Restart RabbitMQ
docker-compose restart rabbitmq

# Wait for health check
Start-Sleep -Seconds 10

# Verify queues exist
docker exec tc-agro-rabbitmq rabbitmqctl list_queues
```

### **Error: "Queue not found"**
```powershell
# Check via Management UI
# http://localhost:15672 → Queues

# Or list via CLI
docker exec tc-agro-rabbitmq rabbitmqctl list_queues

# Expected queues:
# - analytics.sensor.reading.queue
# - analytics.sensor.snapshot.queue
# - analytics.owner.snapshot.queue
```

### **Application not consuming messages:**
```powershell
# Check Wolverine connection in logs
# Should show: "Listening to queue 'analytics.sensor.reading.queue'"

# Verify appsettings.Development.json
# Section: Messaging:RabbitMQ

# Check RabbitMQ connections
docker exec tc-agro-rabbitmq rabbitmqctl list_connections
```

### **SignalR not working:**
```powershell
# Check if hub is registered
# Logs should show: "SignalR hub mapped: /dashboard/alertshub"

# Test WebSocket connection
# Use browser console:
# const connection = new signalR.HubConnectionBuilder()
#   .withUrl("http://localhost:5174/dashboard/alertshub")
#   .build();
# await connection.start();
```

---

## 📝 **NEXT TESTS:**

1. ✅ Test all 5 scenarios (high-temp, low-soil, low-battery, multiple, normal)
2. ✅ Test alert lifecycle (Pending → Acknowledged → Resolved)
3. ✅ Test duplicate messages (same sensor reading)
4. ✅ Test multiple messages rapidly (load test)
5. ✅ Test PostgreSQL failure and retry
6. ✅ Test RabbitMQ failure and retry
7. ✅ Test SignalR real-time notifications
8. ✅ Test snapshot updates (sensor status changes)

---

## 📊 **MONITORING**

### **Application Insights (if configured):**
```
# View telemetry in Azure Portal
# Or check local OTLP collector logs
```

### **Health Checks:**
```powershell
# Overall health
curl http://localhost:5174/health

# Detailed health
curl http://localhost:5174/health/ready
```

### **Metrics:**
```powershell
# If Prometheus exporter is enabled
curl http://localhost:5174/metrics
```

---

**Status:** ✅ READY TO RUN  
**Estimated time:** ~5 minutes for first test  
**Last Updated:** February 2025  
**Version:** 2.0
