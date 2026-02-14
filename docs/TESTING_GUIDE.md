# 🧪 Guia de Teste - Analytics Worker API

## 📋 Pré-requisitos

1. ✅ Banco Supabase configurado
2. ✅ Migration aplicada (`dotnet ef database update`)
3. ✅ Dados de teste inseridos na tabela `alerts`

---

## 🚀 1. EXECUTAR O PROJETO

### Opção A: Visual Studio / Rider
```
1. Abrir o projeto TC.Agro.Analytics.Service
2. Pressionar F5 ou clicar em "Run"
3. Aguardar inicialização
```

### Opção B: Linha de Comando
```powershell
# Na raiz do projeto
cd src\Adapters\Inbound\TC.Agro.Analytics.Service

# Executar
dotnet run

# Ou com hot reload
dotnet watch run
```

**Saída esperada:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5174
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

## 🧪 2. TESTAR ENDPOINTS

### 🏥 Health Check (verificar se está rodando)

**Request:**
```http
GET http://localhost:5174/health
```

**Response esperada:**
```json
{
  "status": "Healthy",
  "timestamp": "2026-02-01T22:00:00Z",
  "service": "Analytics Worker Service"
}
```

---

### 📊 GET /alerts/pending

**Request:**
```http
GET http://localhost:5174/alerts/pending
```

**Response esperada:**
```json
{
  "alerts": [
    {
      "id": "6e1d2316-c80b-4f6e-87a7-661172cea2f3",
      "sensorReadingId": "d0c922a3-ed95-4895-a0b5-cc8730ae29c5",
      "sensorId": "SENSOR-SUPABASE-001",
      "plotId": "ae57f8d7-d491-4899-bb39-30124093e683",
      "alertType": "HighTemperature",
      "message": "🎉 SUCESSO! Teste de inserção no Supabase",
      "status": "Pending",
      "severity": "Critical",
      "value": 42.0,
      "threshold": 35.0,
      "createdAt": "2026-02-01T21:40:37.112648Z",
      "acknowledgedAt": null,
      "acknowledgedBy": null,
      "resolvedAt": null,
      "resolvedBy": null,
      "resolutionNotes": null
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

---

### 📈 GET /alerts/history/{plotId}

**Request:**
```http
GET http://localhost:5174/alerts/history/ae57f8d7-d491-4899-bb39-30124093e683?days=30
```

**Query Parameters:**
- `days` (opcional): Número de dias (default: 30)
- `alertType` (opcional): `HighTemperature`, `LowSoilMoisture`, `LowBattery`
- `status` (opcional): `Pending`, `Acknowledged`, `Resolved`

**Response esperada:**
```json
{
  "alerts": [
    {
      "id": "...",
      "alertType": "HighTemperature",
      "message": "High temperature detected: 42.0°C",
      "status": "Pending",
      "createdAt": "2026-02-01T21:40:37Z"
    }
  ],
  "totalCount": 1
}
```

---

### 🎯 GET /plots/{plotId}/status

**Request:**
```http
GET http://localhost:5174/plots/ae57f8d7-d491-4899-bb39-30124093e683/status
```

**Response esperada:**
```json
{
  "plotId": "ae57f8d7-d491-4899-bb39-30124093e683",
  "pendingAlertsCount": 1,
  "totalAlertsLast24Hours": 1,
  "totalAlertsLast7Days": 1,
  "mostRecentAlert": {
    "id": "6e1d2316-c80b-4f6e-87a7-661172cea2f3",
    "sensorId": "SENSOR-SUPABASE-001",
    "alertType": "HighTemperature",
    "message": "🎉 SUCESSO! Teste de inserção no Supabase",
    "severity": "Critical",
    "createdAt": "2026-02-01T21:40:37Z"
  },
  "alertsByType": {
    "HighTemperature": 1
  },
  "alertsBySeverity": {
    "Critical": 1
  },
  "overallStatus": "Critical"
}
```

---

## 📖 3. SWAGGER UI

**URL:** http://localhost:5174/swagger

**Funcionalidades:**
- ✅ Documentação interativa de todos os endpoints
- ✅ Testar endpoints diretamente no browser
- ✅ Ver schemas de request/response
- ✅ Copiar exemplos de cURL

---

## 🧪 4. TESTE COM VISUAL STUDIO CODE (.http FILE)

1. Abrir arquivo: `TC.Agro.Analytics.Service.http`
2. Instalar extensão: **REST Client** (Huachao Mao)
3. Clicar em **"Send Request"** acima de cada request

---

## ❌ 5. TROUBLESHOOTING

### Erro: "Connection refused" ou "404"

**Causa:** Serviço não está rodando

**Solução:**
```powershell
# Verificar se está rodando
dotnet run --project src\Adapters\Inbound\TC.Agro.Analytics.Service

