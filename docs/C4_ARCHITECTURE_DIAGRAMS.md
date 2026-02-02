# 📐 **C4 MODEL - ARCHITECTURE DIAGRAMS**
## **Analytics Worker - TC.Agro Solutions**

**Documentação Visual da Arquitetura usando C4 Model**

---

## 📚 **Sobre o C4 Model**

O **C4 Model** é uma abordagem para documentação de arquitetura de software criada por Simon Brown. Consiste em 4 níveis de abstração:

1. **Context** - Sistema e seus usuários/sistemas externos
2. **Container** - Aplicações, bancos de dados, serviços
3. **Component** - Componentes internos de cada container
4. **Code** - Classes e interfaces (geralmente não necessário)

**Referência:** https://c4model.com/

---

## 🌍 **NÍVEL 1: CONTEXT DIAGRAM**

### **Sistema no Contexto do Ecossistema TC.Agro**

```mermaid
C4Context
    title System Context Diagram - Analytics Worker

    Person(farmer, "Agricultor", "Monitora alertas de sua fazenda")
    Person(admin, "Administrador", "Gerencia sistema e configurações")
    
    System(analyticsWorker, "Analytics Worker", "Processa dados de sensores e gera alertas em tempo real")
    
    System_Ext(ingestService, "Sensor Ingest Service", "Coleta dados de sensores IoT e publica eventos")
    System_Ext(alertService, "Alert Service", "Notifica agricultores sobre alertas críticos")
    System_Ext(dashboardUI, "Dashboard UI", "Interface web para visualização de dados")
    
    SystemDb_Ext(supabase, "Supabase", "PostgreSQL gerenciado na nuvem")
    SystemQueue_Ext(rabbitmq, "RabbitMQ", "Message Broker para eventos")
    
    Rel(ingestService, analyticsWorker, "Publica eventos", "SensorIngestedIntegrationEvent via RabbitMQ")
    Rel(analyticsWorker, alertService, "Publica alertas", "Domain Events via RabbitMQ")
    Rel(dashboardUI, analyticsWorker, "Consulta alertas", "HTTP/JSON")
    Rel(farmer, dashboardUI, "Visualiza alertas", "HTTPS")
    Rel(admin, dashboardUI, "Gerencia sistema", "HTTPS")
    
    Rel(analyticsWorker, supabase, "Persiste eventos e read models", "PostgreSQL Protocol")
    Rel(analyticsWorker, rabbitmq, "Consome/Publica eventos", "AMQP")
    
    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="2")
```

**Descrição:**

- **Analytics Worker** é o sistema central de análise de dados de sensores
- Consome eventos do **Sensor Ingest Service** via RabbitMQ
- Processa dados aplicando regras de negócio (detecção de alertas)
- Publica domain events para **Alert Service** notificar agricultores
- Expõe API REST para **Dashboard UI** consultar alertas
- Persiste eventos (Event Sourcing) e read models no **Supabase**

---

## 📦 **NÍVEL 2: CONTAINER DIAGRAM**

### **Containers e Tecnologias do Analytics Worker**

```mermaid
C4Container
    title Container Diagram - Analytics Worker

    Person(farmer, "Agricultor", "Usuário final")
    System_Ext(ingestService, "Sensor Ingest Service", "Publica eventos de sensores")
    System_Ext(alertService, "Alert Service", "Recebe alertas")
    
    Container_Boundary(analyticsWorker, "Analytics Worker") {
        Container(api, "Analytics API", ".NET 10, FastEndpoints", "Expõe endpoints REST para consulta de alertas")
        Container(worker, "Message Handler", ".NET 10, WolverineFx", "Processa eventos de sensores e gera alertas")
        ContainerDb(eventStore, "Event Store", "Marten + PostgreSQL", "Armazena Domain Events (Event Sourcing)")
        ContainerDb(readDb, "Read Database", "EF Core + PostgreSQL", "Read Models otimizados para queries")
    }
    
    ContainerQueue(messageBroker, "Message Broker", "RabbitMQ", "Distribui eventos entre serviços")
    
    Rel(ingestService, messageBroker, "Publica", "SensorIngestedIntegrationEvent")
    Rel(messageBroker, worker, "Consome", "AMQP")
    
    Rel(worker, eventStore, "Salva eventos", "Marten API")
    Rel(worker, readDb, "Projeta read models", "EF Core")
    Rel(worker, messageBroker, "Publica domain events", "AMQP")
    
    Rel(messageBroker, alertService, "Entrega alertas", "AMQP")
    
    Rel(farmer, api, "Consulta alertas", "HTTPS/JSON")
    Rel(api, readDb, "Queries", "Dapper/EF Core")
    
    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
```

