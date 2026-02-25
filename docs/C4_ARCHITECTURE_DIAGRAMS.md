# 📐 **C4 MODEL - ARCHITECTURE DIAGRAMS**
## **Analytics Worker - TC.Agro Solutions**

**Visual Architecture Documentation using C4 Model**

---

## 📚 **About the C4 Model**

The **C4 Model** is a software architecture documentation approach created by Simon Brown. It consists of 4 levels of abstraction:

1. **Context** - System and its users/external systems
2. **Container** - Applications, databases, services
3. **Component** - Internal components of each container
4. **Code** - Classes and interfaces (usually not needed)

**Reference:** https://c4model.com/

---

## 🌍 **LEVEL 1: CONTEXT DIAGRAM**

### **System in the Context of TC.Agro Ecosystem**

```mermaid
C4Context
    title System Context Diagram - Analytics Worker

    Person(farmer, "Farmer", "Monitors farm alerts and sensor data")
    Person(admin, "Administrator", "Manages system and configurations")

    System(analyticsWorker, "Analytics Worker", "Processes sensor data, detects anomalies, and manages alert lifecycle with real-time notifications")

    System_Ext(farmService, "Farm Management Service", "Manages farms, plots, and sensor registrations")
    System_Ext(sensorIngest, "Sensor Ingest Service", "Collects IoT sensor data and publishes readings")
    System_Ext(dashboardUI, "Dashboard UI", "Web interface for data visualization and real-time alerts")

    SystemDb_Ext(supabase, "Supabase", "Cloud-managed PostgreSQL database")
    SystemQueue_Ext(rabbitmq, "RabbitMQ", "Message Broker for event-driven communication")

    Rel(farmService, rabbitmq, "Publishes sensor lifecycle events", "SensorRegistered, SensorDeactivated, etc.")
    Rel(sensorIngest, rabbitmq, "Publishes sensor readings", "SensorReadingIntegrationEvent")
    Rel(rabbitmq, analyticsWorker, "Delivers events", "AMQP")

    Rel(analyticsWorker, supabase, "Persists alerts & snapshots", "PostgreSQL Protocol")
    Rel(analyticsWorker, rabbitmq, "Publishes alert events", "AMQP")

    Rel(dashboardUI, analyticsWorker, "Queries alerts", "HTTP/JSON")
    Rel(analyticsWorker, dashboardUI, "Pushes real-time updates", "SignalR WebSocket")

    Rel(farmer, dashboardUI, "Views alerts & status", "HTTPS")
    Rel(admin, dashboardUI, "Manages alerts", "HTTPS")

    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="2")
```

**Description:**

- **Analytics Worker** is the central system for sensor data analysis and alert management
- Consumes events from **Farm Management Service** (sensor lifecycle) and **Sensor Ingest Service** (sensor readings) via RabbitMQ
- Applies business rules to detect anomalies and create alerts
- Manages full alert lifecycle: Pending → Acknowledged → Resolved
- Exposes REST API for **Dashboard UI** to query alerts
- Provides real-time notifications via **SignalR** WebSocket connections
- Maintains sensor snapshots for optimized queries
- Persists alerts and snapshots in **Supabase** PostgreSQL

---

## 📦 **LEVEL 2: CONTAINER DIAGRAM**

### **Containers and Technologies of Analytics Worker**

```mermaid
C4Container
    title Container Diagram - Analytics Worker

    Person(farmer, "Farmer", "End user")
    System_Ext(farmService, "Farm Management Service", "Publishes sensor lifecycle events")
    System_Ext(sensorIngest, "Sensor Ingest Service", "Publishes sensor readings")

    Container_Boundary(analyticsWorker, "Analytics Worker") {
        Container(api, "Analytics API", ".NET 10, FastEndpoints, SignalR", "Exposes REST endpoints and WebSocket hub for alerts")
        Container(worker, "Message Handlers", ".NET 10, WolverineFx", "Processes events and applies business rules")
        ContainerDb(readDb, "Read Database", "EF Core + PostgreSQL", "Alerts and sensor snapshots for optimized queries")
        Container(hubService, "SignalR Hub", "AlertHub", "Real-time push notifications to connected clients")
    }

    ContainerQueue(messageBroker, "Message Broker", "RabbitMQ", "Distributes events between services")

    Rel(farmService, messageBroker, "Publishes", "SensorRegistered, StatusChanged, etc.")
    Rel(sensorIngest, messageBroker, "Publishes", "SensorReadingIntegrationEvent")
    Rel(messageBroker, worker, "Consumes", "AMQP")

    Rel(worker, readDb, "Creates/Updates alerts", "EF Core")
    Rel(worker, hubService, "Triggers notifications", "IAlertHubNotifier")
    Rel(hubService, farmer, "Pushes alerts", "SignalR/WebSocket")

    Rel(worker, messageBroker, "Publishes alert events", "AMQP")

    Rel(farmer, api, "Queries alerts", "HTTPS/JSON")
    Rel(api, readDb, "Reads alerts", "EF Core")
    Rel(api, hubService, "Hosts WebSocket", "SignalR")

    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
```