# Verificar logs no terminal
```

---

### Erro: "Empty response" ou "[]"

**Causa:** Não há dados na tabela `alerts`

**Solução:** Inserir dados de teste no Supabase:
```sql
INSERT INTO analytics.alerts (
    id, sensor_reading_id, sensor_id, plot_id,
    alert_type, message, status, severity,
    value, threshold, created_at
) VALUES (
    gen_random_uuid(),
    gen_random_uuid(),
    'SENSOR-TEST-001',
    'ae57f8d7-d491-4899-bb39-30124093e683',
    'HighTemperature',
    'Test alert: High temperature 40°C',
    'Pending',
    'Critical',
    40.0,
    35.0,
    now()
);
```

---

### Erro: "Database connection failed"

**Causa:** Configuração do Supabase incorreta

**Solução:** Verificar `appsettings.Development.json`:
```json
{
  "Database": {
    "Postgres": {
      "Host": "db.sodwyfyhthybyqlqhdqy.supabase.co",
      "Port": 5432,
      "Database": "postgres",
      "UserName": "postgres",
      "Password": "!Fiap@2026#",
      "Schema": "analytics",
      "SslMode": "Require"
    }
  }
}
```

---

## 📊 6. TESTE E2E COMPLETO

### Passo 1: Inserir dados no Supabase
```sql
-- 3 tipos de alertas
INSERT INTO analytics.alerts (id, sensor_reading_id, sensor_id, plot_id, alert_type, message, status, severity, value, threshold, created_at) VALUES
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-001', 'ae57f8d7-d491-4899-bb39-30124093e683', 'HighTemperature', 'High temp: 38°C', 'Pending', 'High', 38.0, 35.0, now() - interval '2 hours'),
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-002', 'ae57f8d7-d491-4899-bb39-30124093e683', 'LowSoilMoisture', 'Low moisture: 15%', 'Pending', 'Critical', 15.0, 20.0, now() - interval '1 hour'),
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-003', 'ae57f8d7-d491-4899-bb39-30124093e683', 'LowBattery', 'Low battery: 10%', 'Pending', 'Medium', 10.0, 15.0, now());
```

### Passo 2: Executar API
```powershell
dotnet run --project src\Adapters\Inbound\TC.Agro.Analytics.Service
```

### Passo 3: Testar endpoints
```http
# 1. Health
GET http://localhost:5174/health

# 2. Pending (deve retornar 3 alertas)
GET http://localhost:5174/alerts/pending

# 3. History
GET http://localhost:5174/alerts/history/ae57f8d7-d491-4899-bb39-30124093e683

# 4. Status
GET http://localhost:5174/plots/ae57f8d7-d491-4899-bb39-30124093e683/status
```

### Passo 4: Validar respostas
- ✅ `/alerts/pending` retorna 3 alertas
- ✅ `/alerts/history` retorna histórico
- ✅ `/plots/{id}/status` retorna:
  - `pendingAlertsCount: 3`
  - `alertsByType: { HighTemperature: 1, LowSoilMoisture: 1, LowBattery: 1 }`
  - `overallStatus: "Critical"` (porque há alertas críticos)

---

## ✅ SUCESSO!

Se todos os endpoints retornarem dados corretos, você completou:
- ✅ FASE 1: Persistência (EF Core + Supabase)
- ✅ FASE 2: Projeção (AlertProjectionHandler)
- ✅ FASE 3: API (FastEndpoints)

**Parabéns! 🎉 O Analytics Worker está 100% funcional!** 🚀
