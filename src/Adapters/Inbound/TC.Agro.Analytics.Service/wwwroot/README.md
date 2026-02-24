# 🚨 AlertHub SignalR Test Page

## 📋 Overview

This is a **real-time testing interface** for the **AlertHub SignalR** endpoint in the TC.Agro.Analytics service. It allows you to test real-time alert notifications for sensor data analysis.

**URL:** `https://localhost:7132/signalr-test.html`

---

## 🎯 Purpose

The `signalr-test.html` page provides a **visual interface** to:

1. ✅ **Connect** to the AlertHub SignalR endpoint with JWT authentication
2. ✅ **Join/Leave** plot groups to receive alerts for specific plots
3. ✅ **Monitor** real-time alert notifications:
   - **AlertCreated**: New alert triggered by sensor data
   - **AlertAcknowledged**: User acknowledged an alert
   - **AlertResolved**: User resolved an alert
4. ✅ **View** detailed event logs with timestamps

---

## 🚀 How to Use

### **1. Start the Analytics Worker Service**

```powershell
cd C:\FIAP\Hackathon\tc-agro-solutions\services\analytics-worker
dotnet run --project src/Adapters/Inbound/TC.Agro.Analytics.Service
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7132
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### **2. Get a JWT Token**

**Option A: Via Identity Service** (if running)
```powershell
POST https://localhost:5001/identity/auth/login
Content-Type: application/json

{
  "username": "johnsmith",
  "password": "YourPassword123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-02-24T23:59:59Z"
}
```

**Option B: Use test token** (from Identity Service documentation)

### **3. Open the Test Page**

Navigate to: **`https://localhost:7132/signalr-test.html`**

### **4. Connect to AlertHub**

1. **Paste your JWT token** in the "JWT Token" field
2. Click **Connect**
3. ✅ You should see: `Connected successfully! ConnectionId: abc123`

### **5. Join a Plot Group**

1. Enter a **Plot ID (GUID)** (example: `11111111-1111-1111-1111-111111111111`)
2. Click **Join Plot**
3. ✅ You should see:
   - `Joined plot group: 11111111-...`
   - Recent active alerts for that plot (if any exist)

---

## 🧪 Testing Scenarios

### **Scenario 1: Real-time Alert Creation** 🚨

**Goal:** Test that new alerts trigger SignalR notifications

**Steps:**
1. Connect to AlertHub and join a plot group
2. Send sensor data via RabbitMQ that triggers an alert:

```json
// Message to RabbitMQ (SensorIngested event)
{
  "sensorId": "b2c3d4e5-f6a7-4b6c-9d0e-1f2a3b4c5d6e",
  "temperature": 45.0,  // Above threshold (35°C)
  "soilMoisture": 25.0,
  "humidity": 60.0,
  "batteryLevel": 85.0,
  "timestamp": "2026-02-24T12:00:00Z"
}
```

**Expected Result:**
```
[12:00:01] 🚨 AlertCreated
    Message: High temperature detected: 45.0°C
    Type: HighTemperature
    Severity: High
    Sensor: Sensor-001 (b2c3d4e5-...)
    Plot: Talhão Norte (11111111-...)
    Value: 45 (Threshold: 35)
    Alert ID: 5f1c8f28-...
```

---

### **Scenario 2: Acknowledge Alert** ✅

**Goal:** Test that acknowledging an alert notifies all connected clients

**Steps:**
1. Connect two browser tabs to the same plot
2. In another tool (Postman/curl), acknowledge an alert:

```bash
curl -X POST https://localhost:7132/analytics/alerts/{alertId}/acknowledge \
  -H "Authorization: Bearer {your-jwt-token}"
```

**Expected Result (both tabs):**
```
[12:01:30] ✅ AlertAcknowledged
    Alert ID: 5f1c8f28-...
    Sensor: Sensor-001 (b2c3d4e5-...)
    Plot: Talhão Norte (11111111-...)
    Acknowledged By: 9823af89-...
    Acknowledged At: 24/02/2026, 12:01:30
```

