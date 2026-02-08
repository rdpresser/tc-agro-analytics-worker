using FastEndpoints;
using TC.Agro.Analytics.Application.UseCases.GetPendingAlerts;
using TC.Agro.Analytics.Application.UseCases.Shared;
using TC.Agro.SharedKernel.Api.Endpoints;

namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to retrieve all pending alerts across all plots.
/// GET /alerts/pending?pageNumber=1&pageSize=100
/// </summary>
public sealed class GetPendingAlertsEndpoint : BaseApiEndpoint<GetPendingAlertsQuery, AlertListResponse>
{
    public override void Configure()
    {
        Get("/alerts/pending");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get all pending alerts";
            s.Description = "Retrieves all alerts with status 'Pending' across all plots, ordered by creation date (most recent first)";
            s.Params["pageNumber"] = "Page number (default: 1)";
            s.Params["pageSize"] = "Page size (default: 100, max: 500)";
        });

        Description(d => d
            .Produces<AlertListResponse>(200, "application/json")
            .ProducesProblemDetails()
            .WithTags("Alerts"));
    }

    public override async Task HandleAsync(GetPendingAlertsQuery req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct);
        await MatchResultAsync(response, ct);
    }
}
