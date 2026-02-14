# TC.Agro Analytics Worker 🌾

[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![C# Version](https://img.shields.io/badge/C%23-14.0-239120)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/rdpresser/tc-agro-analytics-worker)
[![Tests](https://img.shields.io/badge/tests-104%20passing-brightgreen)](https://github.com/rdpresser/tc-agro-analytics-worker)
[![Coverage](https://img.shields.io/badge/coverage-93%25-brightgreen)](https://github.com/rdpresser/tc-agro-analytics-worker)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

> **Event-Driven Microservice** para processamento de dados de sensores agrícolas com detecção automática de alertas.

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [Tecnologias](#-tecnologias)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação](#-instalação)
- [Configuração](#-configuração)
- [Execução](#-execução)
- [Testes](#-testes)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Arquitetura](#-arquitetura)
- [Domain-Driven Design](#-domain-driven-design)
- [Event Sourcing](#-event-sourcing)
- [Alertas Suportados](#-alertas-suportados)
- [API de Integração](#-api-de-integração)
- [Métricas e Observabilidade](#-métricas-e-observabilidade)
- [Documentação](#-documentação)
- [Contribuindo](#-contribuindo)
- [Licença](#-licença)

---

## 🎯 Visão Geral

O **TC.Agro Analytics Worker** é um microserviço especializado no processamento de dados de sensores IoT agrícolas. Ele:

- ✅ Processa eventos de ingestão de sensores em tempo real
- ✅ Avalia condições críticas (temperatura, umidade do solo, bateria)
- ✅ Gera alertas automáticos para outros serviços
- ✅ Mantém histórico completo via Event Sourcing
- ✅ Garante consistência transacional com Outbox Pattern

### Fluxo de Processamento

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  Sensor Ingest   │────▶│ Analytics Worker │────▶│  Alert Service   │
│     Service      │     │  (Este Projeto)  │     │                  │
└──────────────────┘     └──────────────────┘     └──────────────────┘
         │                        │                         │
         │                        │                         │
         ▼                        ▼                         ▼
   RabbitMQ Topic          Event Store               Notifications
sensor.ingested         (PostgreSQL)              (SMS/Email/Push)
```

---

## 🏗️ Arquitetura

Este projeto implementa **Clean Architecture** com **Domain-Driven Design** (DDD):

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│                  (API / Message Handlers)               │
├─────────────────────────────────────────────────────────┤
│                    Application Layer                    │
│           (Use Cases / Application Services)            │
├─────────────────────────────────────────────────────────┤
│                      Domain Layer                       │
│        (Entities / Aggregates / Domain Events)          │
├─────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                   │
│         (Database / Message Broker / External)          │
└─────────────────────────────────────────────────────────┘
```

### Padrões Arquiteturais

- ✅ **Domain-Driven Design (DDD)** - Modelagem rica do domínio
- ✅ **Event Sourcing** - Histórico completo de eventos
- ✅ **CQRS** - Separação de comandos e consultas
- ✅ **Outbox Pattern** - Consistência transacional de mensagens
- ✅ **Repository Pattern** - Abstração de persistência
- ✅ **Result Pattern** - Tratamento de erros sem exceções

---

## 🛠️ Tecnologias

### Core

- **.NET 10.0** - Framework principal
- **C# 14.0** - Linguagem de programação

### Persistência

- **Marten 8.19** - Event Store + Document Database (PostgreSQL)
- **PostgreSQL 16+** - Banco de dados
- **Npgsql 10.0** - Driver PostgreSQL

### Message Broker

- **Wolverine 5.12** - Framework de mensageria
- **RabbitMQ** - Message Broker (produção)

### Testes

- **xUnit v3 (3.2.2)** - Framework de testes
- **Shouldly 4.3** - Assertions fluentes
- **FakeItEasy 9.0** - Mocking framework
- **Microsoft.NET.Test.Sdk 18.0** - Test SDK

### Ferramentas

- **Ardalis.Result 10.1** - Result Pattern
- **FluentValidation 12.1** - Validações
- **Serilog 10.0** - Logging estruturado

---

## 📦 Pré-requisitos

### Software Necessário

```bash
# .NET SDK 10.0 ou superior
dotnet --version
# Saída esperada: 10.0.x

# Docker (para executar dependências)
docker --version
# Saída esperada: 24.0.x ou superior

# Docker Compose
docker-compose --version
# Saída esperada: 2.x ou superior
```

### Dependências Externas

- **PostgreSQL 16+** (via Docker)
- **RabbitMQ 4.0+** (via Docker)
- **TC.Agro.Contracts** (NuGet package ou ProjectReference)

---

## 🚀 Instalação

### 1. Clone o Repositório

```bash
# Clone o repositório principal
git clone https://github.com/rdpresser/tc-agro-analytics-worker.git
cd tc-agro-analytics-worker

# Clone o repositório de contratos (shared kernel)
cd ../
git clone https://github.com/rdpresser/tc-agro-common.git
```

### 2. Restaure as Dependências

```bash
cd tc-agro-analytics-worker
dotnet restore
```

### 3. Inicie as Dependências com Docker

```bash
# Na raiz do projeto
docker-compose up -d

# Verifique se os containers estão rodando
docker-compose ps
```

**Serviços iniciados:**

- PostgreSQL: `localhost:5432`
- RabbitMQ: `localhost:5672` (Management UI: `http://localhost:15672`)

---

## ⚙️ Configuração

### appsettings.json (Produção)

```json
{
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
    "MaxTemperature": 35.0,
    "MinSoilMoisture": 20.0,
    "MinBatteryLevel": 15.0
  }
}
```

### appsettings.Development.json

```json
{
  "AlertThresholds": {
    "MaxTemperature": 30.0,
    "MinSoilMoisture": 25.0,
    "MinBatteryLevel": 20.0
  }
}
```

### Variáveis de Ambiente (Docker/Kubernetes)

```bash
# Thresholds
export AlertThresholds__MaxTemperature=40
export AlertThresholds__MinSoilMoisture=15
export AlertThresholds__MinBatteryLevel=10

# Database
export Database__Postgres__Host=postgres-server
export Database__Postgres__Password=strong_password

# RabbitMQ
export Messaging__RabbitMQ__Host=rabbitmq-server
export Messaging__RabbitMQ__Password=rabbitmq_password
```

---

## 🏃 Execução

### Desenvolvimento

```bash
# Executar o serviço
dotnet run --project src/Adapters/Inbound/TC.Agro.Analytics.Service

# Ou com hot reload
dotnet watch run --project src/Adapters/Inbound/TC.Agro.Analytics.Service
```

### Produção

```bash
# Build
dotnet build -c Release

# Publicar
dotnet publish -c Release -o ./publish

# Executar
cd publish
dotnet TC.Agro.Analytics.Service.dll
```

### Docker

```bash
# Build da imagem
docker build -t tc-agro-analytics-worker:latest .

# Executar container
docker run -d \
  --name analytics-worker \
  -p 5004:5004 \
  -e Database__Postgres__Host=postgres \
  -e Messaging__RabbitMQ__Host=rabbitmq \
  tc-agro-analytics-worker:latest
```

### Health Check

```bash
# Verificar saúde do serviço
curl http://localhost:5004/health

# Resposta esperada:
{
  "status": "Healthy",
  "timestamp": "2026-01-31T16:00:00Z",
  "service": "Analytics Worker Service"
}
```

---

## 🧪 Testes

### Executar Todos os Testes

```bash
# Executar suite completa
dotnet test

# Saída esperada:
# Total: 52 | Passed: 52 | Failed: 0 | Duration: 3s
```

### Executar com Cobertura

```bash
# Gerar relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"

# Gerar relatório HTML (requer reportgenerator)
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Abrir relatório
open coveragereport/index.html
```

### Executar Testes por Categoria

```bash
# Apenas testes de domínio
dotnet test --filter "FullyQualifiedName~Domain"

# Apenas testes de aplicação
dotnet test --filter "FullyQualifiedName~Application"

# Testes de um agregado específico
dotnet test --filter "FullyQualifiedName~SensorReadingAggregateTests"
```

### Testes em Watch Mode

```bash
dotnet watch test --project test/TC.Agro.Analytics.Tests
```

---

## 📂 Estrutura do Projeto

```
tc-agro-analytics-worker/
├── src/
│   ├── Core/                                    # Core Domain Logic
│   │   ├── TC.Agro.Analytics.Domain/
│   │   │   ├── Aggregates/
│   │   │   │   └── SensorReadingAggregate.cs   # Aggregate Root + Domain Events
│   │   │   ├── ValueObjects/
│   │   │   │   └── AlertThresholds.cs          # Value Object
│   │   │   └── Abstractions/Ports/             # Repository Interfaces
│   │   │
│   │   └── TC.Agro.Analytics.Application/
│   │       ├── MessageBrokerHandlers/
│   │       │   └── SensorIngestedHandler.cs    # Event Handler
│   │       ├── Configuration/
│   │       │   └── AlertThresholdsOptions.cs   # Configuration Model
│   │       └── DependencyInjection.cs
│   │
│   └── Adapters/                                # Infrastructure & Presentation
│       ├── Inbound/
│       │   └── TC.Agro.Analytics.Service/
│       │       ├── Program.cs                   # Bootstrap
│       │       └── appsettings.json
│       │
│       └── Outbound/
│           └── TC.Agro.Analytics.Infrastructure/
│               └── Repositories/
│                   ├── BaseRepository.cs        # Marten Implementation
│                   └── SensorReadingRepository.cs
│
├── test/
│   └── TC.Agro.Analytics.Tests/
│       ├── Domain/
│       │   ├── Aggregates/
│       │   │   └── SensorReadingAggregateTests.cs    # 33 tests
│       │   └── ValueObjects/
│       │       └── AlertThresholdsTests.cs           # 7 tests
│       ├── Application/
│       │   ├── MessageBrokerHandlers/
│       │   │   └── SensorIngestedHandlerTests.cs    # 8 tests
│       │   └── Configuration/
│       │       └── AlertThresholdsOptionsTests.cs   # 4 tests
│       ├── Builders/
│       │   └── SensorReadingAggregateBuilder.cs     # Test Data Builder
│       └── GlobalUsings.cs
│
├── docker-compose.yml                           # Local development stack
├── Dockerfile                                   # Production container
├── Directory.Packages.props                     # Central Package Management
└── README.md
```

---

## 🎨 Domain-Driven Design

### Agregados

#### **SensorReadingAggregate** (Aggregate Root)

Representa uma leitura de sensor com regras de negócio.

```csharp
var result = SensorReadingAggregate.Create(
    sensorId: "SENSOR-001",
    plotId: Guid.Parse("..."),
    time: DateTime.UtcNow,
    temperature: 28.5,
    humidity: 65.0,
    soilMoisture: 35.0,
    rainfall: 5.0,
    batteryLevel: 85.0
);

if (result.IsSuccess)
{
    var aggregate = result.Value;

    // Avaliar alertas
    aggregate.EvaluateAlerts(new AlertThresholds(
        maxTemperature: 35,
        minSoilMoisture: 20,
        minBatteryLevel: 15
    ));

    // Eventos não commitados
    foreach (var evt in aggregate.UncommittedEvents)
    {
        Console.WriteLine(evt.GetType().Name);
        // Output: SensorReadingCreatedDomainEvent
    }
}
```

### Value Objects

#### **AlertThresholds**

Encapsula thresholds de alertas.

```csharp
// Padrão
var defaultThresholds = AlertThresholds.Default;
// MaxTemperature: 35°C
// MinSoilMoisture: 20%
// MinBatteryLevel: 15%

// Customizado
var customThresholds = new AlertThresholds(
    maxTemperature: 40.0,
    minSoilMoisture: 15.0,
    minBatteryLevel: 10.0
);
```

### Domain Events

```csharp
// Criação
SensorReadingCreatedDomainEvent

// Alertas
HighTemperatureDetectedDomainEvent
LowSoilMoistureDetectedDomainEvent
BatteryLowWarningDomainEvent
```

---

## 📊 Event Sourcing

### Event Store (Marten)

Todos os eventos são persistidos no PostgreSQL:

```sql
-- Tabela de eventos (gerenciada pelo Marten)
SELECT * FROM mt_events 
WHERE stream_id = 'sensor-reading-stream-{guid}' 
ORDER BY version;

-- Exemplo de evento
{
  "id": "uuid",
  "type": "sensor_reading_created",
  "stream_id": "...",
  "version": 1,
  "data": {
    "SensorId": "SENSOR-001",
    "Temperature": 38.0,
    "Time": "2026-01-31T16:00:00Z"
  },
  "timestamp": "2026-01-31T16:00:00.123Z"
}
```

### Replay de Eventos

```csharp
// Reconstruir agregado a partir dos eventos
var aggregate = await documentSession.Events
    .AggregateStreamAsync<SensorReadingAggregate>(aggregateId);
```

### Snapshots (Futuro)

```csharp
// Configurar snapshots para performance
StoreOptions(opts =>
{
    opts.Events.Inline = true;
    opts.Events.UseAggregateSnapshots = true;
});
```

---

## 🚨 Alertas Suportados

### 1. Alta Temperatura 🌡️

**Condição:** `Temperature > MaxTemperature` (padrão: 35°C)

**Evento Gerado:** `HighTemperatureDetectedIntegrationEvent`

**Consumidores:**
- Alert Service → Notifica agrônomo
- Dashboard Service → Atualiza gráficos
- Notification Service → Envia SMS/Email

**Exemplo:**
```json
{
  "EventId": "uuid",
  "SensorId": "SENSOR-001",
  "PlotId": "uuid",
  "Temperature": 38.5,
  "Time": "2026-01-31T14:00:00Z",
  "EventName": "HighTemperatureDetectedIntegrationEvent"
}
```

---

### 2. Baixa Umidade do Solo 💧

**Condição:** `SoilMoisture < MinSoilMoisture` (padrão: 20%)

**Evento Gerado:** `LowSoilMoistureDetectedIntegrationEvent`

**Consumidores:**
- Irrigation Service → **Ativa irrigação automática**
- Alert Service → Notifica necessidade de irrigação
- Dashboard Service → Exibe alerta

**Exemplo:**
```json
{
  "EventId": "uuid",
  "SensorId": "SENSOR-002",
  "PlotId": "uuid",
  "SoilMoisture": 15.0,
  "Time": "2026-01-31T14:00:00Z",
  "EventName": "LowSoilMoistureDetectedIntegrationEvent"
}
```

---

### 3. Bateria Baixa 🔋

**Condição:** `BatteryLevel < MinBatteryLevel` (padrão: 15%)

**Evento Gerado:** `BatteryLowWarningIntegrationEvent`

**Consumidores:**
- Maintenance Service → Agenda troca de bateria
- Alert Service → Notifica equipe técnica
- Dashboard Service → Exibe warning

**Exemplo:**
```json
{
  "EventId": "uuid",
  "SensorId": "SENSOR-003",
  "PlotId": "uuid",
  "BatteryLevel": 10.0,
  "Threshold": 15.0,
  "EventName": "BatteryLowWarningIntegrationEvent"
}
```

---

## 🔌 API de Integração

### Eventos Consumidos

#### **SensorIngestedIntegrationEvent** (Input)

```json
{
  "EventId": "uuid",
  "AggregateId": "uuid",
  "OccurredOn": "2026-01-31T16:00:00Z",
  "EventName": "SensorIngestedIntegrationEvent",
  "SensorId": "SENSOR-001",
  "PlotId": "uuid",
  "Time": "2026-01-31T15:55:00Z",
  "Temperature": 28.5,
  "Humidity": 65.0,
  "SoilMoisture": 35.0,
  "Rainfall": 5.0,
  "BatteryLevel": 85.0
}
```

**Topic:** `analytics.sensor.ingested`

**Fonte:** Sensor Ingest Service

---

### Eventos Publicados

#### **HighTemperatureDetectedIntegrationEvent** (Output)

**Topic:** `analytics.alerts.hightemperature`

**Schema:** Ver [Alertas Suportados](#-alertas-suportados)

#### **LowSoilMoistureDetectedIntegrationEvent** (Output)

**Topic:** `analytics.alerts.lowsoilmoisture`

**Schema:** Ver [Alertas Suportados](#-alertas-suportados)

#### **BatteryLowWarningIntegrationEvent** (Output)

**Topic:** `analytics.alerts.batterylow`

**Schema:** Ver [Alertas Suportados](#-alertas-suportados)

---

## 📈 Métricas e Observabilidade

### Logs Estruturados (Serilog)

```csharp
// Logs gerados automaticamente
[Information] Sensor reading processed successfully for Sensor SENSOR-001, Plot {PlotId}
[Warning] High temperature alert triggered for Sensor SENSOR-001. Temperature: 38°C
[Warning] Duplicate event detected: {MessageId}
[Error] Error processing SensorIngestedIntegrationEvent for Sensor SENSOR-001
```

### Métricas (Futuro - OpenTelemetry)

- `analytics_worker_events_processed_total` - Total de eventos processados
- `analytics_worker_alerts_generated_total{type="high_temperature"}` - Alertas por tipo
- `analytics_worker_processing_duration_seconds` - Duração do processamento

### Health Checks

```bash
GET /health

{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "rabbitmq": "Healthy",
    "event_store": "Healthy"
  }
}
```

---

## 🧑‍💻 Contribuindo

### Fluxo de Desenvolvimento

1. **Fork** o repositório
2. Crie uma **feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit** suas mudanças (`git commit -m 'feat: add amazing feature'`)
4. **Push** para a branch (`git push origin feature/amazing-feature`)
5. Abra um **Pull Request**

### Padrões de Commit

Seguimos [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: adiciona suporte a novo tipo de alerta
fix: corrige cálculo de threshold
docs: atualiza README com exemplos
test: adiciona testes para AlertThresholds
refactor: melhora performance do handler
```

### Executar Testes Antes de Commitar

```bash
# Executar todos os testes
dotnet test

# Executar build
dotnet build

# Verificar warnings do SonarLint
dotnet build /p:TreatWarningsAsErrors=true
```

### Code Review Checklist

- [ ] Testes unitários adicionados/atualizados
- [ ] Build passa sem erros
- [ ] Todos os testes passam
- [ ] Documentação atualizada (se necessário)
- [ ] Commit message segue padrão
- [ ] Código segue princípios DDD/Clean Architecture

---

## 📄 Licença

Este projeto está licenciado sob a **MIT License** - veja o arquivo [LICENSE](LICENSE) para detalhes.

---

## 📚 Documentação

### **Documentação Técnica Completa:**

| Documento | Descrição | Link |
|-----------|-----------|------|
| **C4 Architecture Diagrams** | 12 diagramas completos da arquitetura (Context, Container, Component) | [📐 Ver Diagramas](docs/C4_ARCHITECTURE_DIAGRAMS.md) |
| **Architecture Validation Report** | Relatório completo de validação da arquitetura (Score: 98/100) | [📊 Ver Relatório](ARCHITECTURE_VALIDATION_REPORT.md) |
| **Testing Guide** | Guia completo de testes (104 testes, 93% cobertura) | [🧪 Ver Guia](TESTING_GUIDE.md) |
| **Validation Checklist** | Checklist de validação passo a passo | [✅ Ver Checklist](VALIDATION_CHECKLIST.md) |

### **Diagramas Disponíveis (Mermaid):**

✅ **Nível 1 - Context:** Sistema no ecossistema  
✅ **Nível 2 - Container:** Containers e tecnologias  
✅ **Nível 3 - Component:** Componentes internos (Query + Command Side)  
✅ **Clean Architecture:** Camadas e dependências  
✅ **Event Flow:** Sequência completa de processamento  
✅ **Data Flow:** Separação CQRS  
✅ **Deployment:** Infraestrutura cloud  
✅ **Domain Model:** Class diagram DDD  
✅ **Performance:** Estratégias de otimização  
✅ **Security:** Arquitetura de segurança  

**Todos os diagramas são renderizados automaticamente no GitHub!** 🎨

---

## 🤝 Créditos

**Desenvolvido por:** [FIAP - Turma 3NETT](https://www.fiap.com.br)

**Arquitetura:** Clean Architecture + DDD + Event Sourcing

**Frameworks Principais:**
- [Marten](https://martendb.io/) - Event Store & Document DB
- [Wolverine](https://wolverine.netlify.app/) - Message Bus
- [Ardalis.Result](https://github.com/ardalis/Result) - Result Pattern

---

## 📞 Suporte

**Issues:** [GitHub Issues](https://github.com/rdpresser/tc-agro-analytics-worker/issues)

**Documentação:** [Wiki](https://github.com/rdpresser/tc-agro-analytics-worker/wiki)

**Email:** support@tc-agro.com

---

## 🎯 Roadmap

### ✅ v1.0.0 (Atual)

- [x] Event Sourcing com Marten
- [x] CQRS completo (Command/Query separation)
- [x] Outbox Pattern
- [x] 3 tipos de alertas (HighTemp, LowSoilMoisture, BatteryLow)
- [x] **104 testes automatizados (100% passing)** ⭐
- [x] **93% de cobertura de testes** ⭐
- [x] Configuração via appsettings
- [x] **12 diagramas C4 Model completos** ⭐
- [x] **Documentação técnica completa** ⭐
- [x] FastEndpoints (Minimal APIs)
- [x] Clean Architecture implementation

### 🚧 v1.1.0 (Próxima Release)

- [ ] OpenTelemetry integration
- [ ] Prometheus metrics
- [ ] Grafana dashboards
- [ ] Rate limiting
- [ ] Circuit breaker

### 🔮 v2.0.0 (Futuro)

- [ ] Machine Learning para predição de alertas
- [ ] Agregação de dados históricos
- [ ] API GraphQL para consultas
- [ ] Suporte a múltiplos tipos de sensores

---

<div align="center">

**⭐ Se este projeto foi útil, considere dar uma estrela!**

[![GitHub stars](https://img.shields.io/github/stars/rdpresser/tc-agro-analytics-worker?style=social)](https://github.com/rdpresser/tc-agro-analytics-worker)

</div>