---

### **Scenario 3: Resolve Alert** ✅

**Goal:** Test that resolving an alert notifies all connected clients

**Steps:**
1. Connect to AlertHub
2. Resolve an alert via API:

```bash
curl -X POST https://localhost:7132/analytics/alerts/{alertId}/resolve \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{"resolutionNotes": "Problema resolvido, sensor reajustado"}'
```

**Expected Result:**
```
[12:05:00] ✅ AlertResolved
    Alert ID: 5f1c8f28-...
    Sensor: Sensor-001 (b2c3d4e5-...)
    Plot: Talhão Norte (11111111-...)
    Resolved By: 9823af89-...
    Resolution Notes: Problema resolvido, sensor reajustado
    Resolved At: 24/02/2026, 12:05:00
```

---

## 📊 Features

### **Connection Management**
- ✅ JWT Bearer authentication
- ✅ Automatic reconnection on network failures
- ✅ Visual connection status (Connected/Disconnected/Connecting)
- ✅ Connection ID display

### **Plot Group Management**
- ✅ Join plot groups (receive alerts for specific plots)
- ✅ Leave plot groups (stop receiving alerts)
- ✅ GUID validation
- ✅ Recent alerts on join (last 20 active alerts)

### **Event Log**
- ✅ Color-coded log entries (Info/Success/Error/Alert)
- ✅ Timestamps for all events
- ✅ Detailed alert information
- ✅ Auto-scroll to latest events
- ✅ Clear log button

### **SignalR Events**
| Event | Description | Color |
|-------|-------------|-------|
| **AlertCreated** | New alert triggered by sensor data | 🟡 Yellow |
| **AlertAcknowledged** | User acknowledged an alert | 🟢 Green |
| **AlertResolved** | User resolved an alert | 🟢 Green |

---

## 🔧 Technical Details

### **SignalR Configuration**

**Hub URL:** `https://localhost:7132/dashboard/alertshub`

**Authentication:** JWT Bearer token via query string
```javascript
.withUrl(hubUrl, {
    accessTokenFactory: () => token
})
```

**Transport:** WebSockets (fallback to Server-Sent Events, Long Polling)

**Reconnection Policy:** Automatic with exponential backoff

### **Hub Methods**

| Method | Parameters | Description |
|--------|------------|-------------|
| `JoinPlotGroup` | `plotId: string` | Join group `plot:{plotId}` to receive alerts |
| `LeavePlotGroup` | `plotId: string` | Leave group `plot:{plotId}` |

### **Hub Events**

| Event | Payload | Description |
|-------|---------|-------------|
| `AlertCreated` | `AlertCreatedNotification` | New alert was created |
| `AlertAcknowledged` | `AlertAcknowledgedNotification` | Alert was acknowledged |
| `AlertResolved` | `AlertResolvedNotification` | Alert was resolved |

### **Notification Models**

**AlertCreatedNotification:**
```typescript
{
  alertId: string,
  sensorId: string,
  sensorLabel: string,
  plotId: string,
  plotName: string,
  propertyName: string,
  alertType: string,      // "HighTemperature", "LowSoilMoisture", "LowBattery"
  severity: string,       // "Low", "Medium", "High", "Critical"
  message: string,
  value: number,
  threshold: number,
  metadata: string?,
  createdAt: DateTime
}
```

**AlertAcknowledgedNotification:**
```typescript
{
  alertId: string,
  sensorId: string,
  sensorLabel: string,
  plotId: string,
  plotName: string,
  propertyName: string,
  acknowledgedBy: string,
  acknowledgedAt: DateTime
}
```

**AlertResolvedNotification:**
```typescript
{
  alertId: string,
  sensorId: string,
  sensorLabel: string,
  plotId: string,
  plotName: string,
  propertyName: string,
  resolvedBy: string,
  resolutionNotes: string?,
  resolvedAt: DateTime
}
```

---

