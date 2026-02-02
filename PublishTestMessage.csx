#!/usr/bin/env dotnet-script
#r "nuget: RabbitMQ.Client, 6.8.1"
#r "nuget: Newtonsoft.Json, 13.0.3"

using RabbitMQ.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Collections.Generic;

// Mensagem de teste
var message = new
{
    EventId = Guid.NewGuid().ToString(),
    AggregateId = Guid.NewGuid().ToString(),
    OccurredOn = DateTime.UtcNow,
    EventName = "SensorIngestedIntegrationEvent",
    RelatedIds = (Dictionary<string, Guid>)null,
    SensorId = "SENSOR-TEST-001",
    PlotId = Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683"),
    Time = DateTime.UtcNow,
    Temperature = 42.5,
    Humidity = 65.0,
    SoilMoisture = 35.0,
    Rainfall = 2.5,
    BatteryLevel = 85.0
};

Console.WriteLine("📡 Conectando ao RabbitMQ...");

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

var json = JsonConvert.SerializeObject(message);
var body = Encoding.UTF8.GetBytes(json);

var properties = channel.CreateBasicProperties();
properties.Persistent = true;
properties.ContentType = "application/json";
properties.MessageId = message.EventId;

// Headers do Wolverine
properties.Headers = new Dictionary<string, object>
{
    { "message-type", "TC.Agro.Contracts.Events.Analytics.SensorIngestedIntegrationEvent, TC.Agro.Contracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" },
    { "content-type", "application/json" }
};

channel.BasicPublish(
    exchange: "",
    routingKey: "analytics.sensor.ingested.queue",
    basicProperties: properties,
    body: body
);

Console.WriteLine($"✅ Mensagem publicada com sucesso!");
Console.WriteLine($"   EventId: {message.EventId}");
Console.WriteLine($"   Sensor: {message.SensorId}");
Console.WriteLine($"   Temperature: {message.Temperature}°C");
