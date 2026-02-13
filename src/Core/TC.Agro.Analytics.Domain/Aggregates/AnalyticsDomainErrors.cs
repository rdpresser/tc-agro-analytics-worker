namespace TC.Agro.Analytics.Domain.Aggregates;

/// <summary>
/// Domain errors for Analytics bounded context.
/// </summary>
[ExcludeFromCodeCoverage]
public static class AnalyticsDomainErrors
{
    #region Alert Errors

    public static readonly ValidationError AlertNotFound =
        new("Alert.NotFound", "Alert not found.");

    public static readonly ValidationError AlertAlreadyResolved =
        new("Alert.AlreadyResolved", "Alert is already resolved.");

    public static readonly ValidationError AlertNotPending =
        new("Alert.NotPending", "Only pending alerts can be acknowledged.");

    public static readonly ValidationError AlertNotAcknowledged =
        new("Alert.NotAcknowledged", "Alert must be acknowledged before resolving.");

    #endregion

    #region Rule Errors (Future)

    public static readonly ValidationError RuleNotFound =
        new("Rule.NotFound", "Rule not found.");

    public static readonly ValidationError RuleAlreadyExists =
        new("Rule.AlreadyExists", "Rule already exists for this plot.");

    #endregion
}