**Technologies by Container:**

| Container | Technology | Purpose |
|-----------|------------|---------|
| **Analytics API** | .NET 10 + FastEndpoints | REST endpoints for alert queries (CQRS Query Side) |
| **Message Handlers** | WolverineFx + EF Core | Processes events and domain logic (CQRS Command Side) |
| **Read Database** | EF Core + PostgreSQL | Denormalized alerts and sensor snapshots |
| **SignalR Hub** | ASP.NET Core SignalR | Real-time WebSocket notifications to Dashboard UI |
| **Message Broker** | RabbitMQ | Asynchronous event-driven communication |

---

## 🧩 **LEVEL 3: COMPONENT DIAGRAM**

### **3.1 Analytics API (Query Side)**

```mermaid
C4Component
    title Component Diagram - Analytics API (CQRS Query Side)

    Container_Boundary(api, "Analytics API") {
        Component(endpoints, "Alert Endpoints", "FastEndpoints", "Exposes REST API")
        Component(queryHandlers, "Query Handlers", "C#", "Executes optimized queries")
        Component(models, "Response Models", "DTOs", "API response contracts")
        Component(signalRHub, "AlertHub", "SignalR", "WebSocket hub for real-time updates")
    }

    ContainerDb(readDb, "Read Database", "PostgreSQL", "alerts & sensor_snapshots tables")
    Person(client, "Client", "Dashboard UI / Mobile App")

    Rel(client, signalRHub, "Connects", "WebSocket")
    Rel(client, endpoints, "GET /api/alerts/pending", "HTTPS")
    Rel(client, endpoints, "GET /api/alerts/history/{sensorId}", "HTTPS")
    Rel(client, endpoints, "GET /api/alerts/status/{sensorId}", "HTTPS")
    Rel(client, endpoints, "POST /api/alerts/{id}/acknowledge", "HTTPS")
    Rel(client, endpoints, "POST /api/alerts/{id}/resolve", "HTTPS")

    Rel(endpoints, queryHandlers, "Delegates query", "Method call")
    Rel(queryHandlers, readDb, "Optimized SELECT", "EF Core")
    Rel(queryHandlers, models, "Maps to DTO", "")
    Rel(endpoints, client, "Returns JSON", "HTTPS")

    Rel(signalRHub, client, "Pushes alert events", "WebSocket")

    UpdateLayoutConfig($c4ShapeInRow="3")
```

**Query Side Components:**

1. **Alert Endpoints** (`AlertEndpoints/`)
   - `GetPendingAlertsEndpoint` - Lists pending alerts
   - `GetAlertHistoryEndpoint` - Alert history for a sensor
   - `GetSensorStatusEndpoint` - Current sensor status overview
   - `AcknowledgeAlertEndpoint` - Acknowledges an alert
   - `ResolveAlertEndpoint` - Resolves an alert

2. **Query Handlers** (`UseCases/Alerts/`)
   - `GetPendingAlertsQueryHandler`
   - `GetAlertHistoryQueryHandler`
   - `GetSensorStatusQueryHandler`

3. **Command Handlers** (`UseCases/Alerts/`)
   - `AcknowledgeAlertCommandHandler`
   - `ResolveAlertCommandHandler`

4. **Response Models** (Various DTOs)
   - `AlertDto`
   - `SensorStatusDto`
   - `AlertHistoryDto`

5. **SignalR Hub** (`Hubs/AlertHub.cs`)
   - `SubscribeToAlerts(sensorIds)` - Subscribe to specific sensor alerts
   - `UnsubscribeFromAlerts(sensorIds)` - Unsubscribe from updates
   - Real-time event push: `ReceiveAlert`, `AlertAcknowledged`, `AlertResolved`

---

### **3.2 Message Handlers (Command Side)**

