-- ==========================================
-- TC.Agro Analytics - Supabase Setup Script
-- ==========================================
-- Execute este script no Supabase SQL Editor
-- https://supabase.com/dashboard/project/SEU_PROJECT/sql
-- ==========================================

-- 1. Criar schema analytics
CREATE SCHEMA IF NOT EXISTS analytics;

-- 2. Dar permissões ao schema
GRANT ALL ON SCHEMA analytics TO postgres;
GRANT USAGE ON SCHEMA analytics TO authenticated;
GRANT USAGE ON SCHEMA analytics TO anon;

-- 3. Configurar search_path padrão
ALTER DATABASE postgres SET search_path TO analytics, public;

-- 4. Criar extensões úteis (opcional)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";      -- Para gen_random_uuid()
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements"; -- Para estatísticas de queries

-- 5. Verificar schema criado
SELECT schema_name 
FROM information_schema.schemata 
WHERE schema_name = 'analytics';

-- ==========================================
-- RESULTADO ESPERADO:
-- ✅ schema_name
-- ✅ analytics
-- ==========================================

-- 6. (OPCIONAL) Ver configuração do banco
SELECT name, setting 
FROM pg_settings 
WHERE name IN ('max_connections', 'shared_buffers', 'work_mem');

-- ==========================================
-- Após executar este script, você pode:
-- 1. Aplicar a migration do EF Core
-- 2. Inserir dados de teste
-- ==========================================

-- 7. Script de validação pós-migration (execute DEPOIS do dotnet ef database update)
-- Descomente as linhas abaixo após rodar a migration:

/*
-- Ver tabelas criadas
SELECT table_name, table_type
FROM information_schema.tables
WHERE table_schema = 'analytics'
ORDER BY table_name;

-- Ver colunas da tabela alerts
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_schema = 'analytics' 
  AND table_name = 'alerts'
ORDER BY ordinal_position;

-- Ver índices criados
SELECT 
    indexname,
    indexdef
FROM pg_indexes
WHERE schemaname = 'analytics'
  AND tablename = 'alerts';

-- Inserir alerta de teste
INSERT INTO analytics.alerts (
    id, 
    sensor_reading_id, 
    sensor_id, 
    plot_id,
    alert_type, 
    message, 
    status, 
    severity,
    value, 
    threshold, 
    created_at
) VALUES (
    gen_random_uuid(),
    gen_random_uuid(),
    'SENSOR-TEST-001',
    gen_random_uuid(),
    'HighTemperature',
    'Teste de alerta - Temperatura alta: 38.5°C',
    'Pending',
    'High',
    38.5,
    35.0,
    now()
);

-- Verificar inserção
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
*/

-- ==========================================
-- FIM DO SCRIPT
-- ==========================================
