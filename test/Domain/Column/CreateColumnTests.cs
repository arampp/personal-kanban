using PersonalKanban.Domain.Board;
using PersonalKanban.Domain.Column;

using static PersonalKanbanTest.Util.MediatRTestHelper;

namespace PersonalKanbanTest.Domain.Column;

public class CreateColumnTests
{
    [Fact]
    public async Task Publishes_a_Column_created_event()
    {
        var contextBuilder = TestContext.New();
        var boardCreated = contextBuilder.ObserveNotification<BoardCreated>();
        var columnCreated = contextBuilder.ObserveNotification<ColumnCreated>();

        var mediator = contextBuilder.Build().Mediator;
        await mediator.Send(new CreateBoard("Title", "Description"));


        var boardId = boardCreated.Notification!.Id;
        await mediator.Send(new CreateColumn("Title", boardId));

        columnCreated.Notification!.Title.Should().Be("Title");
        columnCreated.Notification!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Fails_if_board_does_not_exist()
    {
        var mediator = TestContext.New().Build().Mediator;

        var boardId = Guid.NewGuid();
        var send = async () => await mediator.Send(new CreateColumn("Title", boardId));

        await send.Should().ThrowAsync<RequestFailed>().WithMessage($"*{boardId}*");
    }

    [Fact]
    public async void Fails_if_title_is_empty()
    {
        var mediator = TestContext.New().Build().Mediator;
        var createWithEmptyTitle = () => mediator.Send(new CreateColumn(" ", Guid.NewGuid()));
        await createWithEmptyTitle.Should()
        .ThrowAsync<RequestFailed>()
        .WithMessage($"*${nameof(ColumnCreated.Title)}*");
    }
}