```mermaid
C4Component
    title Component Diagram - Message Handlers (CQRS Command Side)

    Container_Boundary(worker, "Message Handlers") {
        Component(sensorHandler, "SensorIngestedInHandler", "WolverineFx", "Processes sensor readings")
        Component(snapshotHandler, "SensorSnapshotHandler", "WolverineFx", "Maintains sensor snapshots")
        Component(ownerHandler, "OwnerSnapshotHandler", "WolverineFx", "Maintains owner snapshots")
        Component(aggregate, "AlertAggregate", "Domain Model", "Alert lifecycle & business rules")
        Component(snapshot, "SensorSnapshot", "Domain Model", "Cached sensor metadata")
        Component(notifier, "AlertHubNotifier", "Service", "Triggers SignalR notifications")
    }

    ContainerQueue(rabbitmq, "RabbitMQ", "Message Broker")
    ContainerDb(readDb, "Read Database", "PostgreSQL")
    Container(signalRHub, "AlertHub", "SignalR")

    Rel(rabbitmq, sensorHandler, "SensorReadingIntegrationEvent", "AMQP")
    Rel(rabbitmq, snapshotHandler, "SensorRegistered, StatusChanged", "AMQP")
    Rel(rabbitmq, ownerHandler, "UserRegistered, UserDeactivated", "AMQP")

    Rel(sensorHandler, aggregate, "CreateFromSensorData()", "Domain logic")
    Rel(sensorHandler, snapshot, "Loads metadata", "EF Core")
    Rel(aggregate, readDb, "Saves alerts", "EF Core")

    Rel(snapshotHandler, snapshot, "Creates/Updates", "EF Core")
    Rel(snapshot, readDb, "Persists", "EF Core")

    Rel(sensorHandler, notifier, "NotifyNewAlert()", "Service call")
    Rel(notifier, signalRHub, "Sends to clients", "SignalR IHubContext")

    Rel(aggregate, rabbitmq, "Publishes domain events", "AMQP")

    UpdateLayoutConfig($c4ShapeInRow="3")
```

**Command Side Components:**

1. **SensorIngestedInHandler** (`MessageBrokerHandlers/SensorIngestedInHandler.cs`)
   - Processes `SensorReadingIntegrationEvent`
   - Loads `SensorSnapshot` for metadata (plot, owner, thresholds)
   - Invokes `AlertAggregate.CreateFromSensorData()` with business rules
   - Persists alerts via `IAlertAggregateRepository`
   - Triggers real-time notifications via `IAlertHubNotifier`

2. **SensorSnapshotHandler** (`MessageBrokerHandlers/SensorSnapshotHandler.cs`)
   - Processes sensor lifecycle events:
     - `SensorRegisteredIntegrationEvent` → Create snapshot
     - `SensorOperationalStatusChangedIntegrationEvent` → Update status
     - `SensorDeactivatedIntegrationEvent` → Mark inactive
   - Maintains `sensor_snapshots` table for optimized queries

3. **OwnerSnapshotHandler** (`MessageBrokerHandlers/OwnerSnapshotHandler.cs`)
   - Processes `UserRegisteredIntegrationEvent` and `UserDeactivatedIntegrationEvent`
   - Maintains `owner_snapshots` table

4. **AlertAggregate** (`Aggregates/AlertAggregate.cs`)
   - DDD Aggregate Root with full lifecycle
   - Factory method: `CreateFromSensorData()` applies business rules:
     - Temperature > threshold → HighTemperature alert
     - SoilMoisture < threshold → LowSoilMoisture alert
     - BatteryLevel < threshold → LowBattery alert
   - State transitions: Pending → Acknowledged → Resolved
   - Methods: `Acknowledge()`, `Resolve()`

5. **SensorSnapshot** (`Snapshots/SensorSnapshot.cs`)
   - Domain entity (not aggregate)
   - Denormalized sensor data: owner, plot, property names
   - Optimizes queries by avoiding cross-service calls
   - Updated by events from Farm Management Service

6. **AlertHubNotifier** (`Services/AlertHubNotifier.cs`)
   - Implements `IAlertHubNotifier` interface
   - Injects `IHubContext<AlertHub>`
   - Pushes real-time notifications to subscribed SignalR clients
   - Methods: `NotifyNewAlert()`, `NotifyAlertAcknowledged()`, `NotifyAlertResolved()`

---

## 🏗️ **CLEAN ARCHITECTURE - LAYERS**

### **Layers and Dependencies**

```mermaid
graph TB
    subgraph "Presentation Layer"
        API[Analytics API<br/>FastEndpoints + SignalR]
    end

    subgraph "Application Layer"
        Handlers[Message Handlers<br/>SensorIngestedInHandler]
        QueryHandlers[Query Handlers<br/>GetPendingAlertsQueryHandler]
        CommandHandlers[Command Handlers<br/>AcknowledgeAlertCommandHandler]
        Ports[Ports/Interfaces<br/>IAlertAggregateRepository]
        Notifier[Services<br/>IAlertHubNotifier]
    end

    subgraph "Domain Layer"
        Aggregates[Aggregates<br/>AlertAggregate]
        Entities[Entities<br/>SensorSnapshot, OwnerSnapshot]
        ValueObjects[Value Objects<br/>AlertThresholds, AlertType]
        DomainErrors[Domain Errors<br/>AnalyticsDomainErrors]
    end

    subgraph "Infrastructure Layer"
        Repositories[Repositories<br/>AlertAggregateRepository]
        Snapshots[Snapshot Stores<br/>SensorSnapshotStore]
        Persistence[Persistence<br/>EF Core + Migrations]
        Messaging[Messaging<br/>WolverineFx + RabbitMQ]
        SignalR[Real-time<br/>AlertHubNotifier]
    end

    API --> QueryHandlers
    API --> CommandHandlers
    API --> SignalR

    QueryHandlers --> Ports
    CommandHandlers --> Ports
    Handlers --> Aggregates
    Handlers --> Entities
    Handlers --> Notifier

    Aggregates --> ValueObjects
    Aggregates --> DomainErrors

    Repositories --> Ports
    Snapshots --> Entities
    Persistence --> Ports
    Messaging --> Handlers
    SignalR --> Notifier

    style Domain Layer fill:#90EE90
    style API fill:#87CEEB
    style Handlers fill:#FFD700
    style Repositories fill:#FFA07A
```

