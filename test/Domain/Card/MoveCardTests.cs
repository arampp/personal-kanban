using MediatR;

using NEventStore;

using PersonalKanban.Domain.Board;
using PersonalKanban.Domain.Card;
using PersonalKanban.Domain.Column;

namespace PersonalKanbanTest.Domain.Card;

public class MoveCardTests : IAsyncLifetime
{
    private readonly INotificationObserver<BoardCreated> _boardCreated;
    private readonly INotificationObserver<ColumnCreated> _columnCreated;
    private readonly INotificationObserver<CardCreated> _cardCreated;
    private readonly INotificationObserver<CardMoved> _cardMoved;
    private readonly IMediator _mediator;
    private readonly IStoreEvents _store;
    private Guid _firstColumn;
    private Guid _secondColumn;
    private Guid _card;

    public MoveCardTests()
    {
        var contextBuilder = TestContext.New();
        _boardCreated = contextBuilder.ObserveNotification<BoardCreated>();
        _columnCreated = contextBuilder.ObserveNotification<ColumnCreated>();
        _cardCreated = contextBuilder.ObserveNotification<CardCreated>();
        _cardMoved = contextBuilder.ObserveNotification<CardMoved>();
        var testContext = contextBuilder.Build();
        _mediator = testContext.Mediator;
        _store = testContext.Store;
    }

    public async Task InitializeAsync()
    {
        await _mediator.Send(new CreateBoard("Title", "Description"));
        await _mediator.Send(new CreateColumn("First", _boardCreated.Notification!.Id));
        _firstColumn = _columnCreated.Notification!.Id;
        await _mediator.Send(new CreateColumn("Secord", _boardCreated.Notification!.Id));
        _secondColumn = _columnCreated.Notification!.Id;
        await _mediator.Send(new CreateCard("Card", "Description", _firstColumn));
        _card = _cardCreated.Notification!.Id;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Publishes_a_card_created_event()
    {
        await _mediator.Send(new MoveCard
        {
            Card = _card,
            SourceColumn = _firstColumn,
            TargetColumn = _secondColumn,
            Position = 0
        });

        var notification = _cardMoved.Notification!;
        notification.Card.Should().Be(_card);
        notification.SourceColumn.Should().Be(_firstColumn);
        notification.TargetColumn.Should().Be(_secondColumn);
        notification.Position.Should().Be(0);
    }

    [Fact]
    public async Task Position_is_set_to_null_if_negative()
    {
        await _mediator.Send(new MoveCard
        {
            Card = _card,
            SourceColumn = _firstColumn,
            TargetColumn = _secondColumn,
            Position = -5
        });

        var notification = _cardMoved.Notification!;
        notification.Position.Should().Be(0);
    }

    [Fact]
    public async Task Position_is_set_to_last_if_higher_than_length()
    {
        await _mediator.Send(new CreateCard("Card", "Description", _secondColumn));
        await _mediator.Send(new CreateCard("Card2", "Description", _secondColumn));
        await _mediator.Send(new MoveCard
        {
            Card = _card,
            SourceColumn = _firstColumn,
            TargetColumn = _secondColumn,
            Position = 30
        });

        var notification = _cardMoved.Notification!;
        notification.Position.Should().Be(2);
    }

    [Fact]
    public async Task Fails_if_card_does_not_exist()
    {
        var invalidRequest = () => _mediator.Send(
            new MoveCard
            {
                Card = Guid.NewGuid(),
                SourceColumn = _firstColumn,
                TargetColumn = _secondColumn,
                Position = 1
            });

        await invalidRequest
        .Should()
        .ThrowAsync<NotFound>()
        .WithMessage("*Card not found*");
    }


    [Fact]
    public async Task Fails_if_source_column_does_not_exist()
    {
        var invalidRequest = () => _mediator.Send(
            new MoveCard
            {
                Card = _card,
                SourceColumn = Guid.NewGuid(),
                TargetColumn = _secondColumn,
                Position = 1
            });

        await invalidRequest
        .Should()
        .ThrowAsync<NotFound>()
        .WithMessage("*SourceColumn not found*");
    }

    [Fact]
    public async Task Fails_if_target_column_does_not_exist()
    {
        var invalidRequest = () => _mediator.Send(
            new MoveCard
            {
                Card = _card,
                SourceColumn = _firstColumn,
                TargetColumn = Guid.NewGuid(),
                Position = 1
            });

        await invalidRequest
        .Should()
        .ThrowAsync<NotFound>()
        .WithMessage("*TargetColumn not found*");
    }

    [Fact]
    public async Task Saves_the_event_to_the_store()
    {
        await _mediator.Send(new MoveCard
        {
            Card = _card,
            SourceColumn = _firstColumn,
            TargetColumn = _secondColumn,
            Position = 1
        });

        var stream = _store.OpenStream("card", _card, 0, int.MaxValue);
        var actual = stream.CommittedEvents.Last().Body;
        actual.Should().BeOfType<CardMoved>().And.BeEquivalentTo(_cardMoved.Notification);
    }

}