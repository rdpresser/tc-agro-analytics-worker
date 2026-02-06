using TC.Agro.Analytics.Application.UseCases.Shared;
using TC.Agro.Analytics.Application.UseCases.GetAlertHistory;
using TC.Agro.SharedKernel.Api.Endpoints;

namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to retrieve alert history for a specific plot.
/// GET /alerts/history/{plotId}?days=30&alertType=HighTemperature&status=Pending
/// </summary>
public sealed class GetAlertHistoryEndpoint : BaseApiEndpoint<GetAlertHistoryQuery, AlertListResponse>
{
    public override void Configure()
    {
        Get("/alerts/history/{plotId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get alert history for a plot";
            s.Description = "Retrieves historical alerts for a specific plot with optional filters";
            s.Params["plotId"] = "Plot ID (GUID)";
            s.Params["days"] = "Number of days to look back (default: 30)";
            s.Params["alertType"] = "Filter by alert type (optional): HighTemperature, LowSoilMoisture, LowBattery";
            s.Params["status"] = "Filter by status (optional): Pending, Acknowledged, Resolved";
        });

        Description(d => d
            .Produces<AlertListResponse>(200, "application/json")
            .ProducesProblemDetails()
            .Produces(404)
            .WithTags("Alerts"));
    }

    public override async Task HandleAsync(GetAlertHistoryQuery req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct);
        await MatchResultAsync(response, ct);
    }
}