**Clean Architecture Principles:**

| Layer | Description | Dependencies |
|-------|-------------|--------------|
| **Domain** | Business logic, aggregates, entities, value objects | None (pure domain) |
| **Application** | Use cases, handlers, ports (interfaces) | Domain only |
| **Infrastructure** | Repositories, EF Core, messaging, SignalR | Application, Domain |
| **Presentation** | FastEndpoints, SignalR Hub, DTOs | Application, Domain |

**Dependency Rule:**
- ✅ Domain has zero dependencies (pure business logic)
- ✅ Application depends only on Domain
- ✅ Infrastructure implements Application interfaces (Dependency Inversion)
- ✅ Presentation depends on Application (controllers/endpoints call use cases)

**Key Projects:**

1. **TC.Agro.Analytics.Domain** - Core domain models
2. **TC.Agro.Analytics.Application** - Use cases and handlers
3. **TC.Agro.Analytics.Infrastructure** - EF Core, repositories, persistence
4. **TC.Agro.Analytics.Service** - FastEndpoints API + SignalR Hub
5. **TC.Agro.Contracts** - Integration event contracts (shared)
6. **TC.Agro.SharedKernel** - Common patterns (BaseAggregateRoot, Result, etc.)
7. **TC.Agro.Messaging** - WolverineFx extensions

---

## 🔄 **EVENT FLOW - SEQUENCE DIAGRAM**

### **Complete Flow: Sensor Reading → Alert → Real-time Notification**

```mermaid
sequenceDiagram
    participant Sensor as 🌡️ IoT Sensor
    participant Ingest as Sensor Ingest
    participant RabbitMQ as 🐰 RabbitMQ
    participant Worker as Analytics Worker
    participant Postgres as 🗄️ PostgreSQL
    participant Hub as 🔔 AlertHub (SignalR)
    participant UI as 💻 Dashboard UI

    Sensor->>Ingest: Sends reading<br/>(temp: 45°C)
    Ingest->>RabbitMQ: Publishes SensorReadingIntegrationEvent

    RabbitMQ->>Worker: Consumes event (WolverineFx)

    rect rgb(200, 220, 250)
        Note over Worker: Command Side - Business Logic
        Worker->>Postgres: Loads SensorSnapshot<br/>(plot, owner, thresholds)
        Postgres-->>Worker: Returns metadata
        Worker->>Worker: AlertAggregate.CreateFromSensorData()<br/>🔥 Temperature > 35°C → Alert!
        Worker->>Postgres: INSERT INTO alerts<br/>(status: Pending, severity: Critical)
    end

    rect rgb(250, 220, 200)
        Note over Worker: Real-time Notification
        Worker->>Hub: IAlertHubNotifier.NotifyNewAlert()
        Hub->>UI: Pushes alert via WebSocket<br/>"⚠️ Critical temperature!"
    end

    Worker->>RabbitMQ: Publishes AlertCreatedEvent<br/>(for other services)

    UI->>Worker: GET /api/alerts/pending
    Worker->>Postgres: SELECT * FROM alerts<br/>WHERE status = 'Pending'
    Postgres-->>Worker: Results
    Worker-->>UI: JSON Response

    UI->>Worker: POST /api/alerts/{id}/acknowledge
    Worker->>Postgres: UPDATE alerts SET status = 'Acknowledged'
    Worker->>Hub: NotifyAlertAcknowledged()
    Hub->>UI: Pushes update via WebSocket
```

**Detailed Flow:**

1. **IoT Sensor** sends reading (temp: 45°C) to Sensor Ingest Service
2. **Sensor Ingest** validates and publishes `SensorReadingIntegrationEvent` to RabbitMQ
3. **RabbitMQ** routes event to **Analytics Worker** (WolverineFx handler)
4. **Worker (Command Side):**
   - `SensorIngestedInHandler` receives event
   - Loads `SensorSnapshot` from PostgreSQL (contains plot, owner, thresholds)
   - Invokes `AlertAggregate.CreateFromSensorData()` → applies business rules
   - Detects: temperature (45°C) > threshold (35°C) → creates **HighTemperature** alert
   - Persists alert to PostgreSQL via `IAlertAggregateRepository`
