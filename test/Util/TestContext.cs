
using MediatR;

using Microsoft.Extensions.DependencyInjection;

using PersonalKanban;

namespace PersonalKanbanTest;

public interface INotificationObserver<TNotification> where TNotification : INotification
{
    TNotification? Notification { get; }
}

public class TestContext
{

    private readonly ServiceCollection _services = new ServiceCollection();

    public TestContext()
    {
        _services.AddPersonalKanbanServices();
    }

    public INotificationObserver<TNotification> ObserveNotification<TNotification>() where TNotification : INotification
    {
        var notificationObserver = new GenericNotificationHandler<TNotification>();
        _services.AddTransient<INotificationHandler<TNotification>>(s => notificationObserver);
        return notificationObserver;
    }

    public IMediator BuildMediator()
    {
        var serviceProvider = _services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IMediator>();
    }

    private class GenericNotificationHandler<T> : INotificationHandler<T>, INotificationObserver<T> where T : INotification
    {
        public T? Notification { get; private set; }

        public Task Handle(T notification, CancellationToken cancellationToken)
        {
            Notification = notification;
            return Task.CompletedTask;
        }
    }
}
