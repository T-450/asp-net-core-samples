using NotificationPatternExample.Business.Models;

namespace NotificationPatternExample.Business.Interfaces;

public interface INotificator
{
    bool HasNotification();
    List<Notification> GetNotifications();
    void Handle(Notification n);
    void Handle(IEnumerable<Notification> messages);
}