5. **Worker (Real-time Notification):**
   - `IAlertHubNotifier.NotifyNewAlert()` triggers SignalR notification
   - `AlertHub` pushes event to subscribed clients via WebSocket
   - Dashboard UI receives instant notification without polling
6. **Worker** publishes `AlertCreatedIntegrationEvent` to RabbitMQ (for other services)
7. **Dashboard UI** queries alerts via REST API (CQRS Query Side)
8. **User** acknowledges alert → triggers state transition (Pending → Acknowledged)
9. **SignalR** pushes acknowledgment update to all subscribed clients

---

## 🗄️ **DATA FLOW - CQRS PATTERN**

### **Command/Query Separation**

```mermaid
graph LR
    subgraph "Command Side (Write)"
        CMD[Integration Events] --> Handler[Message Handlers]
        Handler --> AGG[AlertAggregate<br/>Business Rules]
        AGG --> DB[(PostgreSQL<br/>alerts table)]
    end

    subgraph "Real-time Notifications"
        Handler --> Notifier[AlertHubNotifier]
        Notifier --> SignalR[SignalR Hub]
        SignalR --> Clients[Connected Clients]
    end

    subgraph "Query Side (Read)"
        API[REST API] --> QH[Query Handlers]
        QH --> DB
        DB --> API
    end

    style CMD fill:#FFB6C1
    style AGG fill:#90EE90
    style DB fill:#87CEEB
    style Notifier fill:#FFD700
    style SignalR fill:#FFA07A
    style API fill:#F0E68C
```

**CQRS Benefits:**

| Aspect | Command Side | Query Side |
|--------|--------------|------------|
| **Model** | Rich Aggregate (AlertAggregate) | Simple DTOs |
| **Persistence** | EF Core (write-optimized) | EF Core (read-optimized) |
| **Optimization** | Business rule validation | Fast queries with indexes |
| **Consistency** | Strong (transactional) | Strong (same database) |
| **Scalability** | Vertical | Horizontal (read replicas) |
| **Real-time** | Triggers SignalR notifications | Polls via HTTP requests |

**Why CQRS in Analytics Worker?**

1. **Separate concerns:** Commands apply business rules; queries optimize for reads
2. **Performance:** Query side uses denormalized `sensor_snapshots` for fast joins
3. **Real-time:** Command side triggers immediate SignalR notifications
4. **Scalability:** Read replicas can scale independently
5. **Maintainability:** Clear separation of write and read logic

---

## 📊 **DEPLOYMENT DIAGRAM**

### **Cloud Infrastructure (Supabase + Railway/Render)**

```mermaid
C4Deployment
    title Deployment Diagram - Production Environment

    Deployment_Node(cloudProvider, "Cloud Platform", "Railway / Render / Azure") {
        Deployment_Node(appServer, "Application Server", "Container / App Service") {
            Container(api, "Analytics API", ".NET 10")
            Container(worker, "Message Handlers", ".NET 10")
            Container(signalr, "SignalR Hub", "WebSocket")
        }
    }

    Deployment_Node(supabase, "Supabase", "Managed PostgreSQL") {
        ContainerDb(postgres, "PostgreSQL 15+", "Database")
    }

    Deployment_Node(cloudamqp, "CloudAMQP", "Managed RabbitMQ") {
        ContainerQueue(rabbitmq, "RabbitMQ 3.x", "Message Broker")
    }

    Deployment_Node(monitoring, "Observability", "Azure Monitor / Grafana Cloud") {
        Container(logs, "Application Insights", "Logs & Traces")
        Container(metrics, "Prometheus", "Metrics")
    }

    Rel(api, postgres, "Queries & Commands", "PostgreSQL Protocol")
    Rel(worker, postgres, "Writes alerts", "PostgreSQL Protocol")
    Rel(worker, rabbitmq, "Pub/Sub", "AMQP")
    Rel(signalr, api, "WebSocket connections", "SignalR")

    Rel(api, logs, "Telemetry", "OTLP/HTTP")
    Rel(worker, logs, "Telemetry", "OTLP/HTTP")
    Rel(api, metrics, "Metrics", "Prometheus")

    UpdateLayoutConfig($c4ShapeInRow="2")
```

**Infrastructure:**

- **Application:** Railway/Render/Azure App Service (containerized .NET 10 app)
- **Database:** Supabase PostgreSQL 15+ (managed, with connection pooling)
- **Message Broker:** CloudAMQP RabbitMQ (managed, with high availability)
- **Real-time:** SignalR hub running in same process as API (sticky sessions required for scale-out)
- **Observability:** Azure Monitor Application Insights + Prometheus for metrics

