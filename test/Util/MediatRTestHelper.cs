
using MediatR;

using Microsoft.Extensions.DependencyInjection;

using PersonalKanban;

namespace PersonalKanbanTest.Util;

internal class MediatRTestHelper {

    internal static MediatRSenarioBuilder<TRequest> SendRequest<TRequest>(TRequest request) where TRequest: IRequest {
        return new MediatRSenarioBuilder<TRequest>(request);
    }
}

internal class MediatRSenarioBuilder<TRequest> where TRequest: IRequest {
    private readonly TRequest _request;

    internal MediatRSenarioBuilder(TRequest request) {
        _request = request;
    }

    internal async Task<TNotification> AndExpectNotification<TNotification>() where TNotification: INotification
    {
        var services = new ServiceCollection();
        services.AddPersonalKanbanServices();
        var handler = new GenericNotificationHandler<TNotification>();
        services.AddTransient<INotificationHandler<TNotification>>(s => handler);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send(_request);
        if (handler.Notification == null) {
            throw new Exception("expected notification has not been sent");
        }

        return handler.Notification;
    }

    internal async Task<FluentAssertions.Specialized.ExceptionAssertions<TException>> AndExpectException<TException>() where TException: Exception
    {
        var services = new ServiceCollection();
        services.AddPersonalKanbanServices();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var send = async () => await mediator.Send(_request);

        return await send.Should().ThrowAsync<TException>();
    }

    private class GenericNotificationHandler<T>: INotificationHandler<T> where T: INotification
    {
        public T? Notification { get; private set; }

        public Task Handle(T notification, CancellationToken cancellationToken)
        {
            Notification = notification;
            return Task.CompletedTask;
        }
    }

}
