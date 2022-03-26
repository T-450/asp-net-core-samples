using NotificationPatternExample.Business.Interfaces;
using NotificationPatternExample.Business.Models;

namespace NotificationPatternExample.Business.Notifications;

public class Notificator : INotificator
{
    private readonly List<Notification> _notifications;

    public Notificator()
    {
        _notifications = new List<Notification>();
    }

    public void Handle(Notification n)
    {
        _notifications.Add(n);
    }

    public void Handle(IEnumerable<Notification> notifications)
    {
        _notifications.AddRange(notifications);
    }

    public List<Notification> GetNotifications()
    {
        return _notifications;
    }

    public bool HasNotification()
    {
        return _notifications.Any();
    }
}