**Deployment Considerations:**

1. **SignalR Sticky Sessions:** Required when scaling to multiple instances (use Redis backplane for scale-out)
2. **Database Migrations:** Applied automatically on startup via `ApplyMigrations()` extension
3. **Environment Variables:** Secrets managed via platform environment variables (Railway/Render secrets, Azure Key Vault)
4. **Health Checks:** `/health` endpoint for container orchestration
5. **CORS:** Configured for Dashboard UI domain
6. **Ingress Path Base:** Supports nginx `rewrite-target` for path-based routing

---

## 🎨 **DOMAIN MODEL - CLASS DIAGRAM**

### **Core Domain Elements**

```mermaid
classDiagram
    class AlertAggregate {
        +Guid Id
        +Guid SensorId
        +AlertType Type
        +AlertSeverity Severity
        +AlertStatus Status
        +string Message
        +double Value
        +double Threshold
        +string? Metadata
        +DateTimeOffset CreatedAt
        +DateTimeOffset? AcknowledgedAt
        +Guid? AcknowledgedBy
        +DateTimeOffset? ResolvedAt
        +Guid? ResolvedBy
        +string? ResolutionNotes
        +CreateFromSensorData() Result~List~AlertAggregate~~
        +Acknowledge(userId, timestamp) Result
        +Resolve(userId, notes, timestamp) Result
    }

    class AlertType {
        <<enumeration>>
        HighTemperature
        LowSoilMoisture
        LowBattery
        SensorOffline
    }

    class AlertSeverity {
        <<enumeration>>
        Low
        Medium
        High
        Critical
    }

    class AlertStatus {
        <<enumeration>>
        Pending
        Acknowledged
        Resolved
        Expired
    }

    class AlertThresholds {
        +double MaxTemperature
        +double MinSoilMoisture
        +double MinBatteryLevel
        +IsTemperatureCritical(value) bool
        +IsSoilMoistureLow(value) bool
        +IsBatteryLow(value) bool
    }

    class SensorSnapshot {
        +Guid Id
        +Guid OwnerId
        +Guid PropertyId
        +Guid PlotId
        +string? Label
        +string PlotName
        +string PropertyName
        +string? Status
        +bool IsActive
        +DateTimeOffset CreatedAt
        +DateTimeOffset? UpdatedAt
        +ICollection~AlertAggregate~ Alerts
        +CreateFromEvent(event) SensorSnapshot
        +UpdateStatus(status, timestamp) void
        +Deactivate(timestamp) void
    }

    class OwnerSnapshot {
        +Guid Id
        +string FirstName
        +string LastName
        +string Email
        +bool IsActive
        +DateTimeOffset CreatedAt
        +ICollection~SensorSnapshot~ Sensors
        +CreateFromEvent(event) OwnerSnapshot
        +Deactivate() void
    }

    AlertAggregate --> AlertType : type
    AlertAggregate --> AlertSeverity : severity
    AlertAggregate --> AlertStatus : status
    AlertAggregate ..> AlertThresholds : uses for validation
    AlertAggregate --> SensorSnapshot : belongs to
    SensorSnapshot --> OwnerSnapshot : belongs to
```

**DDD Patterns Implemented:**

- ✅ **Aggregate Root:** `AlertAggregate` (manages alert lifecycle)
- ✅ **Entity:** `SensorSnapshot`, `OwnerSnapshot` (domain entities with identity)
- ✅ **Value Object:** `AlertThresholds` (immutable business rules)
- ✅ **Enumerations:** `AlertType`, `AlertSeverity`, `AlertStatus` (domain vocabulary)
- ✅ **Factory Methods:** `CreateFromSensorData()`, `CreateFromEvent()`
- ✅ **Repository Pattern:** `IAlertAggregateRepository`, `ISensorSnapshotStore`
- ✅ **Ubiquitous Language:** Domain terms match business terminology

**Alert Lifecycle State Machine:**

```
Pending → Acknowledged → Resolved
   ↓
Expired (if not acknowledged within time window)
```

**Business Rules (in AlertAggregate):**

1. **Temperature Rule:** `temperature > AlertThresholds.MaxTemperature` → HighTemperature alert (Critical severity)
2. **Soil Moisture Rule:** `soilMoisture < AlertThresholds.MinSoilMoisture` → LowSoilMoisture alert (High severity)
3. **Battery Rule:** `batteryLevel < AlertThresholds.MinBatteryLevel` → LowBattery alert (Medium severity)
4. **State Transition Rules:**
   - Can only acknowledge alert in Pending state
   - Can only resolve alert in Acknowledged state
   - Cannot modify alert in Resolved or Expired state

---

## 📈 **PERFORMANCE & SCALABILITY**

