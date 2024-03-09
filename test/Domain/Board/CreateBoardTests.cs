using NEventStore;

using PersonalKanban.Domain.Board;

namespace PersonalKanbanTest.Domain.Board;

public class CreateBoardTests
{
    [Fact(Skip = "Temporary skipped")]
    public async void Publishes_a_Board_created_event_and_saves_it()
    {
        var testContextBuilder = TestContext.New();
        var boardCreated = testContextBuilder.ObserveNotification<BoardCreated>();
        var testContext = testContextBuilder.Build();

        await testContext.Mediator.Send(new CreateBoard("Title", "Description"));

        var notification = boardCreated.Notification!;
        notification.Title.Should().Be("Title");
        notification.Description.Should().Be("Description");
        using var stream = testContext.Store.OpenStream(notification.Id);
        stream.CommittedEvents.Single().Body.Should().Be(notification);
    }

    [Fact]
    public async void Description_is_an_empty_string_if_it_was_null()
    {
        var testContextBuilder = TestContext.New();
        var boardCreated = testContextBuilder.ObserveNotification<BoardCreated>();
        var testContext = testContextBuilder.Build();

        await testContext.Mediator.Send(new CreateBoard("Title", null));

        boardCreated.Notification!.Description.Should().Be("");
    }

    [Fact]
    public async void Fails_if_title_is_empty()
    {
        var testContextBuilder = TestContext.New();
        var boardCreated = testContextBuilder.ObserveNotification<BoardCreated>();
        var testContext = testContextBuilder.Build();

        var sendWithEmptyTitle = () => testContext.Mediator.Send(new CreateBoard(" ", null));

        await sendWithEmptyTitle
        .Should()
        .ThrowAsync<RequestFailed>()
        .WithMessage($"*${nameof(BoardCreated.Title)}*");
    }
}