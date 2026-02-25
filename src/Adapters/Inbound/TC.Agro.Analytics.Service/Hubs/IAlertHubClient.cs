namespace TC.Agro.Analytics.Service.Hubs;

public interface IAlertHubClient
{
    Task AlertCreated(AlertCreatedNotification alert);
    Task AlertAcknowledged(AlertAcknowledgedNotification alert);
    Task AlertResolved(AlertResolvedNotification alert);
}