### **Optimization Strategies**

```mermaid
graph TB
    subgraph "Read Side Optimization"
        API[API Request] --> Cache{Future: Redis Cache}
        Cache -->|Hit| Response[Fast Response]
        Cache -->|Miss| DB[PostgreSQL]
        DB --> Index[Indexes on alerts table]
        Index --> Response
    end

    subgraph "Write Side Optimization"
        Event[Integration Event] --> Handler{Message Handler}
        Handler --> Batch{Batch Processing}
        Batch --> DB2[(PostgreSQL)]
        Handler --> SignalR[SignalR Push]
    end

    subgraph "Horizontal Scaling"
        LB[Load Balancer] --> API1[API Instance 1]
        LB --> API2[API Instance 2]
        LB --> API3[API Instance N]
        Redis[Redis Backplane] --> API1
        Redis --> API2
        Redis --> API3
    end

    style Cache fill:#90EE90
    style Index fill:#FFD700
    style SignalR fill:#FFA07A
    style Redis fill:#87CEEB
```

**Optimizations Implemented:**

1. **Indexes:** Database indexes on `alerts` table for fast queries
   - `idx_alerts_sensor_status` - Composite index on (SensorId, Status, CreatedAt)
   - `idx_alerts_created_at` - Index on CreatedAt for time-based queries
   - `idx_sensor_snapshots_owner` - Index on OwnerId for owner-based queries

2. **Denormalized Snapshots:** `sensor_snapshots` table contains pre-joined data (plot, property, owner names)
   - Eliminates cross-service calls during query time
   - Updated asynchronously via events

3. **Connection Pooling:** Npgsql connection pooling enabled in connection string

4. **Async Processing:** All handlers use async/await for non-blocking I/O

5. **Real-time Push:** SignalR avoids polling overhead for alert updates

**Future Optimizations:**

- ⚠️ **Caching Layer:** Redis for frequently accessed data (pending)
- ⚠️ **Read Replicas:** PostgreSQL read replicas for query scaling (pending)
- ⚠️ **SignalR Backplane:** Redis backplane for SignalR scale-out (required for multiple instances)
- ⚠️ **Rate Limiting:** Protect API from abuse (pending)

**Scalability Targets:**

| Metric | Target | Current |
|--------|--------|---------|
| **Throughput** | 10,000+ events/min | ~1,000 events/min (single instance) |
| **Latency (p95)** | < 100ms | ~50ms (queries), ~20ms (writes) |
| **Concurrent Connections** | 10,000+ WebSocket | Limited by single instance |
| **Data Retention** | 12 months | Unlimited (no archival yet) |

---

## 🔒 **SECURITY ARCHITECTURE**

### **Security Layers**

```mermaid
graph TB
    Client[Client] --> HTTPS{HTTPS/TLS}
    HTTPS --> CORS[CORS Policy]
    CORS --> Auth[Future: Authentication]
    Auth --> API[Analytics API]

    API --> Validation[Input Validation]
    Validation --> Sanitization[SQL Injection Prevention]
    Sanitization --> Authorization[Future: Authorization]

    Authorization --> DB[(PostgreSQL)]

    subgraph "Secrets Management"
        Config[Configuration] --> EnvVars[Environment Variables]
        EnvVars --> Vault[Future: Azure Key Vault]
    end

    style HTTPS fill:#90EE90
    style Auth fill:#FFD700
    style Validation fill:#87CEEB
```

**Security Measures:**

- ✅ **HTTPS/TLS:** Enforced in production (handled by cloud platform)
- ✅ **Secrets Management:** Sensitive data in environment variables
- ✅ **SQL Injection Prevention:** EF Core parameterized queries
- ✅ **Input Validation:** FluentValidation on commands
- ✅ **CORS:** Configured for Dashboard UI domain
- ✅ **Correlation IDs:** Request tracking via `CorrelationMiddleware`
- ⚠️ **Authentication:** Planned (JWT tokens from Identity Service)
- ⚠️ **Authorization:** Planned (role-based access control)
- ⚠️ **Rate Limiting:** Recommended for API protection

**Planned Security Enhancements:**

1. **JWT Authentication:** Integrate with TC.Agro Identity Service
2. **Role-Based Access:** Farmers see only their sensors; admins see all
3. **API Keys:** For programmatic access
4. **Rate Limiting:** Throttle requests per IP/user
5. **Azure Key Vault:** Centralized secrets management

---

## 🔍 **OBSERVABILITY & MONITORING**

### **Telemetry Stack**

