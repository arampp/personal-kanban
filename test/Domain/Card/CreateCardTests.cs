using MediatR;

using NEventStore;

using PersonalKanban.Domain.Board;
using PersonalKanban.Domain.Card;
using PersonalKanban.Domain.Column;

namespace PersonalKanbanTest.Domain.Card;

public class CreateCardTests : IAsyncLifetime
{
    private readonly INotificationObserver<BoardCreated> _boardCreated;
    private readonly INotificationObserver<ColumnCreated> _columnCreated;
    private readonly INotificationObserver<CardCreated> _cardCreated;
    private readonly IMediator _mediator;
    private readonly IStoreEvents _store;

    public CreateCardTests()
    {
        var contextBuilder = TestContext.New();
        _boardCreated = contextBuilder.ObserveNotification<BoardCreated>();
        _columnCreated = contextBuilder.ObserveNotification<ColumnCreated>();
        _cardCreated = contextBuilder.ObserveNotification<CardCreated>();
        _mediator = contextBuilder.Build().Mediator;
        _store = contextBuilder.Build().Store;

    }

    public async Task InitializeAsync()
    {
        await _mediator.Send(new CreateBoard("Title", "Description"));
        await _mediator.Send(new CreateColumn("Title", _boardCreated.Notification!.Id));
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Publishes_a_card_created_event()
    {

        await _mediator.Send(new CreateCard("Title", "Description", _columnCreated.Notification!.Id));

        var notification = _cardCreated.Notification!;
        notification.Title.Should().Be("Title");
        notification.Description.Should().Be("Description");
    }

    [Fact]
    public async Task Description_is_an_empty_string_if_it_was_null()
    {
        await _mediator.Send(new CreateCard("Title", null, _columnCreated.Notification!.Id));

        var notification = _cardCreated.Notification!;
        notification.Description.Should().Be("");
    }

    [Fact]
    public async Task Fails_if_title_is_empty()
    {
        var sendWithEmptyTitle = () => _mediator.Send(new CreateCard(" ", null, _columnCreated.Notification!.Id));

        await sendWithEmptyTitle
        .Should()
        .ThrowAsync<RequestFailed>()
        .WithMessage($"*${nameof(CardCreated.Title)}*");
    }

    [Fact]
    public async Task Saves_the_event_to_the_store()
    {
        await _mediator.Send(new CreateCard("Title", "Description", _columnCreated.Notification!.Id));

        var stream = _store.OpenStream("card", _cardCreated.Notification!.Id.ToString(), 0, int.MaxValue);
        stream.CommittedEvents.Count.Should().Be(1);
        stream.CommittedEvents.Single().Body.Should().BeEquivalentTo(_cardCreated.Notification);
    }

}