**Tecnologias por Container:**

| Container | Tecnologia | Propósito |
|-----------|------------|-----------|
| **Analytics API** | .NET 10 + FastEndpoints | Minimal APIs para consultas (CQRS Query Side) |
| **Message Handler** | WolverineFx + Marten | Processa comandos e eventos (CQRS Command Side) |
| **Event Store** | Marten + PostgreSQL | Event Sourcing - histórico completo de eventos |
| **Read Database** | EF Core + PostgreSQL | Read Models desnormalizados e otimizados |
| **Message Broker** | RabbitMQ | Comunicação assíncrona entre serviços |

---

## 🧩 **NÍVEL 3: COMPONENT DIAGRAM**

### **3.1 Analytics API (Query Side)**

```mermaid
C4Component
    title Component Diagram - Analytics API (CQRS Query Side)

    Container_Boundary(api, "Analytics API") {
        Component(endpoints, "Alert Endpoints", "FastEndpoints", "Expõe endpoints REST")
        Component(queryHandlers, "Query Handlers", "C#", "Executa queries otimizadas")
        Component(models, "Response Models", "C#", "DTOs de resposta")
    }
    
    ContainerDb(readDb, "Read Database", "PostgreSQL", "alerts table")
    Person(client, "Cliente", "Dashboard UI / Mobile App")
    
    Rel(client, endpoints, "GET /alerts/pending", "HTTPS")
    Rel(client, endpoints, "GET /alerts/history/{plotId}", "HTTPS")
    Rel(client, endpoints, "GET /alerts/status/{plotId}", "HTTPS")
    
    Rel(endpoints, queryHandlers, "Delega query", "Método")
    Rel(queryHandlers, readDb, "SELECT otimizado", "Dapper/EF Core")
    Rel(queryHandlers, models, "Mapeia para DTO", "")
    Rel(endpoints, client, "Retorna JSON", "HTTPS")
    
    UpdateLayoutConfig($c4ShapeInRow="3")
```

**Componentes - Query Side:**

1. **Alert Endpoints** (`AlertEndpoints.cs`)
   - `GetPendingAlertsEndpoint` - Lista alertas pendentes
   - `GetAlertHistoryEndpoint` - Histórico de alertas por talhão
   - `GetPlotStatusEndpoint` - Status geral do talhão

2. **Query Handlers** (`AlertQueries.cs`)
   - `GetPendingAlertsQueryHandler`
   - `GetAlertHistoryQueryHandler`
   - `GetPlotStatusQueryHandler`

3. **Response Models** (`AlertModels.cs`)
   - `AlertDto`
   - `PlotStatusDto`
   - `AlertHistoryDto`

---

### **3.2 Message Handler (Command Side)**

```mermaid
C4Component
    title Component Diagram - Message Handler (CQRS Command Side)

    Container_Boundary(worker, "Message Handler") {
        Component(handler, "SensorIngestedHandler", "WolverineFx", "Processa eventos de sensores")
        Component(aggregate, "SensorReadingAggregate", "Domain Model", "Aggregate Root com lógica de negócio")
        Component(events, "Domain Events", "C#", "HighTemperature, LowSoilMoisture, BatteryLow")
        Component(repository, "SensorReadingRepository", "Marten", "Persiste aggregates como eventos")
        Component(projection, "AlertProjectionHandler", "C#", "Projeta eventos para read model")
    }
    
    ContainerQueue(rabbitmq, "RabbitMQ", "Message Broker")
    ContainerDb(eventStore, "Event Store", "Marten")
    ContainerDb(readDb, "Read Database", "PostgreSQL")
    
    Rel(rabbitmq, handler, "Consome evento", "SensorIngestedIntegrationEvent")
    Rel(handler, aggregate, "Cria/Reconstrói", "Event Sourcing")
    Rel(aggregate, events, "Publica", "Domain Events")
    Rel(handler, repository, "Salva", "Event Stream")
    Rel(repository, eventStore, "Persiste eventos", "Marten API")
    
    Rel(events, projection, "Notifica", "In-Process")
    Rel(projection, readDb, "INSERT/UPDATE", "EF Core")
    
    Rel(events, rabbitmq, "Publica", "Integration Events")
    
    UpdateLayoutConfig($c4ShapeInRow="3")
```

