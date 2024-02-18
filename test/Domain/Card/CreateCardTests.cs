using MediatR;

using PersonalKanban.Domain.Board;
using PersonalKanban.Domain.Card;
using PersonalKanban.Domain.Column;

using Xunit;

using static PersonalKanbanTest.Util.MediatRTestHelper;

namespace PersonalKanbanTest.Domain.Card;

public class CreateCardTests : IAsyncLifetime
{
    private readonly INotificationObserver<BoardCreated> _boardCreated;
    private readonly INotificationObserver<ColumnCreated> _columnCreated;
    private readonly INotificationObserver<CardCreated> _cardCreated;
    private readonly IMediator _mediator;

    public CreateCardTests()
    {
        var context = new TestContext();
        _boardCreated = context.ObserveNotification<BoardCreated>();
        _columnCreated = context.ObserveNotification<ColumnCreated>();
        _cardCreated = context.ObserveNotification<CardCreated>();
        _mediator = context.BuildMediator();

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
        await SendRequest(new CreateCard(" ", null, _columnCreated.Notification!.Id))
            .AndExpectException<RequestFailed>()
            .WithMessage($"*${nameof(CardCreated.Title)}*");
    }

}