## 🐛 Troubleshooting

### **Error: 404 Not Found**

**Problem:** Hub endpoint not found

**Solution:**
1. Check if `UseStaticFiles()` is enabled in `Program.cs`
2. Verify hub is mapped: `app.MapHub<AlertHub>("/dashboard/alertshub")`
3. Restart the application

### **Error: 401 Unauthorized**

**Problem:** JWT token authentication failed

**Solution:**
1. Verify token is not expired (check `exp` claim)
2. Ensure token has correct audience: `tc-agro-analytics-worker`
3. Check if user has `Admin` or `Producer` role
4. Verify `SecretKey` matches between Identity and Analytics services

### **Error: WebSocket Failed to Connect**

**Problem:** WebSocket transport failing

**Solution:**
1. Check if SignalR JWT configuration is present in `ServiceCollectionExtensions.cs`:
```csharp
opt.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        if (!string.IsNullOrEmpty(accessToken) &&
            context.HttpContext.Request.Path.StartsWithSegments("/dashboard/alertshub"))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }
};
```
2. Verify CORS is configured: `UseCors("DefaultCorsPolicy")`
3. Check browser console for detailed errors

### **Error: No Alerts Received on Join**

**Problem:** Joined plot but no alerts appear

**Solution:**
1. Verify plot exists in database
2. Check if plot has sensors with active alerts
3. Query database:
```sql
SELECT a.* 
FROM alerts a
JOIN sensor_snapshots s ON a.sensor_id = s.id
WHERE s.plot_id = '11111111-1111-1111-1111-111111111111'
  AND a.status IN ('Pending', 'Acknowledged')
ORDER BY a.created_at DESC
LIMIT 20;
```

---

## 📁 File Structure

```
src/Adapters/Inbound/TC.Agro.Analytics.Service/wwwroot/
├── signalr-test.html          # SignalR test interface
└── README.md                  # This file
```

---

## 🔗 Related Files

- **AlertHub:** `src/Adapters/Inbound/TC.Agro.Analytics.Service/Hubs/AlertHub.cs`
- **AlertHubNotifier:** `src/Adapters/Inbound/TC.Agro.Analytics.Service/Services/AlertHubNotifier.cs`
- **IAlertHubClient:** `src/Adapters/Inbound/TC.Agro.Analytics.Service/Hubs/IAlertHubClient.cs`
- **Program.cs:** `src/Adapters/Inbound/TC.Agro.Analytics.Service/Program.cs`
- **ServiceCollectionExtensions:** `src/Adapters/Inbound/TC.Agro.Analytics.Service/Extensions/ServiceCollectionExtensions.cs`

---

## 🎓 Learning Resources

### **SignalR Documentation**
- [ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [SignalR JavaScript Client](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
- [SignalR Authentication](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz)

### **Project Documentation**
- Main README: `../../README.md`
- Architecture: `../../docs/ARCHITECTURE.md`
- API Documentation: `https://localhost:7132/swagger`

---

## 🚀 Next Steps

After testing the SignalR connection, consider:

1. **Integration Testing:** Write automated tests for SignalR notifications
2. **Load Testing:** Test with multiple concurrent connections
3. **Dashboard Integration:** Integrate AlertHub into the frontend dashboard
4. **Monitoring:** Add metrics for SignalR connections (active connections, messages/sec)

---

## 📝 Notes

- **Authentication Required:** All SignalR connections require valid JWT token with `Admin` or `Producer` role
- **Plot Isolation:** Clients only receive alerts for plots they've joined
- **Performance:** Hub uses cached sensor snapshots (10-minute TTL) to reduce database queries
- **Scalability:** Hub groups (`plot:{plotId}`) allow efficient broadcasting to multiple clients

---

## 📞 Support

For issues or questions:
- Check logs: `Logging:LogLevel:Default=Debug` in `appsettings.Development.json`
- Review documentation: `docs/` folder
- Contact team: tc-agro-dev@example.com

---

**Happy Testing! 🎉**