**Componentes - Command Side:**

1. **SensorIngestedHandler** (`SensorIngestedInHandler.cs`)
   - Processa `SensorIngestedIntegrationEvent`
   - Carrega ou cria `SensorReadingAggregate`
   - Aplica regras de negócio
   - Salva eventos via repository

2. **SensorReadingAggregate** (`SensorReadingAggregate.cs`)
   - Aggregate Root (DDD)
   - Métodos: `DetectAlerts()`, `UpdateReading()`
   - Publica Domain Events quando condições críticas detectadas

3. **Domain Events**
   - `HighTemperatureDetectedDomainEvent`
   - `LowSoilMoistureDetectedDomainEvent`
   - `BatteryLowWarningDomainEvent`

4. **SensorReadingRepository** (`SensorReadingRepository.cs`)
   - Implementa Event Sourcing com Marten
   - Métodos: `GetByIdAsync()`, `AddAsync()`

5. **AlertProjectionHandler** (`AlertProjectionHandler.cs`)
   - Escuta Domain Events
   - Projeta para `alerts` table (read model)
   - Implementa CQRS eventual consistency

---

## 🏗️ **CLEAN ARCHITECTURE - LAYERS**

### **Camadas e Dependências**

```mermaid
graph TB
    subgraph "Presentation Layer"
        API[Analytics API<br/>FastEndpoints]
    end
    
    subgraph "Application Layer"
        Handlers[Message Handlers<br/>SensorIngestedHandler]
        QueryHandlers[Query Handlers<br/>AlertQueries]
        Ports[Ports/Interfaces<br/>IUnitOfWork]
    end
    
    subgraph "Domain Layer"
        Aggregates[Aggregates<br/>SensorReadingAggregate]
        Entities[Entities<br/>Alert]
        ValueObjects[Value Objects<br/>AlertThresholds]
        DomainEvents[Domain Events<br/>3 eventos]
        DomainPorts[Domain Ports<br/>ISensorReadingRepository]
    end
    
    subgraph "Infrastructure Layer"
        Repositories[Repositories<br/>Marten]
        Projections[Projections<br/>AlertProjectionHandler]
        Persistence[Persistence<br/>EF Core]
        Messaging[Messaging<br/>WolverineFx]
    end
    
    API --> Handlers
    API --> QueryHandlers
    Handlers --> Aggregates
    QueryHandlers --> Ports
    Handlers --> DomainPorts
    
    Aggregates --> DomainEvents
    Aggregates --> ValueObjects
    Aggregates --> Entities
    
    Repositories --> DomainPorts
    Projections --> DomainEvents
    Persistence --> Ports
    Messaging --> Handlers
    
    style Domain Layer fill:#90EE90
    style API fill:#87CEEB
    style Handlers fill:#FFD700
    style Repositories fill:#FFA07A
```

**Dependency Rule:**
- ✅ Domain não depende de nada
- ✅ Application depende apenas de Domain
- ✅ Infrastructure implementa interfaces do Domain
- ✅ Presentation depende de Application

---

## 🔄 **EVENT FLOW - SEQUENCE DIAGRAM**

### **Fluxo Completo: Sensor → Alerta → Notificação**

