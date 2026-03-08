using System.Reflection;
using TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlerts;
using TC.Agro.Analytics.Domain.Aggregates;
using TC.Agro.Analytics.Infrastructure;

namespace TC.Agro.Analytics.Architecture.Tests;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(AlertAggregate).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(GetPendingAlertsQuery).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}
