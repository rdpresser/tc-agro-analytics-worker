using System.Net;
using System.Net.Http.Json;
using TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlerts;
using TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlertsSummary;
using TC.Agro.Analytics.Tests.TestHelpers.Api;
using TC.Agro.SharedKernel.Infrastructure.Pagination;

namespace TC.Agro.Analytics.Tests.Service.Api;

public sealed class AlertsApiFlowTests : IClassFixture<AnalyticsApiWebApplicationFactory>
{
    private readonly AnalyticsApiWebApplicationFactory _factory;

    public AlertsApiFlowTests(AnalyticsApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPendingAlerts_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;

        await _factory.ResetDatabaseAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/alerts/pending?pageNumber=1&pageSize=20", ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPendingAlerts_WithProducerRole_ShouldReturnOwnerScopedAlertsAndSummary()
    {
        var ct = TestContext.Current.CancellationToken;

        await _factory.ResetDatabaseAsync();

        var ownerId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        await _factory.SeedPendingAlertAsync(ownerId, sensorId);

        using var client = _factory.CreateAuthenticatedClient("Producer", ownerId);

        var pendingResponse = await client.GetAsync("/api/alerts/pending?pageNumber=1&pageSize=20", ct);

        pendingResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var pendingPayload = await pendingResponse.Content.ReadFromJsonAsync<PaginatedResponse<PendingAlertResponse>>(cancellationToken: ct);
        pendingPayload.ShouldNotBeNull();
        pendingPayload!.TotalCount.ShouldBe(1);
        pendingPayload.Data.Count.ShouldBe(1);
        pendingPayload.Data[0].SensorId.ShouldBe(sensorId);

        var summaryResponse = await client.GetAsync("/api/alerts/pending/summary?windowHours=24", ct);

        summaryResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var summaryPayload = await summaryResponse.Content.ReadFromJsonAsync<PendingAlertsSummaryResponse>(cancellationToken: ct);
        summaryPayload.ShouldNotBeNull();
        summaryPayload!.PendingAlertsTotal.ShouldBe(1);
        summaryPayload.CriticalPendingCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetPendingAlerts_WithInvalidRole_ShouldReturnForbidden()
    {
        var ct = TestContext.Current.CancellationToken;

        await _factory.ResetDatabaseAsync();

        using var client = _factory.CreateAuthenticatedClient("Sensor");

        var response = await client.GetAsync("/api/alerts/pending?pageNumber=1&pageSize=20", ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