```mermaid
sequenceDiagram
    participant Sensor as 🌡️ Sensor IoT
    participant Ingest as Sensor Ingest
    participant RabbitMQ as 🐰 RabbitMQ
    participant Worker as Analytics Worker
    participant Marten as 📚 Event Store
    participant Postgres as 🗄️ Read DB
    participant Alert as 🔔 Alert Service
    participant UI as 💻 Dashboard UI
    
    Sensor->>Ingest: Envia leitura<br/>(temp: 45°C)
    Ingest->>RabbitMQ: Publica SensorIngestedEvent
    
    RabbitMQ->>Worker: Consome evento
    
    rect rgb(200, 220, 250)
        Note over Worker: CQRS Command Side
        Worker->>Marten: Carrega Event Stream
        Marten-->>Worker: Reconstrói Aggregate
        Worker->>Worker: DetectAlerts()<br/>🔥 Temperatura > 35°C
        Worker->>Worker: Publica HighTemperatureEvent
        Worker->>Marten: Salva Domain Event
    end
    
    rect rgb(250, 220, 200)
        Note over Worker: CQRS Query Side (Projection)
        Worker->>Postgres: INSERT INTO alerts<br/>(type: HighTemperature)
    end
    
    Worker->>RabbitMQ: Publica Integration Event
    RabbitMQ->>Alert: Entrega alerta
    Alert->>Sensor: Envia SMS/Push<br/>"⚠️ Temp. crítica!"
    
    UI->>Worker: GET /alerts/pending
    Worker->>Postgres: SELECT * FROM alerts<br/>WHERE status = 'Pending'
    Postgres-->>Worker: Resultado
    Worker-->>UI: JSON Response
```

**Fluxo Detalhado:**

1. **Sensor IoT** envia leitura (temp: 45°C)
2. **Sensor Ingest** valida e publica `SensorIngestedIntegrationEvent`
3. **RabbitMQ** roteia evento para **Analytics Worker**
4. **Worker (Command Side):**
   - Carrega Event Stream do Marten
   - Reconstrói `SensorReadingAggregate`
   - Executa `DetectAlerts()` → detecta temperatura > 35°C
   - Publica `HighTemperatureDetectedDomainEvent`
   - Salva evento no Event Store (Marten)
5. **Worker (Query Side - Projection):**
   - `AlertProjectionHandler` escuta Domain Event
   - Projeta para `alerts` table no PostgreSQL
6. **Worker** publica Integration Event para **Alert Service**
7. **Alert Service** notifica agricultor (SMS/Push)
8. **Dashboard UI** consulta alertas via API REST

---

## 🗄️ **DATA FLOW - CQRS SEPARATION**

### **Separação Command/Query**

```mermaid
graph LR
    subgraph "Command Side (Write)"
        CMD[Commands] --> AGG[Aggregate<br/>SensorReading]
        AGG --> EVT[Domain Events]
        EVT --> ES[Event Store<br/>Marten]
    end
    
    subgraph "Projection (Async)"
        EVT --> PROJ[Projection Handler]
        PROJ --> RM[Read Model<br/>alerts table]
    end
    
    subgraph "Query Side (Read)"
        API[API Endpoints] --> QH[Query Handlers]
        QH --> RM
        RM --> API
    end
    
    style CMD fill:#FFB6C1
    style AGG fill:#90EE90
    style EVT fill:#FFD700
    style ES fill:#87CEEB
    style PROJ fill:#FFA07A
    style RM fill:#DDA0DD
    style API fill:#F0E68C
```

**Vantagens CQRS:**

| Aspecto | Command Side | Query Side |
|---------|--------------|------------|
| **Modelo** | Aggregate Root (rico) | Read Model (simples) |
| **Persistência** | Event Store (Marten) | Relational (PostgreSQL) |
| **Otimização** | Write-optimized | Read-optimized |
| **Consistência** | Strong (transacional) | Eventual |
| **Escalabilidade** | Vertical | Horizontal (replicas) |

---

## 📊 **DEPLOYMENT DIAGRAM**

### **Infraestrutura Cloud (Supabase + Railway)**

