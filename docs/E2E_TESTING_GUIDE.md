# 🧪 **COMPREHENSIVE GUIDE - E2E TESTING WITH RABBITMQ + POSTGRESQL**

**Objective:** Test complete flow from RabbitMQ message consumption to PostgreSQL persistence, including real-time SignalR notifications.

---

## 📋 **PREREQUISITES**

- ✅ Docker Desktop installed and running
- ✅ .NET 10 SDK installed
- ✅ Visual Studio 2022/2026, VS Code, or JetBrains Rider
- ✅ PowerShell 7+ or Git Bash
- ✅ Python 3.8+ (for test scripts)
- ✅ `jq` command-line JSON processor (optional, for pretty output)

---

## 🐳 **STEP 1: CONFIGURE DOCKER COMPOSE**

### **1.1 Create docker-compose.yml in project root:**

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

### **1.2 Create init-db.sql in project root:**

```sql
-- Database initialization script
-- Creates analytics schema if it doesn't exist

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

-- Log for debugging
\echo 'Database tc-agro-analytics-db initialized successfully!'
```

### **1.3 Start containers:**

```bash
# From project root (where docker-compose.yml is located)
docker-compose up -d

# Check if containers are running
docker-compose ps

# Expected output:
# NAME                  STATUS         PORTS
# tc-agro-postgres      Up (healthy)   0.0.0.0:5432->5432/tcp
# tc-agro-rabbitmq      Up (healthy)   0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
```

### **1.4 Check logs:**

```bash
# PostgreSQL
docker-compose logs postgres

# RabbitMQ
docker-compose logs rabbitmq

# Both in real-time
docker-compose logs -f
```

---

## 🗄️ **STEP 2: CONFIGURE DATABASE**

### **2.1 Apply Migrations:**

```bash
# Restore dependencies
dotnet restore

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

### **2.2 Verify created tables:**

```bash
# Connect to PostgreSQL via docker
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db

# In psql, execute:
\dn  # List schemas
\dt analytics.*  # List tables in analytics schema

# Expected output:
# Schema   | Name               | Type  | Owner
# ---------+--------------------+-------+--------
# analytics| alerts             | table | postgres
# analytics| sensor_snapshots   | table | postgres
# analytics| owner_snapshots    | table | postgres
# analytics| __EFMigrationsHistory | table | postgres

# Check indexes
\di analytics.*

# Expected output: Multiple indexes on alerts and snapshot tables

# Exit psql
\q
```

### **2.3 Insert test data (optional):**

```bash
# Create test data SQL file
cat > test-data-e2e.sql << 'EOF'
-- Insert test sensor snapshot
INSERT INTO analytics.sensor_snapshots 
  (id, owner_id, property_id, plot_id, label, plot_name, property_name, is_active, created_at)
VALUES 
  ('550e8400-e29b-41d4-a716-446655440001', 
   '650e8400-e29b-41d4-a716-446655440001',
   '750e8400-e29b-41d4-a716-446655440001',
   '850e8400-e29b-41d4-a716-446655440001',
   'Sensor Test 001',
   'Plot A',
   'Farm XYZ',
   true,
   NOW());

-- Insert test owner snapshot
INSERT INTO analytics.owner_snapshots
  (id, first_name, last_name, email, is_active, created_at)
VALUES
  ('650e8400-e29b-41d4-a716-446655440001',
   'John',
   'Doe',
   'john.doe@example.com',
   true,
   NOW());
EOF

# Copy SQL file into container
docker cp test-data-e2e.sql tc-agro-postgres:/tmp/

# Execute script
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db -f /tmp/test-data-e2e.sql

# Verify data
docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db -c "SELECT COUNT(*) FROM analytics.sensor_snapshots;"