```mermaid
graph TB
    subgraph "Application Telemetry"
        App[Analytics Worker] --> Serilog[Serilog]
        Serilog --> Logs[Structured Logs]
        App --> Metrics[OpenTelemetry Metrics]
        App --> Traces[OpenTelemetry Traces]
    end

    subgraph "Collection & Export"
        Logs --> OTLP[OTLP Exporter]
        Metrics --> OTLP
        Traces --> OTLP
        OTLP --> AzMon[Azure Monitor]
        OTLP --> Prometheus[Prometheus]
    end

    subgraph "Visualization & Alerting"
        AzMon --> AppInsights[Application Insights]
        Prometheus --> Grafana[Grafana Dashboards]
        AppInsights --> Alerts[Azure Alerts]
    end

    style Serilog fill:#90EE90
    style OTLP fill:#FFD700
    style AppInsights fill:#87CEEB
```

**Telemetry Implementation:**

1. **Structured Logging (Serilog):**
   - Correlation IDs for request tracking
   - Contextual properties: `SensorId`, `AlertId`, `EventType`
   - Log levels: Debug, Info, Warning, Error, Fatal
   - Sinks: Console, Azure Monitor (Application Insights)

2. **Distributed Tracing (OpenTelemetry):**
   - Traces message processing: RabbitMQ → Handler → Database
   - Custom spans for business operations: `CreateAlertFromSensorData`
   - Activity IDs propagated across service boundaries

3. **Metrics (OpenTelemetry):**
   - **Counter:** `alerts_created_total` (by type, severity)
   - **Histogram:** `sensor_reading_processing_duration_ms`
   - **Gauge:** `active_signalr_connections`
   - **Counter:** `signalr_messages_sent_total`

4. **Health Checks:**
   - `/health` endpoint - Overall health
   - Checks: Database connectivity, RabbitMQ connectivity

**Key Metrics Tracked:**

| Metric | Description | Alert Threshold |
|--------|-------------|-----------------|
| `alerts_created_total` | Alerts created per minute | > 1000/min (anomaly) |
| `sensor_reading_processing_duration_ms` | Time to process reading | p95 > 500ms |
| `database_query_duration_ms` | Database query latency | p95 > 200ms |
| `rabbitmq_message_consumption_rate` | Messages consumed/sec | < 10/sec (degraded) |
| `active_signalr_connections` | Active WebSocket connections | N/A (monitoring only) |

**Configuration:**

Telemetry configuration in `appsettings.json`:
```json
{
  "Telemetry": {
    "ServiceName": "analytics-worker",
    "ServiceVersion": "1.0.0",
    "Exporters": {
      "AzureMonitor": {
        "Enabled": true,
        "ConnectionString": "env:APPLICATIONINSIGHTS_CONNECTION_STRING"
      },
      "OTLP": {
        "Enabled": false,
        "Endpoint": "http://otel-collector:4317"
      }
    }
  }
}
```

---

## 📚 **REFERENCES**

- **C4 Model:** https://c4model.com/
- **Clean Architecture:** Robert C. Martin - "Clean Architecture: A Craftsman's Guide to Software Structure and Design"
- **Domain-Driven Design:** Eric Evans - "Domain-Driven Design: Tackling Complexity in the Heart of Software"
- **CQRS:** Martin Fowler - https://martinfowler.com/bliki/CQRS.html
- **SignalR:** Microsoft Docs - https://learn.microsoft.com/aspnet/core/signalr/introduction
- **OpenTelemetry:** https://opentelemetry.io/
- **WolverineFx:** https://wolverine.netlify.app/

---

## 🎯 **HOW TO USE THIS DOCUMENT**

### **Rendering Diagrams:**

1. **On GitHub:** Mermaid diagrams render automatically in markdown preview
2. **VS Code:** Install "Markdown Preview Mermaid Support" extension
3. **Confluence:** Use Mermaid plugin
4. **Online Tools:** https://mermaid.live/

### **Updating Diagrams:**

When evolving the architecture:
1. Update corresponding diagrams at each level (Context → Container → Component)
2. Maintain consistency between levels
3. Document architectural decisions in ADRs (Architecture Decision Records)
4. Update deployment diagram when infrastructure changes
5. Keep domain model in sync with actual code

### **Diagram Conventions:**

- **Blue boxes:** Internal containers/components
- **Gray boxes:** External systems/services
- **Solid arrows:** Synchronous calls (HTTP, method calls)
- **Dashed arrows:** Asynchronous communication (events, messages)
- **Database symbols:** Data stores
- **Queue symbols:** Message brokers

---

## 📝 **DOCUMENT METADATA**

**Created by:** TC.Agro Development Team (with GitHub Copilot assistance)  
**Last Updated:** February 2025  
**Version:** 2.0  
**Format:** Markdown + Mermaid (C4 Model)  
**Target Framework:** .NET 10  
**Status:** ✅ Production-ready (with planned enhancements noted)

---

**Note:** This documentation reflects the current state of the Analytics Worker service as of February 2025. For code-level details, refer to inline documentation in the codebase.

