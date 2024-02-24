using MediatR;

using Microsoft.Extensions.DependencyInjection;

using NEventStore;

using PersonalKanban;

namespace PersonalKanbanTest;

public interface INotificationObserver<TNotification> where TNotification : INotification
{
    TNotification? Notification { get; }
}

public class TestContext
{
    public static TestContextBuilder New()
    {
        return new TestContextBuilder();
    }

    private readonly IServiceProvider _services;
    private TestContext(IServiceProvider services)
    {
        _services = services;
    }


    public IMediator Mediator => _services.GetRequiredService<IMediator>();
    public IStoreEvents Store => _services.GetRequiredService<IStoreEvents>();
    public IServiceProvider Services => _services;

    public class TestContextBuilder
    {

        private readonly ServiceCollection _services = new ServiceCollection();

        public TestContextBuilder()
        {
            _services.AddPersonalKanbanServicesInMemory();
        }

        public INotificationObserver<TNotification> ObserveNotification<TNotification>() where TNotification : INotification
        {
            var notificationObserver = new GenericNotificationHandler<TNotification>();
            _services.AddTransient<INotificationHandler<TNotification>>(s => notificationObserver);
            return notificationObserver;
        }

        public TestContextBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            configure(_services);
            return this;
        }

        public TestContext Build()
        {
            var serviceProvider = _services.BuildServiceProvider();
            return new TestContext(serviceProvider);
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
}