```mermaid
C4Deployment
    title Deployment Diagram - Production Environment

    Deployment_Node(cloudProvider, "Cloud Platform", "Railway / Render") {
        Deployment_Node(appServer, "Application Server", "Container") {
            Container(api, "Analytics API", ".NET 10")
            Container(worker, "Message Handler", ".NET 10")
        }
    }
    
    Deployment_Node(supabase, "Supabase", "Managed PostgreSQL") {
        ContainerDb(postgres, "PostgreSQL 15", "Database")
    }
    
    Deployment_Node(cloudamqp, "CloudAMQP", "Managed RabbitMQ") {
        ContainerQueue(rabbitmq, "RabbitMQ", "Message Broker")
    }
    
    Deployment_Node(monitoring, "Observability", "Cloud Services") {
        Container(loki, "Grafana Loki", "Logs")
        Container(prometheus, "Prometheus", "Metrics")
        Container(jaeger, "Jaeger", "Tracing")
    }
    
    Rel(api, postgres, "Queries", "PostgreSQL Protocol")
    Rel(worker, postgres, "Events + Projections", "PostgreSQL Protocol")
    Rel(worker, rabbitmq, "Pub/Sub", "AMQP")
    
    Rel(api, loki, "Logs", "HTTP")
    Rel(worker, prometheus, "Metrics", "HTTP")
    Rel(api, jaeger, "Traces", "HTTP")
    
    UpdateLayoutConfig($c4ShapeInRow="2")
```

**Infraestrutura:**

- **Application:** Railway/Render (containers)
- **Database:** Supabase (PostgreSQL gerenciado)
- **Message Broker:** CloudAMQP (RabbitMQ gerenciado)
- **Observability:** Grafana Cloud Stack

---

## 🎨 **DOMAIN MODEL - CLASS DIAGRAM**

### **Principais Elementos do Domain**

```mermaid
classDiagram
    class SensorReadingAggregate {
        +Guid Id
        +string SensorId
        +Guid PlotId
        +DateTime Timestamp
        +double Temperature
        +double Humidity
        +double SoilMoisture
        +double Rainfall
        +double BatteryLevel
        +AlertThresholds Thresholds
        +List~IDomainEvent~ DomainEvents
        +void DetectAlerts()
        +void UpdateReading()
        -void RaiseHighTemperatureAlert()
        -void RaiseLowSoilMoistureAlert()
        -void RaiseBatteryLowWarning()
    }
    
    class AlertThresholds {
        +double MaxTemperature
        +double MinSoilMoisture
        +double MinBatteryLevel
        +bool IsTemperatureCritical()
        +bool IsSoilMoistureLow()
        +bool IsBatteryLow()
    }
    
    class Alert {
        +Guid Id
        +Guid SensorReadingId
        +string SensorId
        +Guid PlotId
        +string AlertType
        +string Message
        +string Status
        +string Severity
        +double? Value
        +double? Threshold
        +DateTime CreatedAt
        +DateTime? AcknowledgedAt
        +DateTime? ResolvedAt
        +string Metadata
    }
    
    class HighTemperatureDetectedDomainEvent {
        +Guid AggregateId
        +string SensorId
        +Guid PlotId
        +DateTime Timestamp
        +double Temperature
        +double Humidity
        +double SoilMoisture
        +double Rainfall
        +double BatteryLevel
        +DateTimeOffset OccurredOn
    }
    
    class LowSoilMoistureDetectedDomainEvent {
        +Guid AggregateId
        +string SensorId
        +Guid PlotId
        +DateTime Timestamp
        +double Temperature
        +double Humidity
        +double SoilMoisture
        +double Rainfall
        +double BatteryLevel
        +DateTimeOffset OccurredOn
    }
    
    class BatteryLowWarningDomainEvent {
        +Guid AggregateId
        +string SensorId
        +Guid PlotId
        +double BatteryLevel
        +double Threshold
        +DateTimeOffset OccurredOn
    }
    
    SensorReadingAggregate --> AlertThresholds : uses
    SensorReadingAggregate ..> HighTemperatureDetectedDomainEvent : publishes
    SensorReadingAggregate ..> LowSoilMoistureDetectedDomainEvent : publishes
    SensorReadingAggregate ..> BatteryLowWarningDomainEvent : publishes
    
    HighTemperatureDetectedDomainEvent ..> Alert : projects to
    LowSoilMoistureDetectedDomainEvent ..> Alert : projects to
    BatteryLowWarningDomainEvent ..> Alert : projects to
```

