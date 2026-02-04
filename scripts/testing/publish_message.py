#!/usr/bin/env python3
"""
Script para publicar mensagens de teste no RabbitMQ no formato Wolverine
Para o Analytics Worker processar

Uso:
    python publish_message.py
    python publish_message.py --scenario high-temp
    python publish_message.py --scenario low-soil
    python publish_message.py --scenario low-battery
    python publish_message.py --scenario multiple
"""

import pika
import json
import uuid
from datetime import datetime, timezone
import argparse


def generate_wolverine_message_id():
    """Gera Message ID no formato Wolverine (HiLo-based)"""
    # Formato: 08de6212-074e-7629-0015-5d42ba480000
    return str(uuid.uuid4())


def publish_message(scenario='high-temp'):
    """Publica mensagem de teste no RabbitMQ no formato Wolverine"""

    # Conectar no RabbitMQ
    print("📡 Conectando ao RabbitMQ...")
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(
            'localhost', 
            5672, 
            '/', 
            pika.PlainCredentials('guest', 'guest')
        )
    )
    channel = connection.channel()

    # Gerar IDs únicos
    event_id = str(uuid.uuid4())
    aggregate_id = str(uuid.uuid4())
    message_id = generate_wolverine_message_id()
    correlation_id = str(uuid.uuid4())
    conversation_id = message_id  # Wolverine usa message_id como conversation_id
    plot_id = "ae57f8d7-d491-4899-bb39-30124093e683"  # ID fixo para testes

    # Cenários de teste
    scenarios = {
        'high-temp': {
            'sensorId': 'SENSOR-TEST-001',
            'temperature': 42.5,  # Acima do threshold (35°C)
            'humidity': 65.0,
            'soilMoisture': 35.0,
            'rainfall': 2.5,
            'batteryLevel': 85.0,
            'description': '🌡️  Alta Temperatura (42.5°C > 35°C)'
        },
        'low-soil': {
            'sensorId': 'SENSOR-TEST-002',
            'temperature': 25.0,
            'humidity': 45.0,
            'soilMoisture': 15.0,  # Abaixo do threshold (20%)
            'rainfall': 0.0,
            'batteryLevel': 90.0,
            'description': '💧 Baixa Umidade do Solo (15% < 20%)'
        },
        'low-battery': {
            'sensorId': 'SENSOR-TEST-003',
            'temperature': 28.0,
            'humidity': 60.0,
            'soilMoisture': 30.0,
            'batteryLevel': 10.0,  # Abaixo do threshold (20%)
            'rainfall': 1.0,
            'description': '🔋 Bateria Baixa (10% < 20%)'
        },
        'multiple': {
            'sensorId': 'SENSOR-TEST-004',
            'temperature': 45.0,  # Acima
            'humidity': 40.0,
            'soilMoisture': 12.0,  # Abaixo
            'batteryLevel': 8.0,   # Abaixo
            'rainfall': 0.0,
            'description': '⚠️  MÚLTIPLOS ALERTAS (3 simultaneamente)'
        },
        'ok': {
            'sensorId': 'SENSOR-TEST-005',
            'temperature': 25.0,
            'humidity': 60.0,
            'soilMoisture': 35.0,
            'batteryLevel': 85.0,
            'rainfall': 1.5,
            'description': '✅ Todos os valores OK (sem alertas)'
        }
    }

    # Selecionar cenário
    selected = scenarios.get(scenario, scenarios['high-temp'])

    # Timestamp UTC no formato ISO8601
    now_utc = datetime.now(timezone.utc)
    occurred_on = now_utc.isoformat()
    time_reading = now_utc.isoformat()
    sent_at = now_utc.strftime('%Y-%m-%d %H:%M:%S:%f')[:-3] + ' Z'  # Formato Wolverine

    # Payload exatamente como Wolverine espera (camelCase!)
    message = {
        "sensorId": selected['sensorId'],
        "plotId": plot_id,
        "time": time_reading,
        "temperature": selected['temperature'],
        "humidity": selected['humidity'],
        "soilMoisture": selected['soilMoisture'],
        "rainfall": selected['rainfall'],
        "batteryLevel": selected['batteryLevel'],
        "eventId": event_id,
        "aggregateId": aggregate_id,
        "occurredOn": occurred_on,
        "eventName": "SensorIngestedIntegrationEvent",
        "relatedIds": None
    }

    # Headers Wolverine completos
    headers = {
        'accepted-content-types': 'application/json',
        'attempts': 0,
        'conversation-id': conversation_id,
        'reply-uri': f'rabbitmq://queue/wolverine.response.{uuid.uuid4()}',
        'sent-at': sent_at,
        'source': 'PythonTestScript',
        'wolverine-protocol-version': '1.0'
    }

    # Properties RabbitMQ
    properties = pika.BasicProperties(
        type='TC.Agro.Contracts.Events.Analytics.SensorIngestedIntegrationEvent',
        message_id=message_id,
        correlation_id=correlation_id,
        delivery_mode=1,  # Non-persistent (como no exemplo)
        content_type='application/json',
        headers=headers
    )

    # Publicar mensagem
    print(f"\n📨 Publicando mensagem:")
    print(f"   Cenário: {selected['description']}")
    print(f"   Sensor: {selected['sensorId']}")
    print(f"   MessageId: {message_id}")
    print(f"   AggregateId: {aggregate_id}")
    print(f"\n   Valores:")
    print(f"      Temperature: {selected['temperature']}°C")
    print(f"      SoilMoisture: {selected['soilMoisture']}%")
    print(f"      BatteryLevel: {selected['batteryLevel']}%")
    print(f"\n   Properties:")
    print(f"      Type: TC.Agro.Contracts.Events.Analytics.SensorIngestedIntegrationEvent")
    print(f"      ContentType: application/json")
    print(f"      DeliveryMode: 1 (non-persistent)")

    channel.basic_publish(
        exchange='',  # AMQP default exchange
        routing_key='analytics.sensor.ingested.queue',
        body=json.dumps(message),
        properties=properties
    )

    print(f"\n✅ Mensagem enviada com sucesso!")
    print(f"   Queue: analytics.sensor.ingested.queue")
    print(f"\n🔍 Próximos passos:")
    print(f"   1. Verifique os logs da aplicação")
    print(f"   2. Consulte o banco:")
    print(f"      docker exec -it tc-agro-postgres psql -U postgres -d tc-agro-analytics-db -c \\")
    print(f"      \"SELECT sensor_id, alert_type, message, severity FROM analytics.alerts ORDER BY created_at DESC LIMIT 5;\"")

    connection.close()


def main():
    parser = argparse.ArgumentParser(description='Publicar mensagem de teste no RabbitMQ (formato Wolverine)')
    parser.add_argument(
        '--scenario',
        type=str,
        default='high-temp',
        choices=['high-temp', 'low-soil', 'low-battery', 'multiple', 'ok'],
        help='Cenário de teste a executar'
    )

    args = parser.parse_args()

    print("=" * 70)
    print("🧪 ANALYTICS WORKER - TESTE E2E (Formato Wolverine)")
    print("=" * 70)

    try:
        publish_message(args.scenario)
    except Exception as e:
        print(f"\n❌ Erro ao publicar mensagem: {e}")
        print(f"\n💡 Dicas:")
        print(f"   - Verifique se RabbitMQ está rodando: docker ps | grep rabbitmq")
        print(f"   - Verifique se a fila existe: http://localhost:15672")
        print(f"   - Instale dependências: pip install pika")
        return 1

    return 0


if __name__ == '__main__':
    exit(main())
