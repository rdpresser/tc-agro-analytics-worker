# 🚀 Guia Rápido: Configurar Supabase

## 1️⃣ Criar Conta Supabase

1. Acesse: https://supabase.com
2. Login com GitHub
3. Criar novo projeto:
   - **Name:** `tc-agro-analytics`
   - **Database Password:** `SuaSenhaForte123!` (anote!)
   - **Region:** `South America (São Paulo)`
   - **Plan:** `Free`

## 2️⃣ Pegar Connection String

1. **Settings** → **Database** → **Connection string** (URI)
2. Copie a string que aparece (exemplo):

```
postgresql://postgres.xxxxxxxxxxxxxxxxxxxx:SuaSenhaForte123!@aws-0-sa-east-1.pooler.supabase.com:6543/postgres
```

## 3️⃣ Extrair Informações

Da connection string acima, extraia:

- **Host:** `aws-0-sa-east-1.pooler.supabase.com`
- **Port:** `6543`
- **Database:** `postgres`
- **UserName:** `postgres.xxxxxxxxxxxxxxxxxxxx` (parte antes do `:`)
- **Password:** `SuaSenhaForte123!` (parte depois do `:` e antes do `@`)

## 4️⃣ Atualizar appsettings.Supabase.json

Abra o arquivo `src/Adapters/Inbound/TC.Agro.Analytics.Service/appsettings.Supabase.json` e substitua:

```json
{
  "Database": {
    "Postgres": {
      "Host": "SEU_HOST_AQUI.pooler.supabase.com",
      "Port": 6543,
      "Database": "postgres",
      "UserName": "postgres.SEU_PROJECT_ID",
      "Password": "SUA_SENHA_AQUI",
      "Schema": "analytics",
      "SslMode": "Require"
    }
  }
}
```

## 5️⃣ Criar Schema `analytics` no Supabase

No Supabase SQL Editor (https://supabase.com/dashboard/project/SEU_PROJECT/sql):

```sql
-- Criar schema analytics
CREATE SCHEMA IF NOT EXISTS analytics;

-- Dar permissões
GRANT ALL ON SCHEMA analytics TO postgres;
GRANT ALL ON SCHEMA analytics TO authenticated;
GRANT ALL ON SCHEMA analytics TO anon;
```

## 6️⃣ Executar Migration

No terminal (PowerShell), execute:

```powershell
# Definir environment
$env:ASPNETCORE_ENVIRONMENT="Supabase"

# Aplicar migration
cd src\Adapters\Outbound\TC.Agro.Analytics.Infrastructure
dotnet ef database update --startup-project ..\..\Inbound\TC.Agro.Analytics.Service
```

## 7️⃣ Verificar Tabela Criada

No Supabase SQL Editor:

```sql
-- Ver tabelas no schema analytics
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'analytics';

-- Ver estrutura da tabela alerts
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'analytics' 
  AND table_name = 'alerts'
ORDER BY ordinal_position;
```

## 8️⃣ Testar Inserção

```sql
-- Inserir alerta de teste
INSERT INTO analytics.alerts (
    id, sensor_reading_id, sensor_id, plot_id,
    alert_type, message, status, severity,
    value, threshold, created_at
) VALUES (
    gen_random_uuid(),
    gen_random_uuid(),
    'SENSOR-TEST-001',
    gen_random_uuid(),
    'HighTemperature',
    'Temperatura alta detectada: 38.5°C',
    'Pending',
    'High',
    38.5,
    35.0,
    now()
);

-- Verificar inserção
SELECT * FROM analytics.alerts;
```

## ✅ Pronto!

Agora você tem:
- ✅ PostgreSQL na nuvem (gratuito)
- ✅ Schema `analytics` criado
- ✅ Tabela `alerts` com migration aplicada
- ✅ Connection string configurada
- ✅ Pronto para desenvolvimento

---

## 🔍 Troubleshooting

### Erro: "SSL connection required"

Certifique-se que no `appsettings.Supabase.json` está:

```json
"SslMode": "Require"
```

### Erro: "permission denied for schema analytics"

Execute no SQL Editor:

```sql
GRANT ALL ON SCHEMA analytics TO postgres;
GRANT ALL ON ALL TABLES IN SCHEMA analytics TO postgres;
```

### Erro: "peer authentication failed"

Verifique que está usando a **connection string correta** do Supabase (aba URI, não pooler mode).

---

## 📚 Referências

- Supabase Docs: https://supabase.com/docs
- PostgreSQL Connection Strings: https://www.postgresql.org/docs/current/libpq-connect.html#LIBPQ-CONNSTRING
