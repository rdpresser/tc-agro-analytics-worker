-- ============================================
-- Script de Teste E2E - Analytics Worker
-- Execute no Supabase SQL Editor
-- ============================================

-- 1. Limpar dados de teste anteriores (opcional)
-- DELETE FROM analytics.alerts WHERE sensor_id LIKE 'SENSOR-E2E-%';

-- 2. Criar 10 alertas de teste de tipos diferentes
INSERT INTO analytics.alerts (id, sensor_reading_id, sensor_id, plot_id, alert_type, message, status, severity, value, threshold, created_at) VALUES
-- HighTemperature alerts (Critical e High)
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-E2E-001', 'ae57f8d7-d491-4899-bb39-30124093e683', 'HighTemperature', 'Critical: Temperature 45°C', 'Pending', 'Critical', 45.0, 35.0, now() - interval '30 minutes'),
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-E2E-002', 'ae57f8d7-d491-4899-bb39-30124093e683', 'HighTemperature', 'High: Temperature 38°C', 'Pending', 'High', 38.0, 35.0, now() - interval '1 hour'),
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-E2E-003', 'ae57f8d7-d491-4899-bb39-30124093e683', 'HighTemperature', 'Medium: Temperature 37°C', 'Acknowledged', 'Medium', 37.0, 35.0, now() - interval '2 hours'),

-- LowSoilMoisture alerts (Critical)
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-E2E-004', 'ae57f8d7-d491-4899-bb39-30124093e683', 'LowSoilMoisture', 'Critical: Soil moisture 10%', 'Pending', 'Critical', 10.0, 20.0, now() - interval '45 minutes'),
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-E2E-005', 'ae57f8d7-d491-4899-bb39-30124093e683', 'LowSoilMoisture', 'High: Soil moisture 15%', 'Pending', 'High', 15.0, 20.0, now() - interval '3 hours'),
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-E2E-006', 'ae57f8d7-d491-4899-bb39-30124093e683', 'LowSoilMoisture', 'Resolved: Soil moisture was low', 'Resolved', 'Medium', 18.0, 20.0, now() - interval '1 day'),

-- LowBattery alerts (Medium e Low)
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-E2E-007', 'ae57f8d7-d491-4899-bb39-30124093e683', 'LowBattery', 'High: Battery 8%', 'Pending', 'High', 8.0, 15.0, now() - interval '20 minutes'),
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-E2E-008', 'ae57f8d7-d491-4899-bb39-30124093e683', 'LowBattery', 'Medium: Battery 12%', 'Acknowledged', 'Medium', 12.0, 15.0, now() - interval '4 hours'),
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-E2E-009', 'ae57f8d7-d491-4899-bb39-30124093e683', 'LowBattery', 'Low: Battery 14%', 'Resolved', 'Low', 14.0, 15.0, now() - interval '2 days'),

-- Alert antigo (fora do range de 7 dias para testar filtro)
(gen_random_uuid(), gen_random_uuid(), 'SENSOR-E2E-010', 'ae57f8d7-d491-4899-bb39-30124093e683', 'HighTemperature', 'Old alert - should not appear in 7-day queries', 'Resolved', 'High', 40.0, 35.0, now() - interval '10 days');

-- 3. Verificar inserção
SELECT 
    alert_type,
    status,
    severity,
    COUNT(*) as count
FROM analytics.alerts
WHERE sensor_id LIKE 'SENSOR-E2E-%'
GROUP BY alert_type, status, severity
ORDER BY alert_type, severity;

-- Resultado esperado:
-- HighTemperature | Acknowledged | Medium    | 1
-- HighTemperature | Pending      | Critical  | 1
-- HighTemperature | Pending      | High      | 1
-- HighTemperature | Resolved     | High      | 1
-- LowBattery      | Acknowledged | Medium    | 1
-- LowBattery      | Pending      | High      | 1
-- LowBattery      | Resolved     | Low       | 1
-- LowSoilMoisture | Pending      | Critical  | 1
-- LowSoilMoisture | Pending      | High      | 1
-- LowSoilMoisture | Resolved     | Medium    | 1

-- 4. Ver todos os alertas inseridos
SELECT 
    sensor_id,
    alert_type,
    message,
    status,
    severity,
    value,
    threshold,
    created_at
FROM analytics.alerts
WHERE sensor_id LIKE 'SENSOR-E2E-%'
ORDER BY created_at DESC;