**Padrões DDD Implementados:**

- ✅ **Aggregate Root:** `SensorReadingAggregate`
- ✅ **Entity:** `Alert` (Read Model)
- ✅ **Value Object:** `AlertThresholds`
- ✅ **Domain Events:** 3 eventos ricos
- ✅ **Factory Methods:** No Aggregate
- ✅ **Repository Pattern:** `ISensorReadingRepository`

---

## 📈 **PERFORMANCE & SCALABILITY**

### **Estratégias de Otimização**

```mermaid
graph TB
    subgraph "Read Side Optimization"
        API[API Request] --> Cache{FusionCache}
        Cache -->|Hit| Response[Fast Response]
        Cache -->|Miss| DB[PostgreSQL]
        DB --> Index[8 Indexes]
        Index --> Response
    end
    
    subgraph "Write Side Optimization"
        Event[Domain Event] --> Batch{Batch Writer}
        Batch --> EventStore[Event Store]
        Batch --> Projection[Async Projection]
    end
    
    subgraph "Horizontal Scaling"
        LB[Load Balancer] --> API1[API Instance 1]
        LB --> API2[API Instance 2]
        LB --> API3[API Instance N]
    end
    
    style Cache fill:#90EE90
    style Index fill:#FFD700
    style Batch fill:#87CEEB
```

**Otimizações Implementadas:**

1. **Caching:** FusionCache para queries frequentes
2. **Indexação:** 8 índices no PostgreSQL
3. **Read Replicas:** Suporte a replicas read-only
4. **Connection Pooling:** Npgsql connection pooling
5. **Async Processing:** Projections assíncronas
6. **Batch Operations:** Marten batch append

---

## 🔒 **SECURITY ARCHITECTURE**

### **Camadas de Segurança**

```mermaid
graph TB
    Client[Cliente] --> HTTPS{HTTPS/TLS}
    HTTPS --> Auth[Authentication]
    Auth --> CORS[CORS Policy]
    CORS --> RateLimit[Rate Limiting]
    RateLimit --> API[Analytics API]
    
    API --> Validation[Input Validation]
    Validation --> Sanitization[SQL Injection Prevention]
    Sanitization --> Encryption[Data Encryption]
    
    Encryption --> DB[(Encrypted DB)]
    
    subgraph "Secrets Management"
        Config[appsettings.json] --> Vault[Secret Vault]
        Vault --> EnvVars[Environment Variables]
    end
    
    style HTTPS fill:#90EE90
    style Auth fill:#FFD700
    style Encryption fill:#87CEEB
```

**Medidas de Segurança:**

- ✅ HTTPS/TLS obrigatório
- ✅ Secrets em variáveis de ambiente
- ✅ SQL Injection prevention (EF Core/Dapper)
- ✅ Input validation (FluentValidation)
- ✅ CORS configurado
- ⚠️ Rate Limiting recomendado
- ⚠️ API Keys/JWT recomendado

---

## 📚 **REFERENCIAS**

- **C4 Model:** https://c4model.com/
- **Clean Architecture:** Robert C. Martin
- **Domain-Driven Design:** Eric Evans
- **CQRS:** Martin Fowler
- **Event Sourcing:** Greg Young

---

## 🎯 **COMO USAR ESTE DOCUMENTO**

### **Renderização dos Diagramas:**

1. **No GitHub:** Diagramas Mermaid são renderizados automaticamente
2. **VS Code:** Instale extensão "Markdown Preview Mermaid Support"
3. **Confluence:** Use plugin Mermaid
4. **Ferramentas Online:** https://mermaid.live/

### **Atualização dos Diagramas:**

Ao evoluir a arquitetura:
1. Atualize os diagramas correspondentes
2. Mantenha consistência entre níveis (Context → Container → Component)
3. Documente decisões arquiteturais em ADRs

---

**Criado por:** GitHub Copilot AI  
**Data:** 01/02/2025  
**Versão:** 1.0  
**Formato:** Mermaid (C4 Model)