# Expected output: 1
```

---

## 🐰 **STEP 3: CONFIGURE RABBITMQ**

### **3.1 Access Management UI:**

```
URL: http://localhost:15672
User: guest
Password: guest
```

### **3.2 Create Exchanges and Queues manually:**

**Option A: Via Management UI:**

1. **Exchanges Tab** → **Add a new exchange**

   Create these exchanges:
   - Name: `farm-events`, Type: `topic`, Durability: `Durable`
   - Name: `sensor-readings`, Type: `topic`, Durability: `Durable`
   - Name: `analytics-events`, Type: `topic`, Durability: `Durable`

2. **Queues Tab** → **Add a new queue**

   Create these queues:
   - Name: `analytics.sensor.reading.queue`, Durability: `Durable`
   - Name: `analytics.sensor.snapshot.queue`, Durability: `Durable`
   - Name: `analytics.owner.snapshot.queue`, Durability: `Durable`

3. **Bindings** (for each queue, click on queue name → Bindings section)

   Add bindings:
   - Queue: `analytics.sensor.reading.queue`
     - From exchange: `sensor-readings`
     - Routing key: `#`
   - Queue: `analytics.sensor.snapshot.queue`
     - From exchange: `farm-events`
     - Routing key: `sensor.#`
   - Queue: `analytics.owner.snapshot.queue`
     - From exchange: `farm-events`
     - Routing key: `owner.#`

**Option B: Via Python Script (Automated):**

```bash
# Install pika library
pip install pika

# Create setup script
cat > scripts/setup-rabbitmq.py << 'EOF'
import pika

# Connect to RabbitMQ
connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
channel = connection.channel()

# Declare exchanges
exchanges = [
    'farm-events',
    'sensor-readings',
    'analytics-events'
]

for exchange in exchanges:
    channel.exchange_declare(
        exchange=exchange,
        exchange_type='topic',
        durable=True
    )
    print(f"✅ Exchange created: {exchange}")

# Declare queues
queues = [
    'analytics.sensor.reading.queue',
    'analytics.sensor.snapshot.queue',
    'analytics.owner.snapshot.queue'
]

for queue in queues:
    channel.queue_declare(queue=queue, durable=True)
    print(f"✅ Queue created: {queue}")

# Create bindings
bindings = [
    ('sensor-readings', 'analytics.sensor.reading.queue', '#'),
    ('farm-events', 'analytics.sensor.snapshot.queue', 'sensor.#'),
    ('farm-events', 'analytics.owner.snapshot.queue', 'owner.#')
]

for exchange, queue, routing_key in bindings:
    channel.queue_bind(
        exchange=exchange,
        queue=queue,
        routing_key=routing_key
    )
    print(f"✅ Binding created: {exchange} → {queue} ({routing_key})")

connection.close()
print("\n✅ RabbitMQ configuration completed!")
EOF

# Run setup script
python scripts/setup-rabbitmq.py
```

### **3.3 Verify Configuration:**

