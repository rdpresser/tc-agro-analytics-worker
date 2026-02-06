using TC.Agro.Analytics.Application.UseCases.Shared;
using TC.Agro.Analytics.Application.UseCases.GetPlotStatus;
using TC.Agro.SharedKernel.Api.Endpoints;

namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to get plot status summary with aggregated metrics.
/// GET /plots/{plotId}/status
/// </summary>
public sealed class GetPlotStatusEndpoint : BaseApiEndpoint<GetPlotStatusQuery, PlotStatusResponse>
{
    public override void Configure()
    {
        Get("/plots/{plotId}/status");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get plot status summary";
            s.Description = "Retrieves aggregated alert metrics for a plot (last 7 days)";
            s.Params["plotId"] = "Plot ID (GUID)";
        });

        Description(d => d
            .Produces<PlotStatusResponse>(200, "application/json")
            .ProducesProblemDetails()
            .Produces(404)
            .WithTags("Plots"));
    }

    public override async Task HandleAsync(GetPlotStatusQuery req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct);
        await MatchResultAsync(response, ct);
    }
}