```bash
# List exchanges
docker exec tc-agro-rabbitmq rabbitmqctl list_exchanges

# Expected output:
# farm-events       topic
# sensor-readings   topic
# analytics-events  topic

# List queues
docker exec tc-agro-rabbitmq rabbitmqctl list_queues

# Expected output:
# analytics.sensor.reading.queue    0
# analytics.sensor.snapshot.queue   0
# analytics.owner.snapshot.queue    0

# List bindings
docker exec tc-agro-rabbitmq rabbitmqctl list_bindings

# Should show bindings between exchanges and queues
```
   ---

   ## ⚙️ **STEP 4: CONFIGURE APPLICATION**

   ### **4.1 Update appsettings.Development.json:**

   ```json
   {
     "Serilog": {
       "MinimumLevel": {
         "Default": "Information",
         "Override": {
           "Microsoft": "Warning",
           "System": "Warning",
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
         "Password": "postgres",
         "Schema": "analytics",
         "SslMode": "Prefer"
       }
     },
     "Messaging": {
       "RabbitMQ": {
         "Host": "localhost",
         "Port": 5672,
         "UserName": "guest",
         "Password": "guest",
         "VirtualHost": "/"
       }
     },
     "AlertThresholds": {
       "MaxTemperature": 35.0,
       "MinSoilMoisture": 20.0,
       "MinBatteryLevel": 15.0
     },
     "SignalR": {
       "Enabled": true,
       "HubPath": "/dashboard/alertshub"
     }
   }
   ```

   ### **4.2 Verify Program.cs Configuration:**

   Ensure the following components are configured:

   ```csharp
   // Wolverine with RabbitMQ
   builder.Host.UseWolverine(opts =>
   {
       var rabbitConfig = builder.Configuration.GetSection("Messaging:RabbitMQ");

       opts.UseRabbitMq(rabbit =>
       {
           rabbit.HostName = rabbitConfig["Host"];
           rabbit.Port = int.Parse(rabbitConfig["Port"]);
           rabbit.UserName = rabbitConfig["UserName"];
           rabbit.Password = rabbitConfig["Password"];
           rabbit.VirtualHost = rabbitConfig["VirtualHost"];
       })
       .AutoProvision()
       .UseConventionalRouting();

       // Listen to queues
       opts.ListenToRabbitQueue("analytics.sensor.reading.queue");
       opts.ListenToRabbitQueue("analytics.sensor.snapshot.queue");
       opts.ListenToRabbitQueue("analytics.owner.snapshot.queue");
   });

   // SignalR
   builder.Services.AddSignalR();
   app.MapHub<AlertHub>("/dashboard/alertshub");

   // FastEndpoints
   builder.Services.AddFastEndpoints();
   app.UseFastEndpoints();

   // EF Core
   builder.Services.AddDbContext<AnalyticsDbContext>();
   ```

   **⚠️ IMPORTANT:** All these configurations should already be in place. This is just for verification.

   ---

   ## 🚀 **STEP 5: RUN APPLICATION**

   ### **5.1 Build Project:**

   ```bash
   # Clean and build
   dotnet clean
   dotnet build

   # Expected output:
   # Build succeeded. 0 Warning(s). 0 Error(s).
   ```

   ### **5.2 Run Application:**

   ```bash
   # From project root
   dotnet run --project src/Adapters/Inbound/TC.Agro.Analytics.Service

   # Expected output:
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
         Application started. Press Ctrl+C to shut down.
   info: Microsoft.Hosting.Lifetime[0]
         Hosting environment: Development
   info: Microsoft.Hosting.Lifetime[0]
         Now listening on: http://localhost:5174
   ```

   ### **5.3 Verify Health Check:**

   ```bash
   # In another terminal
   curl http://localhost:5174/health

   # Expected output:
   {
     "status": "Healthy",
     "timestamp": "2025-02-01T10:00:00Z",
     "service": "Analytics Worker Service",
     "checks": {
       "database": "Healthy",
       "rabbitmq": "Healthy"
     }
   }
   ```

   ### **5.4 Verify SignalR Hub:**

   ```bash
   # Open in browser
   http://localhost:5174/signalr-test.html

   # Click "Connect" button
   # Should see: "Connected to SignalR hub"
   ```

   ---

   ## 📨 **STEP 6: PUBLISH TEST MESSAGE**

   ### **6.1 Via Python Script (Recommended):**

   ```bash
   # Install dependencies (first time only)
   pip install pika

   # Publish high temperature message
   python scripts/publish_test_message.py --scenario high-temp

   # Expected output:
   ✅ Connected to RabbitMQ at localhost:5672
   ✅ Message published to queue: analytics.sensor.reading.queue
   📊 Scenario: high-temp
      Sensor ID: 550e8400-e29b-41d4-a716-446655440001
      Temperature: 42.5°C (threshold: 35.0°C)
      Expected: HighTemperature alert (Critical severity)
   ```

   **Test Payload (high-temp scenario):**

   ```json
   {
     "eventId": "12345678-1234-1234-1234-123456789012",
     "aggregateId": "87654321-4321-4321-4321-210987654321",
     "occurredOn": "2025-02-01T10:00:00Z",
     "eventName": "SensorReadingIntegrationEvent",
     "sensorId": "550e8400-e29b-41d4-a716-446655440001",
     "ownerId": "650e8400-e29b-41d4-a716-446655440001",
     "plotId": "750e8400-e29b-41d4-a716-446655440001",
     "timestamp": "2025-02-01T09:55:00Z",
     "temperature": 42.5,
     "humidity": 65.0,
     "soilMoisture": 45.0,
     "rainfall": 2.5,
     "batteryLevel": 85.0
   }
   ```

   **📌 IMPORTANT:** 
   - `Temperature: 42.5°C` is **ABOVE** threshold (35°C) → Will generate alert!
   - `SoilMoisture: 45.0%` is **OK** (above 20%) → No alert
   - `BatteryLevel: 85.0%` is **OK** (above 15%) → No alert

   ### **6.2 Via RabbitMQ Management UI:**

   1. Access **Queues** → `analytics.sensor.reading.queue`
   2. Scroll to **Publish message** section
   3. **Payload:** Paste JSON from above
   4. **Properties:** 
      - delivery_mode: `2` (persistent)
      - content_type: `application/json`
   5. Click **Publish message**

   ### **6.3 Via curl (Alternative):**

   ```bash
   curl -u guest:guest -X POST \
     'http://localhost:15672/api/exchanges/%2F/sensor-readings/publish' \
     -H 'Content-Type: application/json' \
     -d '{
       "properties": {
         "delivery_mode": 2,
         "content_type": "application/json"
       },
       "routing_key": "sensor.reading",
       "payload": "{\"eventId\":\"12345678-1234-1234-1234-123456789012\",\"aggregateId\":\"87654321-4321-4321-4321-210987654321\",\"occurredOn\":\"2025-02-01T10:00:00Z\",\"eventName\":\"SensorReadingIntegrationEvent\",\"sensorId\":\"550e8400-e29b-41d4-a716-446655440001\",\"ownerId\":\"650e8400-e29b-41d4-a716-446655440001\",\"plotId\":\"750e8400-e29b-41d4-a716-446655440001\",\"timestamp\":\"2025-02-01T09:55:00Z\",\"temperature\":42.5,\"humidity\":65.0,\"soilMoisture\":45.0,\"rainfall\":2.5,\"batteryLevel\":85.0}",
       "payload_encoding": "string"
     }'
   ```

   ---

   ## ✅ **STEP 7: VERIFY PROCESSING**

   ### **7.1 Check Application Logs:**

   In the terminal where the application is running, you should see:

   ```
   info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedInHandler[0]
         Processing SensorReadingIntegrationEvent for Sensor 550e8400-e29b-41d4-a716-446655440001

   info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedInHandler[0]
         Alert created: Type=HighTemperature, Severity=Critical, 
         Value=42.5°C, Threshold=35.0°C

   info: TC.Agro.Analytics.Service.Services.AlertHubNotifier[0]
         Real-time notification sent via SignalR to subscribed clients

   info: TC.Agro.Analytics.Application.MessageBrokerHandlers.SensorIngestedInHandler[0]
         Sensor reading processed successfully for Sensor 550e8400-e29b-41d4-a716-446655440001
   ```

   ### **7.2 Check RabbitMQ Management UI:**

   1. Access **Queues** → `analytics.sensor.reading.queue`
   2. **Ready** messages → Should be **0** (message consumed)
   3. **Total** messages → Should show +1 in **Ack** (acknowledged)
   4. **Message rates** → Should show spike in "Deliver / get" and "Ack"

   ### **7.3 Check Database - Alerts Table:**

   ```bash
   docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db

   # In psql:
   SELECT 
     id,
     sensor_id,
     type,
     severity,
     status,
     message,
     value,
     threshold,
     created_at
   FROM analytics.alerts
   ORDER BY created_at DESC
   LIMIT 5;

   # Expected output:
   # id   | sensor_id    | type             | severity | status  | message                        | value | threshold | created_at
   # -----+--------------+------------------+----------+---------+--------------------------------+-------+-----------+-------------------
   # ...  | 550e8400-... | HighTemperature  | Critical | Pending | High temperature detected: ... | 42.5  | 35.0      | 2025-02-01 10:00

   \q
   ```

   ### **7.4 Check Database - Sensor Snapshots:**

   ```bash
   docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db

   # In psql:
   SELECT 
     id,
     label,
     plot_name,
     property_name,
     status,
     is_active,
     created_at
   FROM analytics.sensor_snapshots
   ORDER BY created_at DESC
   LIMIT 5;

   # Should show sensor snapshot if SensorRegisteredIntegrationEvent was processed

   \q
   ```

   ### **7.5 Check SignalR Notifications:**

   ```
   # In browser with signalr-test.html open:
   # Should see real-time alert appearing in the "Received Alerts" section:

   Alert Received:
     ID: ...
     Type: HighTemperature
     Severity: Critical
     Sensor: 550e8400-e29b-41d4-a716-446655440001
     Message: High temperature detected: 42.5°C (threshold: 35.0°C)
     Value: 42.5
     Timestamp: 2025-02-01T10:00:00Z
   ```

   ---

   ## 🌐 **STEP 8: TEST REST API**

   ### **8.1 Test GET /health:**

   ```bash
   curl http://localhost:5174/health | jq
   ```

   ### **8.2 Test GET /api/alerts/pending:**

   ```bash
   curl http://localhost:5174/api/alerts/pending | jq

   # Expected output:
   {
     "alerts": [
       {
         "id": "...",
         "sensorId": "550e8400-e29b-41d4-a716-446655440001",
         "sensorLabel": "Sensor 001",
         "plotName": "Plot A",
         "propertyName": "Farm XYZ",
         "ownerName": "John Doe",
         "type": "HighTemperature",
         "severity": "Critical",
         "status": "Pending",
         "message": "High temperature detected: 42.5°C (threshold: 35.0°C)",
         "value": 42.5,
         "threshold": 35.0,
         "createdAt": "2025-02-01T10:00:00Z",
         "acknowledgedAt": null,
         "resolvedAt": null
       }
     ],
     "totalCount": 1,
     "pageNumber": 1,
     "pageSize": 10
   }
   ```

   ### **8.3 Test GET /api/alerts/history/{sensorId}:**

   ```bash
   curl "http://localhost:5174/api/alerts/history/550e8400-e29b-41d4-a716-446655440001?days=30" | jq

   # Expected output: List of alerts for that sensor
   ```

   ### **8.4 Test GET /api/alerts/status/{sensorId}:**

   ```bash
   curl "http://localhost:5174/api/alerts/status/550e8400-e29b-41d4-a716-446655440001" | jq

   # Expected output:
   {
     "sensorId": "550e8400-e29b-41d4-a716-446655440001",
     "sensorLabel": "Sensor 001",
     "plotName": "Plot A",
     "propertyName": "Farm XYZ",
     "ownerName": "John Doe",
     "status": "Active",
     "overallHealthStatus": "Critical",
     "pendingAlertCount": 1,
     "criticalAlertCount": 1,
     "last24HoursAlertCount": 1,
     "last7DaysAlertCount": 1,
     "alertsByType": {
       "HighTemperature": 1
     },
     "alertsBySeverity": {
       "Critical": 1
     },
     "lastAlertDate": "2025-02-01T10:00:00Z"
   }
   ```

   ### **8.5 Test POST /api/alerts/{id}/acknowledge:**

   ```bash
   # Get alert ID from pending alerts
   ALERT_ID=$(curl -s http://localhost:5174/api/alerts/pending | jq -r '.alerts[0].id')

   # Acknowledge alert
   curl -X POST "http://localhost:5174/api/alerts/$ALERT_ID/acknowledge" \
     -H "Content-Type: application/json" \
     -d '{
       "userId": "650e8400-e29b-41d4-a716-446655440001"
     }' | jq

   # Expected output:
   {
     "success": true,
     "alertId": "...",
     "newStatus": "Acknowledged",
     "acknowledgedAt": "2025-02-01T10:05:00Z",
     "acknowledgedBy": "650e8400-e29b-41d4-a716-446655440001"
   }
   ```

   ### **8.6 Test POST /api/alerts/{id}/resolve:**

   ```bash
   # Resolve alert
   curl -X POST "http://localhost:5174/api/alerts/$ALERT_ID/resolve" \
     -H "Content-Type: application/json" \
     -d '{
       "userId": "650e8400-e29b-41d4-a716-446655440001",
       "resolutionNotes": "Temperature normalized after irrigation"
     }' | jq

   # Expected output:
   {
     "success": true,
     "alertId": "...",
     "newStatus": "Resolved",
     "resolvedAt": "2025-02-01T10:10:00Z",
     "resolvedBy": "650e8400-e29b-41d4-a716-446655440001",
     "resolutionNotes": "Temperature normalized after irrigation"
   }
   ```

   ---

   ## 🧪 **STEP 9: TEST ADDITIONAL SCENARIOS**

   ### **Scenario 1: Low Soil Moisture Alert**

   ```bash
   python scripts/publish_test_message.py --scenario low-soil
   ```

   **Expected Result:**
   - ✅ `LowSoilMoisture` alert created (soil moisture < 20%)
   - ✅ Severity: High or Critical (depending on value)
   - ✅ SignalR notification sent
   - ✅ API returns new alert

   ### **Scenario 2: Battery Low Warning**

   ```bash
   python scripts/publish_test_message.py --scenario low-battery
   ```

   **Expected Result:**
   - ✅ `LowBattery` alert created (battery level < 15%)
   - ✅ Severity: Medium or High
   - ✅ SignalR notification sent
   - ✅ API returns new alert

   ### **Scenario 3: Multiple Simultaneous Alerts**

   ```bash
   python scripts/publish_test_message.py --scenario multiple
   ```

   **Expected Result:**
   - ✅ 3 alerts created simultaneously:
     - HighTemperature
     - LowSoilMoisture
     - LowBattery
   - ✅ All alerts have Critical/High severity
   - ✅ SignalR sends 3 notifications
   - ✅ API returns all 3 alerts

   ### **Scenario 4: Normal Values (No Alerts)**

   ```bash
   python scripts/publish_test_message.py --scenario normal
   ```

   **Expected Result:**
   - ✅ Message processed successfully
   - ❌ NO alerts created (all values within thresholds)
   - ❌ NO SignalR notifications
   - ✅ Logs show: "No alerts detected"

   ### **Scenario 5: Duplicate Message (Idempotency)**

   ```bash
   # Publish same message twice
   python scripts/publish_test_message.py --scenario high-temp
   python scripts/publish_test_message.py --scenario high-temp
   ```

   **Expected Result:**
   - ✅ First message: Alert created
   - ⚠️ Second message: Duplicate detected (same sensorId + timestamp)
   - ❌ NO duplicate alert created
   - ✅ Logs show: "Duplicate sensor reading detected, skipping alert creation"

   ---

   ## 🔄 **STEP 10: TEST ALERT LIFECYCLE**

   ### **Complete Lifecycle Test:**

   ```bash
   # 1. Create alert (high temperature)
   python scripts/publish_test_message.py --scenario high-temp

   # 2. Get alert ID
   ALERT_ID=$(curl -s http://localhost:5174/api/alerts/pending | jq -r '.alerts[0].id')
   echo "Alert ID: $ALERT_ID"

   # 3. Check status (should be Pending)
   curl "http://localhost:5174/api/alerts/history/550e8400-e29b-41d4-a716-446655440001" | jq '.alerts[] | select(.id=="'$ALERT_ID'") | .status'
   # Output: "Pending"

   # 4. Acknowledge alert
   curl -X POST "http://localhost:5174/api/alerts/$ALERT_ID/acknowledge" \
     -H "Content-Type: application/json" \
     -d '{"userId": "650e8400-e29b-41d4-a716-446655440001"}' | jq

   # 5. Check status (should be Acknowledged)
   curl "http://localhost:5174/api/alerts/history/550e8400-e29b-41d4-a716-446655440001" | jq '.alerts[] | select(.id=="'$ALERT_ID'") | .status'
   # Output: "Acknowledged"

   # 6. Resolve alert
   curl -X POST "http://localhost:5174/api/alerts/$ALERT_ID/resolve" \
     -H "Content-Type: application/json" \
     -d '{
       "userId": "650e8400-e29b-41d4-a716-446655440001",
       "resolutionNotes": "Issue resolved - temperature back to normal"
     }' | jq

   # 7. Check status (should be Resolved)
   curl "http://localhost:5174/api/alerts/history/550e8400-e29b-41d4-a716-446655440001" | jq '.alerts[] | select(.id=="'$ALERT_ID'") | .status'
   # Output: "Resolved"

   # 8. Verify SignalR received 3 events:
   #    - ReceiveAlert (when created)
   #    - AlertAcknowledged (when acknowledged)
   #    - AlertResolved (when resolved)
   ```

   ---

   ## 🎯 **STEP 11: VALIDATION CHECKLIST**

   After completing all steps, verify:

   - [ ] ✅ Docker containers running (PostgreSQL + RabbitMQ)
   - [ ] ✅ Database migrations applied
   - [ ] ✅ RabbitMQ exchanges and queues configured
   - [ ] ✅ Application starts without errors
   - [ ] ✅ Wolverine connects to RabbitMQ
   - [ ] ✅ Messages consumed from queues
   - [ ] ✅ Alerts created in database
   - [ ] ✅ Sensor snapshots created/updated
   - [ ] ✅ SignalR hub accessible
   - [ ] ✅ Real-time notifications working
   - [ ] ✅ REST API endpoints returning correct data
   - [ ] ✅ Alert lifecycle working (Pending → Acknowledged → Resolved)
   - [ ] ✅ All test scenarios passing
   - [ ] ✅ No critical errors in logs

   ---

   ## 🏆 **SUCCESS!**

   If all validation items are checked, you have successfully:

   ```
   ╔══════════════════════════════════════════════════════════╗
   ║                                                          ║
   ║   🎉 E2E TESTING COMPLETE AND SUCCESSFUL! 🎉            ║
   ║                                                          ║
   ║  ✅ Infrastructure: Docker + PostgreSQL + RabbitMQ      ║
   ║  ✅ Database: Migrations + Tables + Indexes             ║
   ║  ✅ Messaging: WolverineFx + 3 Queues + 3 Exchanges     ║
   ║  ✅ Domain Logic: AlertAggregate + Business Rules       ║
   ║  ✅ Persistence: EF Core + Snapshots                    ║
   ║  ✅ API: FastEndpoints + 6 Endpoints                    ║
   ║  ✅ Real-time: SignalR + WebSocket Notifications        ║
   ║  ✅ Lifecycle: Pending → Acknowledged → Resolved        ║
   ║                                                          ║
   ║  🎯 STATUS: PRODUCTION READY                            ║
   ║                                                          ║
   ╚══════════════════════════════════════════════════════════╝
   ```

   ---

   **Congratulations! Your Analytics Worker is fully tested and ready for production deployment!** 🚀

   ---

   **Documentation Version:** 2.0  
   **Last Updated:** February 2025  
   **Status:** ✅ Complete  
   **Target Framework:** .NET 10


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
